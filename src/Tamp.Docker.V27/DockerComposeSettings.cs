namespace Tamp.Docker.V27;

/// <summary>
/// Common base for <c>docker compose &lt;verb&gt;</c> settings. Compose's
/// shared flags (<c>-f</c>, <c>--profile</c>, <c>--project-name</c>,
/// <c>--project-directory</c>, <c>--env-file</c>) come BEFORE the verb
/// — they sit between <c>compose</c> and the subcommand.
/// </summary>
public abstract class DockerComposeBaseSettings : DockerSettingsBase
{
    /// <summary>Compose file paths. Repeated as <c>-f &lt;file&gt;</c>. HoldFast's overlay pattern: base + hobby-dotnet + soak.</summary>
    public List<string> Files { get; } = [];

    /// <summary>Profile names to activate. Repeated as <c>--profile &lt;name&gt;</c>.</summary>
    public List<string> Profiles { get; } = [];

    /// <summary>Override the compose project name. Maps to <c>--project-name</c>.</summary>
    public string? ProjectName { get; set; }

    /// <summary>Override the project working directory. Maps to <c>--project-directory</c>.</summary>
    public string? ProjectDirectory { get; set; }

    /// <summary>Path to a <c>.env</c> file. Maps to <c>--env-file</c>.</summary>
    public string? EnvFile { get; set; }

    /// <summary>Compatibility mode for v1 compose files. Maps to <c>--compatibility</c>.</summary>
    public bool Compatibility { get; set; }

    /// <summary>Dry-run mode at the compose level. Maps to <c>--dry-run</c>.</summary>
    public bool DryRun { get; set; }

    /// <summary>Progress output style (<c>auto</c>, <c>tty</c>, <c>plain</c>, <c>json</c>). Maps to <c>--progress</c>.</summary>
    public string? Progress { get; set; }

    /// <summary>Subclasses produce the per-verb tokens (e.g. <c>up</c>, <c>down</c>) and any verb-specific flags.</summary>
    protected abstract IEnumerable<string> BuildSubVerbArguments();

    protected override IEnumerable<string> BuildVerbArguments()
    {
        yield return "compose";
        foreach (var f in Files) { yield return "-f"; yield return f; }
        foreach (var p in Profiles) { yield return "--profile"; yield return p; }
        if (!string.IsNullOrEmpty(ProjectName)) { yield return "--project-name"; yield return ProjectName!; }
        if (!string.IsNullOrEmpty(ProjectDirectory)) { yield return "--project-directory"; yield return ProjectDirectory!; }
        if (!string.IsNullOrEmpty(EnvFile)) { yield return "--env-file"; yield return EnvFile!; }
        if (Compatibility) yield return "--compatibility";
        if (DryRun) yield return "--dry-run";
        if (!string.IsNullOrEmpty(Progress)) { yield return "--progress"; yield return Progress!; }
        foreach (var a in BuildSubVerbArguments()) yield return a;
    }
}

/// <summary>Fluent setters for the common compose knobs.</summary>
public static class DockerComposeBaseSettingsExtensions
{
    public static T AddFile<T>(this T s, string path) where T : DockerComposeBaseSettings { s.Files.Add(path); return s; }
    public static T AddProfile<T>(this T s, string name) where T : DockerComposeBaseSettings { s.Profiles.Add(name); return s; }
    public static T SetProjectName<T>(this T s, string? name) where T : DockerComposeBaseSettings { s.ProjectName = name; return s; }
    public static T SetProjectDirectory<T>(this T s, string? dir) where T : DockerComposeBaseSettings { s.ProjectDirectory = dir; return s; }
    public static T SetEnvFile<T>(this T s, string? path) where T : DockerComposeBaseSettings { s.EnvFile = path; return s; }
    public static T SetCompatibility<T>(this T s, bool v = true) where T : DockerComposeBaseSettings { s.Compatibility = v; return s; }
    public static T SetDryRun<T>(this T s, bool v = true) where T : DockerComposeBaseSettings { s.DryRun = v; return s; }
    public static T SetProgress<T>(this T s, string? style) where T : DockerComposeBaseSettings { s.Progress = style; return s; }
}

/// <summary>Settings for <c>docker compose up</c>.</summary>
public sealed class DockerComposeUpSettings : DockerComposeBaseSettings
{
    /// <summary>Detached mode. Maps to <c>-d</c>.</summary>
    public bool Detach { get; set; }
    /// <summary>Build images before starting. Maps to <c>--build</c>.</summary>
    public bool Build { get; set; }
    /// <summary>Don't build (overrides per-service policy). Maps to <c>--no-build</c>.</summary>
    public bool NoBuild { get; set; }
    /// <summary>Force-recreate containers. Maps to <c>--force-recreate</c>.</summary>
    public bool ForceRecreate { get; set; }
    /// <summary>Don't recreate even if config changed. Maps to <c>--no-recreate</c>.</summary>
    public bool NoRecreate { get; set; }
    /// <summary>Recreate dependent containers. Maps to <c>--always-recreate-deps</c>.</summary>
    public bool AlwaysRecreateDeps { get; set; }
    /// <summary>Don't start linked services. Maps to <c>--no-deps</c>.</summary>
    public bool NoDeps { get; set; }
    /// <summary>Stop all containers if any one exits. Maps to <c>--abort-on-container-exit</c>.</summary>
    public bool AbortOnContainerExit { get; set; }
    /// <summary>Return the named container's exit code. Maps to <c>--exit-code-from</c>.</summary>
    public string? ExitCodeFrom { get; set; }
    /// <summary>Wait for services to become healthy. Maps to <c>--wait</c>.</summary>
    public bool Wait { get; set; }
    /// <summary>Wait timeout (seconds). Maps to <c>--wait-timeout</c>.</summary>
    public int? WaitTimeout { get; set; }
    /// <summary>Remove containers for services not in the compose file. Maps to <c>--remove-orphans</c>.</summary>
    public bool RemoveOrphans { get; set; }
    /// <summary>Pull policy: <c>missing</c> (default), <c>always</c>, <c>never</c>. Maps to <c>--pull</c>.</summary>
    public string? Pull { get; set; }
    /// <summary>Scale service to N instances. Repeated as <c>--scale &lt;service&gt;=&lt;count&gt;</c>.</summary>
    public Dictionary<string, int> Scale { get; } = new();
    /// <summary>Stream logs without prefix. Maps to <c>--no-log-prefix</c>.</summary>
    public bool NoLogPrefix { get; set; }
    /// <summary>Service names to start. Empty = all services.</summary>
    public List<string> Services { get; } = [];

    public DockerComposeUpSettings SetDetach(bool v = true) { Detach = v; return this; }
    public DockerComposeUpSettings SetBuild(bool v = true) { Build = v; return this; }
    public DockerComposeUpSettings SetNoBuild(bool v = true) { NoBuild = v; return this; }
    public DockerComposeUpSettings SetForceRecreate(bool v = true) { ForceRecreate = v; return this; }
    public DockerComposeUpSettings SetNoRecreate(bool v = true) { NoRecreate = v; return this; }
    public DockerComposeUpSettings SetAlwaysRecreateDeps(bool v = true) { AlwaysRecreateDeps = v; return this; }
    public DockerComposeUpSettings SetNoDeps(bool v = true) { NoDeps = v; return this; }
    public DockerComposeUpSettings SetAbortOnContainerExit(bool v = true) { AbortOnContainerExit = v; return this; }
    public DockerComposeUpSettings SetExitCodeFrom(string? service) { ExitCodeFrom = service; return this; }
    public DockerComposeUpSettings SetWait(bool v = true) { Wait = v; return this; }
    public DockerComposeUpSettings SetWaitTimeout(int seconds) { WaitTimeout = seconds; return this; }
    public DockerComposeUpSettings SetRemoveOrphans(bool v = true) { RemoveOrphans = v; return this; }
    public DockerComposeUpSettings SetPull(string? policy) { Pull = policy; return this; }
    public DockerComposeUpSettings SetScale(string service, int count) { Scale[service] = count; return this; }
    public DockerComposeUpSettings SetNoLogPrefix(bool v = true) { NoLogPrefix = v; return this; }
    public DockerComposeUpSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "up";
        if (Detach) yield return "-d";
        if (Build) yield return "--build";
        if (NoBuild) yield return "--no-build";
        if (ForceRecreate) yield return "--force-recreate";
        if (NoRecreate) yield return "--no-recreate";
        if (AlwaysRecreateDeps) yield return "--always-recreate-deps";
        if (NoDeps) yield return "--no-deps";
        if (AbortOnContainerExit) yield return "--abort-on-container-exit";
        if (!string.IsNullOrEmpty(ExitCodeFrom)) { yield return "--exit-code-from"; yield return ExitCodeFrom!; }
        if (Wait) yield return "--wait";
        if (WaitTimeout is { } wt) { yield return "--wait-timeout"; yield return wt.ToString(); }
        if (RemoveOrphans) yield return "--remove-orphans";
        if (!string.IsNullOrEmpty(Pull)) { yield return "--pull"; yield return Pull!; }
        if (NoLogPrefix) yield return "--no-log-prefix";
        foreach (var (svc, n) in Scale) { yield return "--scale"; yield return $"{svc}={n}"; }
        foreach (var s in Services) yield return s;
    }
}

/// <summary>Settings for <c>docker compose down</c>.</summary>
public sealed class DockerComposeDownSettings : DockerComposeBaseSettings
{
    /// <summary>Remove named volumes declared in the <c>volumes</c> section. Maps to <c>--volumes</c>.</summary>
    public bool Volumes { get; set; }
    /// <summary>Remove images. <c>all</c> or <c>local</c>. Maps to <c>--rmi</c>.</summary>
    public string? Rmi { get; set; }
    /// <summary>Remove containers for services not in the compose file. Maps to <c>--remove-orphans</c>.</summary>
    public bool RemoveOrphans { get; set; }
    /// <summary>Shutdown timeout (seconds). Maps to <c>--timeout</c>.</summary>
    public int? Timeout { get; set; }
    /// <summary>Service names to stop. Empty = all.</summary>
    public List<string> Services { get; } = [];

    public DockerComposeDownSettings SetVolumes(bool v = true) { Volumes = v; return this; }
    public DockerComposeDownSettings SetRmi(string mode) { Rmi = mode; return this; }
    public DockerComposeDownSettings SetRemoveOrphans(bool v = true) { RemoveOrphans = v; return this; }
    public DockerComposeDownSettings SetTimeout(int seconds) { Timeout = seconds; return this; }
    public DockerComposeDownSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "down";
        if (Volumes) yield return "--volumes";
        if (!string.IsNullOrEmpty(Rmi)) { yield return "--rmi"; yield return Rmi!; }
        if (RemoveOrphans) yield return "--remove-orphans";
        if (Timeout is { } t) { yield return "--timeout"; yield return t.ToString(); }
        foreach (var s in Services) yield return s;
    }
}

/// <summary>Settings for <c>docker compose build</c>.</summary>
public sealed class DockerComposeBuildSettings : DockerComposeBaseSettings
{
    /// <summary>Don't use cache. Maps to <c>--no-cache</c>.</summary>
    public bool NoCache { get; set; }
    /// <summary>Always attempt to pull a newer base image. Maps to <c>--pull</c>.</summary>
    public bool Pull { get; set; }
    /// <summary>Build in parallel. Maps to <c>--parallel</c>.</summary>
    public bool Parallel { get; set; }
    /// <summary>Push built images. Maps to <c>--push</c>.</summary>
    public bool Push { get; set; }
    /// <summary>Build args (<c>--build-arg KEY=VALUE</c>, repeated).</summary>
    public Dictionary<string, string> BuildArgs { get; } = new();
    /// <summary>Service names to build. Empty = all.</summary>
    public List<string> Services { get; } = [];

    public DockerComposeBuildSettings SetNoCache(bool v = true) { NoCache = v; return this; }
    public DockerComposeBuildSettings SetPull(bool v = true) { Pull = v; return this; }
    public DockerComposeBuildSettings SetParallel(bool v = true) { Parallel = v; return this; }
    public DockerComposeBuildSettings SetPush(bool v = true) { Push = v; return this; }
    public DockerComposeBuildSettings SetBuildArg(string key, string value) { BuildArgs[key] = value; return this; }
    public DockerComposeBuildSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "build";
        if (NoCache) yield return "--no-cache";
        if (Pull) yield return "--pull";
        if (Parallel) yield return "--parallel";
        if (Push) yield return "--push";
        foreach (var (k, v) in BuildArgs) { yield return "--build-arg"; yield return $"{k}={v}"; }
        foreach (var s in Services) yield return s;
    }
}

/// <summary>Settings for <c>docker compose logs</c>.</summary>
public sealed class DockerComposeLogsSettings : DockerComposeBaseSettings
{
    public bool Follow { get; set; }
    public bool Timestamps { get; set; }
    public string? Tail { get; set; }
    public bool NoLogPrefix { get; set; }
    public bool NoColor { get; set; }
    public string? Since { get; set; }
    public string? Until { get; set; }
    public List<string> Services { get; } = [];

    public DockerComposeLogsSettings SetFollow(bool v = true) { Follow = v; return this; }
    public DockerComposeLogsSettings SetTimestamps(bool v = true) { Timestamps = v; return this; }
    public DockerComposeLogsSettings SetTail(string spec) { Tail = spec; return this; }
    public DockerComposeLogsSettings SetTail(int n) { Tail = n.ToString(); return this; }
    public DockerComposeLogsSettings SetNoLogPrefix(bool v = true) { NoLogPrefix = v; return this; }
    public DockerComposeLogsSettings SetNoColor(bool v = true) { NoColor = v; return this; }
    public DockerComposeLogsSettings SetSince(string when) { Since = when; return this; }
    public DockerComposeLogsSettings SetUntil(string when) { Until = when; return this; }
    public DockerComposeLogsSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "logs";
        if (Follow) yield return "-f";
        if (Timestamps) yield return "--timestamps";
        if (!string.IsNullOrEmpty(Tail)) { yield return "--tail"; yield return Tail!; }
        if (NoLogPrefix) yield return "--no-log-prefix";
        if (NoColor) yield return "--no-color";
        if (!string.IsNullOrEmpty(Since)) { yield return "--since"; yield return Since!; }
        if (!string.IsNullOrEmpty(Until)) { yield return "--until"; yield return Until!; }
        foreach (var s in Services) yield return s;
    }
}

/// <summary>Settings for <c>docker compose ps</c>.</summary>
public sealed class DockerComposePsSettings : DockerComposeBaseSettings
{
    public bool All { get; set; }
    public bool Quiet { get; set; }
    public string? Format { get; set; }
    public string? Status { get; set; }
    public bool Services_ListOnly { get; set; }
    public List<string> Services { get; } = [];

    public DockerComposePsSettings SetAll(bool v = true) { All = v; return this; }
    public DockerComposePsSettings SetQuiet(bool v = true) { Quiet = v; return this; }
    public DockerComposePsSettings SetFormat(string format) { Format = format; return this; }
    public DockerComposePsSettings SetStatus(string status) { Status = status; return this; }
    public DockerComposePsSettings SetServicesListOnly(bool v = true) { Services_ListOnly = v; return this; }
    public DockerComposePsSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "ps";
        if (All) yield return "--all";
        if (Quiet) yield return "--quiet";
        if (!string.IsNullOrEmpty(Format)) { yield return "--format"; yield return Format!; }
        if (!string.IsNullOrEmpty(Status)) { yield return "--status"; yield return Status!; }
        if (Services_ListOnly) yield return "--services";
        foreach (var s in Services) yield return s;
    }
}

/// <summary>Settings for <c>docker compose exec &lt;service&gt; &lt;command&gt; [args...]</c>.</summary>
public sealed class DockerComposeExecSettings : DockerComposeBaseSettings
{
    public string? Service { get; set; }
    public string? Command { get; set; }
    public List<string> CommandArgs { get; } = [];
    public bool Detach { get; set; }
    public bool Privileged { get; set; }
    public string? User { get; set; }
    public string? Workdir { get; set; }
    public bool NoTty { get; set; }
    public int? Index { get; set; }
    public Dictionary<string, string> Env { get; } = new();

    public DockerComposeExecSettings SetService(string? service) { Service = service; return this; }
    public DockerComposeExecSettings SetCommand(string? command) { Command = command; return this; }
    public DockerComposeExecSettings AddCommandArg(string arg) { CommandArgs.Add(arg); return this; }
    public DockerComposeExecSettings SetDetach(bool v = true) { Detach = v; return this; }
    public DockerComposeExecSettings SetPrivileged(bool v = true) { Privileged = v; return this; }
    public DockerComposeExecSettings SetUser(string user) { User = user; return this; }
    public DockerComposeExecSettings SetWorkdir(string dir) { Workdir = dir; return this; }
    public DockerComposeExecSettings SetNoTty(bool v = true) { NoTty = v; return this; }
    public DockerComposeExecSettings SetIndex(int n) { Index = n; return this; }
    public DockerComposeExecSettings SetEnv(string key, string value) { Env[key] = value; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        if (string.IsNullOrEmpty(Service)) throw new InvalidOperationException("docker compose exec: Service is required.");
        if (string.IsNullOrEmpty(Command)) throw new InvalidOperationException("docker compose exec: Command is required.");
        yield return "exec";
        if (Detach) yield return "-d";
        if (Privileged) yield return "--privileged";
        if (!string.IsNullOrEmpty(User)) { yield return "--user"; yield return User!; }
        if (!string.IsNullOrEmpty(Workdir)) { yield return "--workdir"; yield return Workdir!; }
        if (NoTty) yield return "-T";
        if (Index is { } i) { yield return "--index"; yield return i.ToString(); }
        foreach (var (k, v) in Env) { yield return "-e"; yield return $"{k}={v}"; }
        yield return Service!;
        yield return Command!;
        foreach (var a in CommandArgs) yield return a;
    }
}

/// <summary>Settings for <c>docker compose run &lt;service&gt; [command] [args...]</c>.</summary>
public sealed class DockerComposeRunSettings : DockerComposeBaseSettings
{
    public string? Service { get; set; }
    public string? Command { get; set; }
    public List<string> CommandArgs { get; } = [];
    public bool Detach { get; set; }
    public bool Rm { get; set; }
    public bool NoDeps { get; set; }
    public bool NoTty { get; set; }
    public string? Name { get; set; }
    public string? User { get; set; }
    public string? Workdir { get; set; }
    public string? Entrypoint { get; set; }
    public Dictionary<string, string> Env { get; } = new();
    public List<string> Publish { get; } = [];
    public List<string> Volume { get; } = [];

    public DockerComposeRunSettings SetService(string? service) { Service = service; return this; }
    public DockerComposeRunSettings SetCommand(string? command) { Command = command; return this; }
    public DockerComposeRunSettings AddCommandArg(string arg) { CommandArgs.Add(arg); return this; }
    public DockerComposeRunSettings SetDetach(bool v = true) { Detach = v; return this; }
    public DockerComposeRunSettings SetRm(bool v = true) { Rm = v; return this; }
    public DockerComposeRunSettings SetNoDeps(bool v = true) { NoDeps = v; return this; }
    public DockerComposeRunSettings SetNoTty(bool v = true) { NoTty = v; return this; }
    public DockerComposeRunSettings SetName(string name) { Name = name; return this; }
    public DockerComposeRunSettings SetUser(string user) { User = user; return this; }
    public DockerComposeRunSettings SetWorkdir(string dir) { Workdir = dir; return this; }
    public DockerComposeRunSettings SetEntrypoint(string entrypoint) { Entrypoint = entrypoint; return this; }
    public DockerComposeRunSettings SetEnv(string k, string v) { Env[k] = v; return this; }
    public DockerComposeRunSettings AddPublish(string spec) { Publish.Add(spec); return this; }
    public DockerComposeRunSettings AddVolume(string spec) { Volume.Add(spec); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        if (string.IsNullOrEmpty(Service)) throw new InvalidOperationException("docker compose run: Service is required.");
        yield return "run";
        if (Detach) yield return "-d";
        if (Rm) yield return "--rm";
        if (NoDeps) yield return "--no-deps";
        if (NoTty) yield return "-T";
        if (!string.IsNullOrEmpty(Name)) { yield return "--name"; yield return Name!; }
        if (!string.IsNullOrEmpty(User)) { yield return "--user"; yield return User!; }
        if (!string.IsNullOrEmpty(Workdir)) { yield return "--workdir"; yield return Workdir!; }
        if (!string.IsNullOrEmpty(Entrypoint)) { yield return "--entrypoint"; yield return Entrypoint!; }
        foreach (var (k, v) in Env) { yield return "-e"; yield return $"{k}={v}"; }
        foreach (var p in Publish) { yield return "--publish"; yield return p; }
        foreach (var v in Volume) { yield return "--volume"; yield return v; }
        yield return Service!;
        if (!string.IsNullOrEmpty(Command)) yield return Command!;
        foreach (var a in CommandArgs) yield return a;
    }
}

/// <summary>Settings for <c>docker compose pull</c>.</summary>
public sealed class DockerComposePullSettings : DockerComposeBaseSettings
{
    public bool Quiet { get; set; }
    public bool IgnorePullFailures { get; set; }
    public bool IncludeDeps { get; set; }
    public List<string> Services { get; } = [];

    public DockerComposePullSettings SetQuiet(bool v = true) { Quiet = v; return this; }
    public DockerComposePullSettings SetIgnorePullFailures(bool v = true) { IgnorePullFailures = v; return this; }
    public DockerComposePullSettings SetIncludeDeps(bool v = true) { IncludeDeps = v; return this; }
    public DockerComposePullSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "pull";
        if (Quiet) yield return "--quiet";
        if (IgnorePullFailures) yield return "--ignore-pull-failures";
        if (IncludeDeps) yield return "--include-deps";
        foreach (var s in Services) yield return s;
    }
}

/// <summary>Settings for <c>docker compose push</c>.</summary>
public sealed class DockerComposePushSettings : DockerComposeBaseSettings
{
    public bool Quiet { get; set; }
    public bool IgnorePushFailures { get; set; }
    public bool IncludeDeps { get; set; }
    public List<string> Services { get; } = [];

    public DockerComposePushSettings SetQuiet(bool v = true) { Quiet = v; return this; }
    public DockerComposePushSettings SetIgnorePushFailures(bool v = true) { IgnorePushFailures = v; return this; }
    public DockerComposePushSettings SetIncludeDeps(bool v = true) { IncludeDeps = v; return this; }
    public DockerComposePushSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "push";
        if (Quiet) yield return "--quiet";
        if (IgnorePushFailures) yield return "--ignore-push-failures";
        if (IncludeDeps) yield return "--include-deps";
        foreach (var s in Services) yield return s;
    }
}

/// <summary>Settings for <c>docker compose config</c> — validate / canonicalize.</summary>
public sealed class DockerComposeConfigSettings : DockerComposeBaseSettings
{
    public bool Quiet { get; set; }
    public bool Services_ListOnly { get; set; }
    public bool VolumesOnly { get; set; }
    public bool ProfilesOnly { get; set; }
    public bool ImagesOnly { get; set; }
    public bool Hash { get; set; }
    public string? Format { get; set; }
    public string? Output { get; set; }

    public DockerComposeConfigSettings SetQuiet(bool v = true) { Quiet = v; return this; }
    public DockerComposeConfigSettings SetServicesListOnly(bool v = true) { Services_ListOnly = v; return this; }
    public DockerComposeConfigSettings SetVolumesOnly(bool v = true) { VolumesOnly = v; return this; }
    public DockerComposeConfigSettings SetProfilesOnly(bool v = true) { ProfilesOnly = v; return this; }
    public DockerComposeConfigSettings SetImagesOnly(bool v = true) { ImagesOnly = v; return this; }
    public DockerComposeConfigSettings SetHash(bool v = true) { Hash = v; return this; }
    public DockerComposeConfigSettings SetFormat(string format) { Format = format; return this; }
    public DockerComposeConfigSettings SetOutput(string path) { Output = path; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "config";
        if (Quiet) yield return "--quiet";
        if (Services_ListOnly) yield return "--services";
        if (VolumesOnly) yield return "--volumes";
        if (ProfilesOnly) yield return "--profiles";
        if (ImagesOnly) yield return "--images";
        if (Hash) yield return "--hash";
        if (!string.IsNullOrEmpty(Format)) { yield return "--format"; yield return Format!; }
        if (!string.IsNullOrEmpty(Output)) { yield return "--output"; yield return Output!; }
    }
}

/// <summary>Settings for compose lifecycle verbs that share the same shape: stop, start, restart, kill, pause, unpause.</summary>
public sealed class DockerComposeLifecycleSettings : DockerComposeBaseSettings
{
    /// <summary>The lifecycle subverb (<c>start</c>, <c>stop</c>, <c>restart</c>, <c>kill</c>, <c>pause</c>, <c>unpause</c>).</summary>
    public string? Subverb { get; set; }
    public int? Timeout { get; set; }
    public List<string> Services { get; } = [];

    public DockerComposeLifecycleSettings SetSubverb(string subverb) { Subverb = subverb; return this; }
    public DockerComposeLifecycleSettings SetTimeout(int seconds) { Timeout = seconds; return this; }
    public DockerComposeLifecycleSettings AddService(string name) { Services.Add(name); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        if (string.IsNullOrEmpty(Subverb)) throw new InvalidOperationException("compose lifecycle: Subverb is required.");
        yield return Subverb!;
        if (Timeout is { } t) { yield return "--timeout"; yield return t.ToString(); }
        foreach (var s in Services) yield return s;
    }
}
