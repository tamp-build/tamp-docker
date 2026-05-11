using Xunit;

namespace Tamp.Docker.V27.Tests;

/// <summary>
/// Unit tests for <c>docker compose</c> verbs. These verify the verb
/// token ordering invariant (compose-shared flags BEFORE the subverb,
/// subverb-specific flags AFTER) and that each new settings class
/// surfaces the right CLI flags.
/// </summary>
public sealed class DockerComposeTests
{
    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++)
            if (args[i] == value) return i;
        return -1;
    }

    [Fact]
    public void Every_Compose_Verb_Begins_With_compose()
    {
        Assert.Equal("compose", Docker.Compose.Up(s => { }).Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Down().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.BuildImages().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Logs().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Ps().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Pull().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Push().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Exec(s => s.SetService("api").SetCommand("sh")).Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Run(s => s.SetService("api")).Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Config().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Start().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Stop().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Restart().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Kill().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Pause().Arguments[0]);
        Assert.Equal("compose", Docker.Compose.Unpause().Arguments[0]);
    }

    [Fact]
    public void Compose_Shared_Flags_Sit_Between_compose_And_Subverb()
    {
        var plan = Docker.Compose.Up(s => s
            .AddFile("docker-compose.yml")
            .AddFile("docker-compose.override.yml")
            .AddProfile("dev")
            .SetProjectName("smoke")
            .SetEnvFile(".env.ci"));
        var args = plan.Arguments;

        // compose must be first, up must come AFTER all -f/--profile/etc.
        Assert.Equal("compose", args[0]);
        var upIdx = IndexOf(args, "up");
        Assert.True(upIdx > 0);

        var fileIdx = IndexOf(args, "-f");
        var profileIdx = IndexOf(args, "--profile");
        var projectIdx = IndexOf(args, "--project-name");
        var envIdx = IndexOf(args, "--env-file");

        Assert.InRange(fileIdx, 1, upIdx - 1);
        Assert.InRange(profileIdx, 1, upIdx - 1);
        Assert.InRange(projectIdx, 1, upIdx - 1);
        Assert.InRange(envIdx, 1, upIdx - 1);
    }

    [Fact]
    public void Compose_Multiple_Files_Emit_As_Repeated_f_Flags()
    {
        var plan = Docker.Compose.Up(s => s
            .AddFile("base.yml")
            .AddFile("dotnet.yml")
            .AddFile("soak.yml"));
        var args = plan.Arguments;
        var first = IndexOf(args, "-f");
        var second = IndexOf(args, "-f", first + 1);
        var third = IndexOf(args, "-f", second + 1);
        Assert.True(first >= 0 && second > first && third > second);
        Assert.Equal("base.yml", args[first + 1]);
        Assert.Equal("dotnet.yml", args[second + 1]);
        Assert.Equal("soak.yml", args[third + 1]);
    }

    [Fact]
    public void Compose_Multiple_Profiles_Emit_As_Repeated_Flags()
    {
        var plan = Docker.Compose.Up(s => s.AddProfile("dev").AddProfile("debug"));
        var args = plan.Arguments;
        var first = IndexOf(args, "--profile");
        var second = IndexOf(args, "--profile", first + 1);
        Assert.True(first >= 0 && second > first);
        Assert.Equal("dev", args[first + 1]);
        Assert.Equal("debug", args[second + 1]);
    }

    [Fact]
    public void Compose_Up_All_Flags_Round_Trip()
    {
        var plan = Docker.Compose.Up(s => s
            .SetDetach()
            .SetBuild()
            .SetForceRecreate()
            .SetAlwaysRecreateDeps()
            .SetNoDeps()
            .SetAbortOnContainerExit()
            .SetExitCodeFrom("api")
            .SetWait()
            .SetWaitTimeout(60)
            .SetRemoveOrphans()
            .SetPull("always")
            .SetScale("worker", 3)
            .SetNoLogPrefix()
            .AddService("api")
            .AddService("worker"));
        var args = plan.Arguments;
        Assert.Contains("up", args);
        Assert.Contains("-d", args);
        Assert.Contains("--build", args);
        Assert.Contains("--force-recreate", args);
        Assert.Contains("--always-recreate-deps", args);
        Assert.Contains("--no-deps", args);
        Assert.Contains("--abort-on-container-exit", args);
        Assert.Contains("--exit-code-from", args);
        Assert.Contains("api", args);
        Assert.Contains("--wait", args);
        Assert.Contains("--wait-timeout", args);
        Assert.Contains("60", args);
        Assert.Contains("--remove-orphans", args);
        Assert.Contains("--pull", args);
        Assert.Contains("always", args);
        Assert.Contains("--scale", args);
        Assert.Contains("worker=3", args);
        Assert.Contains("--no-log-prefix", args);
        Assert.Equal("worker", args[^1]); // services tail the verb
    }

    [Fact]
    public void Compose_Down_With_Volumes_And_Rmi()
    {
        var plan = Docker.Compose.Down(s => s.SetVolumes().SetRmi("local").SetRemoveOrphans().SetTimeout(10));
        var args = plan.Arguments;
        Assert.Contains("down", args);
        Assert.Contains("--volumes", args);
        Assert.Contains("--rmi", args);
        Assert.Contains("local", args);
        Assert.Contains("--remove-orphans", args);
        Assert.Contains("--timeout", args);
        Assert.Contains("10", args);
    }

    [Fact]
    public void Compose_Build_Build_Args_Use_Key_Eq_Value()
    {
        var plan = Docker.Compose.BuildImages(s => s
            .SetNoCache()
            .SetPull()
            .SetParallel()
            .SetBuildArg("VERSION", "1.2.3")
            .AddService("api"));
        var args = plan.Arguments;
        Assert.Contains("build", args);
        Assert.Contains("--no-cache", args);
        Assert.Contains("--pull", args);
        Assert.Contains("--parallel", args);
        Assert.Contains("--build-arg", args);
        Assert.Contains("VERSION=1.2.3", args);
        Assert.Contains("api", args);
    }

    [Fact]
    public void Compose_Logs_Tail_And_Follow_Round_Trip()
    {
        var plan = Docker.Compose.Logs(s => s.SetFollow().SetTail(100).SetTimestamps().AddService("api"));
        var args = plan.Arguments;
        Assert.Contains("logs", args);
        Assert.Contains("-f", args);
        Assert.Contains("--tail", args);
        Assert.Contains("100", args);
        Assert.Contains("--timestamps", args);
        Assert.Equal("api", args[^1]);
    }

    [Fact]
    public void Compose_Ps_All_And_ServicesOnly()
    {
        var plan = Docker.Compose.Ps(s => s.SetAll().SetServicesListOnly().SetFormat("json"));
        var args = plan.Arguments;
        Assert.Contains("ps", args);
        Assert.Contains("--all", args);
        Assert.Contains("--services", args);
        Assert.Contains("--format", args);
        Assert.Contains("json", args);
    }

    [Fact]
    public void Compose_Exec_Requires_Service_And_Command()
    {
        Assert.Throws<InvalidOperationException>(() => Docker.Compose.Exec(s => { }));
        Assert.Throws<InvalidOperationException>(() => Docker.Compose.Exec(s => s.SetService("api")));
    }

    [Fact]
    public void Compose_Exec_Emits_Service_Then_Command_Then_Args()
    {
        var plan = Docker.Compose.Exec(s => s
            .SetService("api")
            .SetCommand("sh")
            .AddCommandArg("-c")
            .AddCommandArg("echo hi")
            .SetUser("ci")
            .SetWorkdir("/app")
            .SetEnv("FOO", "bar")
            .SetNoTty());
        var args = plan.Arguments;

        // service → command → command args, all at the tail
        Assert.Equal("api", args[^4]);
        Assert.Equal("sh", args[^3]);
        Assert.Equal("-c", args[^2]);
        Assert.Equal("echo hi", args[^1]);
        Assert.Contains("--user", args);
        Assert.Contains("ci", args);
        Assert.Contains("--workdir", args);
        Assert.Contains("/app", args);
        Assert.Contains("-e", args);
        Assert.Contains("FOO=bar", args);
        Assert.Contains("-T", args);
    }

    [Fact]
    public void Compose_Run_Requires_Service()
    {
        Assert.Throws<InvalidOperationException>(() => Docker.Compose.Run(s => { }));
    }

    [Fact]
    public void Compose_Run_Optional_Command_Omitted_When_Null()
    {
        var plan = Docker.Compose.Run(s => s.SetService("api").SetRm());
        var args = plan.Arguments;
        Assert.Equal("api", args[^1]); // service is last; no command
        Assert.Contains("--rm", args);
    }

    [Fact]
    public void Compose_Config_Hash_And_Output_Round_Trip()
    {
        var plan = Docker.Compose.Config(s => s.SetHash().SetServicesListOnly().SetOutput("config.yml"));
        var args = plan.Arguments;
        Assert.Contains("config", args);
        Assert.Contains("--hash", args);
        Assert.Contains("--services", args);
        Assert.Contains("--output", args);
        Assert.Contains("config.yml", args);
    }

    [Theory]
    [InlineData("start")]
    [InlineData("stop")]
    [InlineData("restart")]
    [InlineData("kill")]
    [InlineData("pause")]
    [InlineData("unpause")]
    public void Compose_Lifecycle_Subverbs_Round_Trip(string subverb)
    {
        var plan = subverb switch
        {
            "start" => Docker.Compose.Start(s => s.AddService("api")),
            "stop" => Docker.Compose.Stop(s => s.AddService("api").SetTimeout(5)),
            "restart" => Docker.Compose.Restart(s => s.AddService("api")),
            "kill" => Docker.Compose.Kill(s => s.AddService("api")),
            "pause" => Docker.Compose.Pause(s => s.AddService("api")),
            "unpause" => Docker.Compose.Unpause(s => s.AddService("api")),
            _ => throw new InvalidOperationException()
        };
        Assert.Contains(subverb, plan.Arguments);
        Assert.Equal("api", plan.Arguments[^1]);
    }

    [Fact]
    public void Compose_Lifecycle_Without_Subverb_Throws()
    {
        // Directly constructing the lifecycle settings and skipping Subverb
        // should fail at plan time. The facade methods always set a Subverb
        // so this is a defensive check on the settings class itself.
        var s = new DockerComposeLifecycleSettings();
        Assert.Throws<InvalidOperationException>(() => s.ToCommandPlan());
    }

    [Fact]
    public void Compose_DryRun_And_Progress_Are_Pre_Verb()
    {
        var plan = Docker.Compose.Up(s => s.SetDryRun().SetProgress("json"));
        var args = plan.Arguments;
        var upIdx = IndexOf(args, "up");
        var dryIdx = IndexOf(args, "--dry-run");
        var progressIdx = IndexOf(args, "--progress");
        Assert.InRange(dryIdx, 1, upIdx - 1);
        Assert.InRange(progressIdx, 1, upIdx - 1);
    }
}
