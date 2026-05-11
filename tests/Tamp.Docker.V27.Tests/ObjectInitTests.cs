using Xunit;

namespace Tamp.Docker.V27.Tests;

/// <summary>
/// Round-trip + smoke coverage for the object-init overloads added in 0.3.1
/// (TAM-161 satellite fanout). Each overload should yield a CommandPlan with
/// identical Arguments to its fluent twin, and every public overload must
/// at minimum compile and return non-null.
/// </summary>
public sealed class ObjectInitTests
{
    // ---- Round-trip: top-level Docker verb ----

    [Fact]
    public void Login_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var pwd = new Secret("RegistryPassword", "p@ssw0rd!");

        var fluent = Docker.Login(s => s
            .SetServer("registry.example.com")
            .SetUsername("ci")
            .SetPassword(pwd));

        var objectInit = Docker.Login(new DockerLoginSettings
        {
            Server = "registry.example.com",
            Username = "ci",
            Password = pwd,
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Build_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var fluent = Docker.Build(b => b
            .SetContext(".")
            .SetDockerfile("Dockerfile")
            .AddTag("acme/web:1.0")
            .AddPlatform("linux/amd64")
            .SetPush(true));

        var objectInit = Docker.Build(new DockerBuildxBuildSettings
        {
            Context = ".",
            Dockerfile = "Dockerfile",
            Tags = { "acme/web:1.0" },
            Platforms = { "linux/amd64" },
            Push = true,
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    // ---- Round-trip: Compose sub-facade ----

    [Fact]
    public void Compose_Up_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var fluent = Docker.Compose.Up(u => u
            .AddFile("compose.yaml")
            .SetProjectName("ci-stack")
            .SetDetach(true)
            .SetWait(true)
            .AddService("web")
            .AddService("db"));

        var objectInit = Docker.Compose.Up(new DockerComposeUpSettings
        {
            Files = { "compose.yaml" },
            ProjectName = "ci-stack",
            Detach = true,
            Wait = true,
            Services = { "web", "db" },
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    // ---- Smoke: every overload accepts a settings instance and returns a plan ----

    [Fact]
    public void Every_ObjectInit_Overload_Compiles_And_Returns_CommandPlan()
    {
        // Top-level Docker verbs.
        Assert.NotNull(Docker.Login(new DockerLoginSettings { Server = "ghcr.io" }));
        Assert.NotNull(Docker.Logout(new DockerLogoutSettings()));
        Assert.NotNull(Docker.Build(new DockerBuildxBuildSettings { Context = "." }));
        Assert.NotNull(Docker.LegacyBuild(new DockerLegacyBuildSettings { Context = "." }));
        Assert.NotNull(Docker.Tag(new DockerTagSettings { SourceImage = "a:1", TargetImage = "a:2" }));
        Assert.NotNull(Docker.Push(new DockerPushSettings { Image = "a:1" }));
        Assert.NotNull(Docker.Pull(new DockerPullSettings { Image = "a:1" }));

        // Compose sub-facade (lifecycle verbs intentionally absent — fluent-only).
        Assert.NotNull(Docker.Compose.Up(new DockerComposeUpSettings()));
        Assert.NotNull(Docker.Compose.Down(new DockerComposeDownSettings()));
        Assert.NotNull(Docker.Compose.BuildImages(new DockerComposeBuildSettings()));
        Assert.NotNull(Docker.Compose.Logs(new DockerComposeLogsSettings()));
        Assert.NotNull(Docker.Compose.Ps(new DockerComposePsSettings()));
        Assert.NotNull(Docker.Compose.Pull(new DockerComposePullSettings()));
        Assert.NotNull(Docker.Compose.Push(new DockerComposePushSettings()));
        Assert.NotNull(Docker.Compose.Exec(new DockerComposeExecSettings { Service = "web", Command = "sh" }));
        Assert.NotNull(Docker.Compose.Run(new DockerComposeRunSettings { Service = "web" }));
        Assert.NotNull(Docker.Compose.Config(new DockerComposeConfigSettings()));

        // Buildx sub-facade.
        Assert.NotNull(Docker.Buildx.Build(new DockerBuildxBuildSettings { Context = "." }));
        Assert.NotNull(Docker.Buildx.Bake(new DockerBuildxBakeSettings()));
        Assert.NotNull(Docker.Buildx.Create(new DockerBuildxCreateSettings()));
        Assert.NotNull(Docker.Buildx.Ls(new DockerBuildxLsSettings()));
        Assert.NotNull(Docker.Buildx.Use(new DockerBuildxUseSettings { BuilderName = "default" }));
        Assert.NotNull(Docker.Buildx.Inspect(new DockerBuildxInspectSettings()));
        Assert.NotNull(Docker.Buildx.Prune(new DockerBuildxPruneSettings()));
        Assert.NotNull(Docker.Buildx.Rm(new DockerBuildxRmSettings()));
        Assert.NotNull(Docker.Buildx.Stop(new DockerBuildxStopSettings()));
        Assert.NotNull(Docker.Buildx.Version(new DockerBuildxVersionSettings()));
    }
}
