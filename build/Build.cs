using Tamp;
using Tamp.NetCli.V10;

/// <summary>
/// tamp-docker's self-hosted build script. Drives the
/// restore / build / test / pack / push pipeline through Tamp itself.
/// </summary>
class Build : TampBuild
{
    public static int Main(string[] args) => Execute<Build>(args);

    [Parameter("Build configuration")]
    Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Package version override (resolved from CI tag, e.g. v0.1.0 → 0.1.0)", EnvironmentVariable = "PACKAGE_VERSION")]
#pragma warning disable CS0649 // Set by reflection via [Parameter] binding.
    readonly string? Version;
#pragma warning restore CS0649

    [Solution] readonly Solution Solution = null!;
    [GitRepository] readonly GitRepository Git = null!;

    // Tamp.Core 1.0.0's [Secret] resolver isn't wired up yet (TAM-78).
    // Read NUGET_API_KEY directly until 1.0.1 lands.
    static readonly Secret? NuGetApiKey =
        Environment.GetEnvironmentVariable("NUGET_API_KEY") is { Length: > 0 } v
            ? new Secret("NuGet API key", v)
            : null;

    AbsolutePath Artifacts => RootDirectory / "artifacts";

    Target Info => _ => _
        .Description("Print build context — useful at the top of CI logs.")
        .Executes(() =>
        {
            Console.WriteLine($"  Branch:        {Git.Branch ?? "<detached>"}");
            Console.WriteLine($"  Commit:        {Git.Commit[..7]}");
            Console.WriteLine($"  Configuration: {Configuration}");
            Console.WriteLine($"  Solution:      {Solution.Name} ({Solution.Projects.Count} projects)");
            Console.WriteLine($"  Local build:   {IsLocalBuild}");
        });

    Target Clean => _ => _
        .TopLevel()
        .Description("Delete bin/obj and the artifacts directory.")
        .Executes(() =>
        {
            foreach (var d in RootDirectory.GlobDirectories("**/bin", "**/obj"))
                d.Delete();
            Artifacts.Delete();
        });

    Target Restore => _ => _
        .Executes(() => DotNet.Restore(s => s.SetProject(Solution.Path)));

    Target Compile => _ => _
        .TopLevel()
        .DependsOn(nameof(Restore))
        .Executes(() => DotNet.Build(s => s
            .SetProject(Solution.Path)
            .SetConfiguration(Configuration)
            .SetNoRestore(true)));

    Target Test => _ => _
        .TopLevel()
        .DependsOn(nameof(Compile))
        .Description("Run the unit test suite.")
        .Executes(() => DotNet.Test(s => s
            .SetProject(RootDirectory / "tests" / "Tamp.Docker.V27.Tests" / "Tamp.Docker.V27.Tests.csproj")
            .SetConfiguration(Configuration)
            .SetNoBuild(true)
            .AddLogger("trx;LogFileName=test-results.trx")
            .AddDataCollector("XPlat Code Coverage")
            .SetSettings((RootDirectory / "build" / "coverlet.runsettings").Value)
            .SetResultsDirectory(Artifacts / "test-results")));

    Target Pack => _ => _
        .TopLevel()
        .DependsOn(nameof(Test))
        .Description("Pack Tamp.Docker.V27 into ./artifacts.")
        .Executes(() => DotNet.Pack(s =>
        {
            s.SetProject(RootDirectory / "src" / "Tamp.Docker.V27" / "Tamp.Docker.V27.csproj");
            s.SetConfiguration(Configuration);
            s.SetNoBuild(true);
            s.SetOutput(Artifacts);
            if (!string.IsNullOrEmpty(Version)) s.SetProperty("Version", Version);
        }));

    Target Push => _ => _
        .TopLevel()
        .DependsOn(nameof(Pack))
        .Description("Push every nupkg to nuget.org. Tag-driven CI.")
        .Requires(() => NuGetApiKey != null)
        .Executes(() => Artifacts.GlobFiles("*.nupkg")
            .Select(p => DotNet.NuGetPush(s => s
                .SetPackagePath(p)
                .SetSource("https://api.nuget.org/v3/index.json")
                .SetApiKey(NuGetApiKey!)
                .SetSkipDuplicate(true))));

    Target Ci => _ => _
        .TopLevel()
        .DependsOn(nameof(Info), nameof(Clean), nameof(Pack))
        .Description("Full CI pipeline: print info, clean, restore, build, test, pack.");

    Target Default => _ => _
        .DependsOn(nameof(Compile))
        .Description("Local-developer default: restore + build the solution.");

    // ----- Sonar (TAM-17) -----

    [NuGetPackage("dotnet-sonarscanner", Version = "10.4.1")]
    readonly Tool SonarTool = null!;

    static readonly Secret? SonarToken =
        Environment.GetEnvironmentVariable("SONAR_TOKEN") is { Length: > 0 } v
            ? new Secret("SonarQube token", v)
            : null;

    [Parameter("Sonar host URL", EnvironmentVariable = "SONAR_HOST_URL")]
    readonly string SonarHostUrl = "https://sonar.brewingcoder.com";

    [Parameter("Sonar project key")]
    readonly string SonarProjectKey = "tamp-build_tamp-docker";

    Target SonarBegin => _ => _
        .Description("Initialize the SonarScanner pre-build phase.")
        .Before(nameof(Compile))
        .Requires(() => SonarToken != null)
        .Executes(() => Tamp.SonarScanner.V10.SonarScanner.Begin(SonarTool, s =>
        {
            s.SetProjectKey(SonarProjectKey);
            s.SetHostUrl(SonarHostUrl);
            s.SetToken(SonarToken!);
            s.SetProperty("sonar.cs.vstest.reportsPaths", $"{(Artifacts / "test-results").Value}/**/*.trx");
            s.SetProperty("sonar.cs.opencover.reportsPaths", $"{(Artifacts / "test-results").Value}/**/coverage.opencover.xml");

            s.SetProperty("sonar.coverage.exclusions", "tests/**,build/**,samples/**");

            s.SetProperty("sonar.exclusions", "**/bin/**,**/obj/**,artifacts/**,build/**,docs/**,samples/**");
        }));

    Target SonarEnd => _ => _
        .Description("Finalize SonarScanner and submit results to the server.")
        .DependsOn(nameof(Test))
        .Requires(() => SonarToken != null)
        .Executes(() => Tamp.SonarScanner.V10.SonarScanner.End(SonarTool, s => s.SetToken(SonarToken!)));

    Target Sonar => _ => _
        .TopLevel()
        .DependsOn(nameof(SonarBegin), nameof(SonarEnd))
        .Description("End-to-end Sonar scan: Begin (before Compile) → Compile → Test → End. Requires SONAR_TOKEN.");

}
