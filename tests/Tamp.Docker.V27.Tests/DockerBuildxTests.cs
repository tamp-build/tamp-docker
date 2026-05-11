using Xunit;

namespace Tamp.Docker.V27.Tests;

/// <summary>
/// Unit tests for <c>docker buildx</c> verbs — Bx prefix on each test
/// name to keep them visually distinct from compose tests.
/// </summary>
public sealed class DockerBuildxTests
{
    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++)
            if (args[i] == value) return i;
        return -1;
    }

    [Fact]
    public void Bx_Every_Verb_Begins_With_buildx()
    {
        Assert.Equal("buildx", Docker.Buildx.Build(s => { }).Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Bake().Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Create().Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Ls().Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Use(s => s.SetBuilderName("rig")).Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Inspect().Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Prune().Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Rm(s => s.AddBuilderName("rig")).Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Stop().Arguments[0]);
        Assert.Equal("buildx", Docker.Buildx.Version().Arguments[0]);
    }

    [Fact]
    public void Bx_Builder_Flag_Sits_Before_Subverb()
    {
        var plan = Docker.Buildx.Build(s => s.Builder = "rig");
        var args = plan.Arguments;
        var builderIdx = IndexOf(args, "--builder");
        var buildIdx = IndexOf(args, "build");
        Assert.InRange(builderIdx, 1, buildIdx - 1);
        Assert.Equal("rig", args[builderIdx + 1]);
    }

    [Fact]
    public void Bx_Build_Multiplatform_Joins_With_Commas()
    {
        var plan = Docker.Buildx.Build(s => s
            .SetContext(".")
            .AddPlatform("linux/amd64")
            .AddPlatform("linux/arm64"));
        var args = plan.Arguments;
        var platIdx = IndexOf(args, "--platform");
        Assert.Equal("linux/amd64,linux/arm64", args[platIdx + 1]);
    }

    [Fact]
    public void Bx_Build_Push_Load_Cache_All_Round_Trip()
    {
        var plan = Docker.Buildx.Build(s => s
            .SetContext(".")
            .AddTag("img:1")
            .AddCacheFrom("type=registry,ref=img:cache")
            .AddCacheTo("type=registry,ref=img:cache,mode=max")
            .AddOutput("type=registry")
            .SetPush()
            .SetMetadata("metadata.json")
            .SetSbom("true")
            .SetProvenance("mode=max"));
        var args = plan.Arguments;
        Assert.Contains("build", args);
        Assert.Contains("--tag", args);
        Assert.Contains("img:1", args);
        Assert.Contains("--cache-from", args);
        Assert.Contains("type=registry,ref=img:cache", args);
        Assert.Contains("--cache-to", args);
        Assert.Contains("type=registry,ref=img:cache,mode=max", args);
        Assert.Contains("--output", args);
        Assert.Contains("type=registry", args);
        Assert.Contains("--push", args);
        Assert.Contains("--metadata-file", args);
        Assert.Contains("metadata.json", args);
        Assert.Contains("--sbom", args);
        Assert.Contains("--provenance", args);
        Assert.Equal(".", args[^1]);
    }

    [Fact]
    public void Bx_Build_Secret_Emits_Id_Equals_Form()
    {
        var plan = Docker.Buildx.Build(s => s
            .SetContext(".")
            .SetSecret("npmrc", "src=./.npmrc"));
        var args = plan.Arguments;
        var secretIdx = IndexOf(args, "--secret");
        Assert.True(secretIdx >= 0);
        Assert.Equal("id=npmrc,src=./.npmrc", args[secretIdx + 1]);
    }

    [Fact]
    public void Bx_Bake_Targets_Tail_The_Verb()
    {
        var plan = Docker.Buildx.Bake(s => s
            .AddFile("docker-bake.hcl")
            .SetOverride("default.tags", "img:1")
            .AddTarget("default")
            .AddTarget("dev"));
        var args = plan.Arguments;
        Assert.Contains("bake", args);
        Assert.Contains("--file", args);
        Assert.Contains("docker-bake.hcl", args);
        Assert.Contains("--set", args);
        Assert.Contains("default.tags=img:1", args);
        Assert.Equal("dev", args[^1]);
        Assert.Equal("default", args[^2]);
    }

    [Fact]
    public void Bx_Create_With_Driver_Opts_And_Endpoints()
    {
        var plan = Docker.Buildx.Create(s => s
            .SetName("rig")
            .SetDriver("docker-container")
            .SetDriverOpt("image", "moby/buildkit:v0.13.0")
            .SetUse()
            .SetBootstrap()
            .AddEndpoint("unix:///var/run/docker.sock"));
        var args = plan.Arguments;
        Assert.Contains("create", args);
        Assert.Contains("--name", args);
        Assert.Contains("rig", args);
        Assert.Contains("--driver", args);
        Assert.Contains("docker-container", args);
        Assert.Contains("--driver-opt", args);
        Assert.Contains("image=moby/buildkit:v0.13.0", args);
        Assert.Contains("--use", args);
        Assert.Contains("--bootstrap", args);
        Assert.Equal("unix:///var/run/docker.sock", args[^1]);
    }

    [Fact]
    public void Bx_Ls_Format_Round_Trips()
    {
        var plan = Docker.Buildx.Ls(s => s.SetFormat("json").SetNoTrunc());
        Assert.Contains("ls", plan.Arguments);
        Assert.Contains("--format", plan.Arguments);
        Assert.Contains("json", plan.Arguments);
        Assert.Contains("--no-trunc", plan.Arguments);
    }

    [Fact]
    public void Bx_Use_Requires_Builder_Name()
    {
        Assert.Throws<InvalidOperationException>(() => Docker.Buildx.Use(s => { }));
    }

    [Fact]
    public void Bx_Use_With_Default_And_Global()
    {
        var plan = Docker.Buildx.Use(s => s.SetBuilderName("rig").SetDefault().SetGlobal());
        var args = plan.Arguments;
        Assert.Contains("--default", args);
        Assert.Contains("--global", args);
        Assert.Equal("rig", args[^1]);
    }

    [Fact]
    public void Bx_Inspect_Optional_Builder_Tails_When_Present()
    {
        var plan = Docker.Buildx.Inspect(s => s.SetBuilderName("rig").SetBootstrap());
        Assert.Contains("inspect", plan.Arguments);
        Assert.Contains("--bootstrap", plan.Arguments);
        Assert.Equal("rig", plan.Arguments[^1]);
    }

    [Fact]
    public void Bx_Inspect_Without_Builder_Just_Emits_inspect()
    {
        var plan = Docker.Buildx.Inspect();
        Assert.Equal("inspect", plan.Arguments[^1]);
    }

    [Fact]
    public void Bx_Prune_Filters_And_Keep_Storage()
    {
        var plan = Docker.Buildx.Prune(s => s
            .SetAll()
            .SetForce()
            .SetKeepStorage("10gb")
            .SetFilter("type", "regular"));
        var args = plan.Arguments;
        Assert.Contains("prune", args);
        Assert.Contains("--all", args);
        Assert.Contains("--force", args);
        Assert.Contains("--keep-storage", args);
        Assert.Contains("10gb", args);
        Assert.Contains("--filter", args);
        Assert.Contains("type=regular", args);
    }

    [Fact]
    public void Bx_Rm_Multiple_Builders_Tail_The_Verb()
    {
        var plan = Docker.Buildx.Rm(s => s
            .AddBuilderName("rig-a")
            .AddBuilderName("rig-b")
            .SetForce());
        var args = plan.Arguments;
        Assert.Contains("rm", args);
        Assert.Contains("--force", args);
        Assert.Equal("rig-a", args[^2]);
        Assert.Equal("rig-b", args[^1]);
    }

    [Fact]
    public void Bx_Stop_Optional_Builder()
    {
        var withName = Docker.Buildx.Stop(s => s.SetBuilderName("rig"));
        Assert.Equal("rig", withName.Arguments[^1]);

        var bare = Docker.Buildx.Stop();
        Assert.Equal("stop", bare.Arguments[^1]);
    }

    [Fact]
    public void Bx_Version_Round_Trips()
    {
        var plan = Docker.Buildx.Version();
        Assert.Equal(["buildx", "version"], plan.Arguments);
    }

    // ---- Docker.Build now routes to BuildKit (TAM-117, 0.3.0 breaking change) ----

    [Fact]
    public void Build_Routes_To_Buildx_Build()
    {
        var plan = Docker.Build(s => s.SetContext("."));
        Assert.Equal("buildx", plan.Arguments[0]);
        Assert.Equal("build", plan.Arguments[1]);
    }

    [Fact]
    public void Build_Accepts_DockerBuildxBuildSettings_Surface()
    {
        // Compile-time check: the configurer takes DockerBuildxBuildSettings, not the legacy class.
        // We exercise buildx-only flags (AddPlatform, SetPush, AddCacheFrom) to prove the binding.
        var plan = Docker.Build(s => s
            .SetContext(".")
            .SetDockerfile("./Dockerfile")
            .AddPlatform("linux/amd64")
            .AddPlatform("linux/arm64")
            .SetPush(true)
            .AddCacheFrom("type=registry,ref=acme/cache"));

        Assert.Contains("--platform", plan.Arguments);
        Assert.Contains("--push", plan.Arguments);
        Assert.Contains("--cache-from", plan.Arguments);
    }

    [Fact]
    public void Build_Rejects_Null_Configurer()
    {
        Assert.Throws<ArgumentNullException>(() => Docker.Build(null!));
    }

    [Fact]
    public void Build_Is_Reference_Equivalent_To_Buildx_Build()
    {
        // The new Docker.Build is a thin forwarder; calling either should emit the same plan
        // for the same settings.
        var direct = Docker.Buildx.Build(s => s.SetContext(".").AddTag("foo:1"));
        var routed = Docker.Build(s => s.SetContext(".").AddTag("foo:1"));
        Assert.Equal(direct.Arguments, routed.Arguments);
        Assert.Equal(direct.Executable, routed.Executable);
    }

    [Fact]
    public void LegacyBuild_Still_Emits_Pre_BuildKit_Build()
    {
        // Sanity: the legacy entry point still produces a plain `docker build` (no `buildx`).
        var plan = Docker.LegacyBuild(s => s.SetContext("."));
        Assert.Equal("build", plan.Arguments[0]);
        Assert.DoesNotContain("buildx", plan.Arguments);
    }
}
