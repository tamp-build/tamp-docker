namespace Tamp.Docker.V27;

/// <summary>
/// Common base for <c>docker buildx &lt;verb&gt;</c> settings. Buildx has
/// a couple of pre-verb knobs (<c>--builder</c>, <c>--debug</c>) plus
/// the verb-specific arguments emitted by subclasses.
/// </summary>
public abstract class DockerBuildxBaseSettings : DockerSettingsBase
{
    /// <summary>Selects a non-default builder instance. Maps to <c>--builder</c>.</summary>
    public string? Builder { get; set; }

    /// <summary>Enables debug-level logging from the buildx CLI itself. Maps to <c>--debug</c>.</summary>
    public bool Debug { get; set; }

    /// <summary>Subclasses produce the per-verb tokens and verb-specific flags.</summary>
    protected abstract IEnumerable<string> BuildSubVerbArguments();

    protected override IEnumerable<string> BuildVerbArguments()
    {
        yield return "buildx";
        if (Debug) yield return "--debug";
        if (!string.IsNullOrEmpty(Builder)) { yield return "--builder"; yield return Builder!; }
        foreach (var a in BuildSubVerbArguments()) yield return a;
    }
}

/// <summary>Settings for <c>docker buildx build</c> — superset of <c>docker build</c> with multi-platform + cache backends.</summary>
public sealed class DockerBuildxBuildSettings : DockerBuildxBaseSettings
{
    public string? Context { get; set; }
    public string? Dockerfile { get; set; }
    public List<string> Tags { get; } = [];
    public List<string> Platforms { get; } = [];
    public Dictionary<string, string> BuildArgs { get; } = new();
    public Dictionary<string, string> Labels { get; } = new();
    public Dictionary<string, string> SecretsArgs { get; } = new();
    public List<string> CacheFrom { get; } = [];
    public List<string> CacheTo { get; } = [];
    public List<string> Outputs { get; } = [];
    public string? Target { get; set; }
    public bool NoCache { get; set; }
    public bool Pull { get; set; }
    public bool Push { get; set; }
    public bool Load { get; set; }
    public string? Network { get; set; }
    public string? Progress { get; set; }
    public string? Metadata { get; set; }
    public string? Sbom { get; set; }
    public string? Provenance { get; set; }

    public DockerBuildxBuildSettings SetContext(string? path) { Context = path; return this; }
    public DockerBuildxBuildSettings SetDockerfile(string? path) { Dockerfile = path; return this; }
    public DockerBuildxBuildSettings AddTag(string tag) { Tags.Add(tag); return this; }
    public DockerBuildxBuildSettings AddPlatform(string p) { Platforms.Add(p); return this; }
    public DockerBuildxBuildSettings SetBuildArg(string k, string v) { BuildArgs[k] = v; return this; }
    public DockerBuildxBuildSettings SetLabel(string k, string v) { Labels[k] = v; return this; }
    public DockerBuildxBuildSettings SetSecret(string id, string source) { SecretsArgs[id] = source; return this; }
    public DockerBuildxBuildSettings AddCacheFrom(string spec) { CacheFrom.Add(spec); return this; }
    public DockerBuildxBuildSettings AddCacheTo(string spec) { CacheTo.Add(spec); return this; }
    public DockerBuildxBuildSettings AddOutput(string spec) { Outputs.Add(spec); return this; }
    public DockerBuildxBuildSettings SetTarget(string? target) { Target = target; return this; }
    public DockerBuildxBuildSettings SetNoCache(bool v = true) { NoCache = v; return this; }
    public DockerBuildxBuildSettings SetPull(bool v = true) { Pull = v; return this; }
    public DockerBuildxBuildSettings SetPush(bool v = true) { Push = v; return this; }
    public DockerBuildxBuildSettings SetLoad(bool v = true) { Load = v; return this; }
    public DockerBuildxBuildSettings SetNetwork(string? network) { Network = network; return this; }
    public DockerBuildxBuildSettings SetProgress(string? progress) { Progress = progress; return this; }
    public DockerBuildxBuildSettings SetMetadata(string path) { Metadata = path; return this; }
    public DockerBuildxBuildSettings SetSbom(string spec) { Sbom = spec; return this; }
    public DockerBuildxBuildSettings SetProvenance(string spec) { Provenance = spec; return this; }
    public DockerBuildxBuildSettings SetWorkingDirectory(string? cwd) { WorkingDirectory = cwd; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "build";
        if (!string.IsNullOrEmpty(Dockerfile)) { yield return "--file"; yield return Dockerfile!; }
        foreach (var t in Tags) { yield return "--tag"; yield return t; }
        if (Platforms.Count > 0) { yield return "--platform"; yield return string.Join(',', Platforms); }
        foreach (var (k, v) in BuildArgs) { yield return "--build-arg"; yield return $"{k}={v}"; }
        foreach (var (k, v) in Labels) { yield return "--label"; yield return $"{k}={v}"; }
        foreach (var (id, src) in SecretsArgs) { yield return "--secret"; yield return $"id={id},{src}"; }
        foreach (var spec in CacheFrom) { yield return "--cache-from"; yield return spec; }
        foreach (var spec in CacheTo) { yield return "--cache-to"; yield return spec; }
        foreach (var o in Outputs) { yield return "--output"; yield return o; }
        if (!string.IsNullOrEmpty(Target)) { yield return "--target"; yield return Target!; }
        if (NoCache) yield return "--no-cache";
        if (Pull) yield return "--pull";
        if (Push) yield return "--push";
        if (Load) yield return "--load";
        if (!string.IsNullOrEmpty(Network)) { yield return "--network"; yield return Network!; }
        if (!string.IsNullOrEmpty(Progress)) { yield return "--progress"; yield return Progress!; }
        if (!string.IsNullOrEmpty(Metadata)) { yield return "--metadata-file"; yield return Metadata!; }
        if (!string.IsNullOrEmpty(Sbom)) { yield return "--sbom"; yield return Sbom!; }
        if (!string.IsNullOrEmpty(Provenance)) { yield return "--provenance"; yield return Provenance!; }
        yield return Context ?? ".";
    }
}

/// <summary>Settings for <c>docker buildx bake</c>.</summary>
public sealed class DockerBuildxBakeSettings : DockerBuildxBaseSettings
{
    public List<string> Files { get; } = [];
    public List<string> Targets { get; } = [];
    public Dictionary<string, string> Sets { get; } = new();
    public bool NoCache { get; set; }
    public bool Pull { get; set; }
    public bool Push { get; set; }
    public bool Load { get; set; }
    public bool PrintOnly { get; set; }
    public string? Progress { get; set; }
    public string? Metadata { get; set; }

    public DockerBuildxBakeSettings AddFile(string path) { Files.Add(path); return this; }
    public DockerBuildxBakeSettings AddTarget(string name) { Targets.Add(name); return this; }
    public DockerBuildxBakeSettings SetOverride(string key, string value) { Sets[key] = value; return this; }
    public DockerBuildxBakeSettings SetNoCache(bool v = true) { NoCache = v; return this; }
    public DockerBuildxBakeSettings SetPull(bool v = true) { Pull = v; return this; }
    public DockerBuildxBakeSettings SetPush(bool v = true) { Push = v; return this; }
    public DockerBuildxBakeSettings SetLoad(bool v = true) { Load = v; return this; }
    public DockerBuildxBakeSettings SetPrintOnly(bool v = true) { PrintOnly = v; return this; }
    public DockerBuildxBakeSettings SetProgress(string? progress) { Progress = progress; return this; }
    public DockerBuildxBakeSettings SetMetadata(string path) { Metadata = path; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "bake";
        foreach (var f in Files) { yield return "--file"; yield return f; }
        foreach (var (k, v) in Sets) { yield return "--set"; yield return $"{k}={v}"; }
        if (NoCache) yield return "--no-cache";
        if (Pull) yield return "--pull";
        if (Push) yield return "--push";
        if (Load) yield return "--load";
        if (PrintOnly) yield return "--print";
        if (!string.IsNullOrEmpty(Progress)) { yield return "--progress"; yield return Progress!; }
        if (!string.IsNullOrEmpty(Metadata)) { yield return "--metadata-file"; yield return Metadata!; }
        foreach (var t in Targets) yield return t;
    }
}

/// <summary>Settings for <c>docker buildx create</c> — provision a builder instance.</summary>
public sealed class DockerBuildxCreateSettings : DockerBuildxBaseSettings
{
    public string? Name { get; set; }
    public string? Driver { get; set; }
    public Dictionary<string, string> DriverOpts { get; } = new();
    public bool Use { get; set; }
    public bool Bootstrap { get; set; }
    public string? Platform { get; set; }
    public List<string> Endpoints { get; } = [];

    public DockerBuildxCreateSettings SetName(string name) { Name = name; return this; }
    public DockerBuildxCreateSettings SetDriver(string driver) { Driver = driver; return this; }
    public DockerBuildxCreateSettings SetDriverOpt(string k, string v) { DriverOpts[k] = v; return this; }
    public DockerBuildxCreateSettings SetUse(bool v = true) { Use = v; return this; }
    public DockerBuildxCreateSettings SetBootstrap(bool v = true) { Bootstrap = v; return this; }
    public DockerBuildxCreateSettings SetPlatform(string platform) { Platform = platform; return this; }
    public DockerBuildxCreateSettings AddEndpoint(string endpoint) { Endpoints.Add(endpoint); return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "create";
        if (!string.IsNullOrEmpty(Name)) { yield return "--name"; yield return Name!; }
        if (!string.IsNullOrEmpty(Driver)) { yield return "--driver"; yield return Driver!; }
        foreach (var (k, v) in DriverOpts) { yield return "--driver-opt"; yield return $"{k}={v}"; }
        if (Use) yield return "--use";
        if (Bootstrap) yield return "--bootstrap";
        if (!string.IsNullOrEmpty(Platform)) { yield return "--platform"; yield return Platform!; }
        foreach (var e in Endpoints) yield return e;
    }
}

/// <summary>Settings for <c>docker buildx ls</c> — list builders.</summary>
public sealed class DockerBuildxLsSettings : DockerBuildxBaseSettings
{
    public string? Format { get; set; }
    public bool NoTrunc { get; set; }

    public DockerBuildxLsSettings SetFormat(string format) { Format = format; return this; }
    public DockerBuildxLsSettings SetNoTrunc(bool v = true) { NoTrunc = v; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "ls";
        if (!string.IsNullOrEmpty(Format)) { yield return "--format"; yield return Format!; }
        if (NoTrunc) yield return "--no-trunc";
    }
}

/// <summary>Settings for <c>docker buildx use</c> — switch active builder.</summary>
public sealed class DockerBuildxUseSettings : DockerBuildxBaseSettings
{
    public string? BuilderName { get; set; }
    public bool Default { get; set; }
    public bool Global { get; set; }

    public DockerBuildxUseSettings SetBuilderName(string name) { BuilderName = name; return this; }
    public DockerBuildxUseSettings SetDefault(bool v = true) { Default = v; return this; }
    public DockerBuildxUseSettings SetGlobal(bool v = true) { Global = v; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        if (string.IsNullOrEmpty(BuilderName)) throw new InvalidOperationException("docker buildx use: BuilderName is required.");
        yield return "use";
        if (Default) yield return "--default";
        if (Global) yield return "--global";
        yield return BuilderName!;
    }
}

/// <summary>Settings for <c>docker buildx inspect</c>.</summary>
public sealed class DockerBuildxInspectSettings : DockerBuildxBaseSettings
{
    public string? BuilderName { get; set; }
    public bool Bootstrap { get; set; }

    public DockerBuildxInspectSettings SetBuilderName(string? name) { BuilderName = name; return this; }
    public DockerBuildxInspectSettings SetBootstrap(bool v = true) { Bootstrap = v; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "inspect";
        if (Bootstrap) yield return "--bootstrap";
        if (!string.IsNullOrEmpty(BuilderName)) yield return BuilderName!;
    }
}

/// <summary>Settings for <c>docker buildx prune</c> — reclaim cache.</summary>
public sealed class DockerBuildxPruneSettings : DockerBuildxBaseSettings
{
    public bool All { get; set; }
    public bool Force { get; set; }
    public bool Verbose { get; set; }
    public Dictionary<string, string> Filters { get; } = new();
    public string? KeepStorage { get; set; }

    public DockerBuildxPruneSettings SetAll(bool v = true) { All = v; return this; }
    public DockerBuildxPruneSettings SetForce(bool v = true) { Force = v; return this; }
    public DockerBuildxPruneSettings SetVerbose(bool v = true) { Verbose = v; return this; }
    public DockerBuildxPruneSettings SetFilter(string k, string v) { Filters[k] = v; return this; }
    public DockerBuildxPruneSettings SetKeepStorage(string spec) { KeepStorage = spec; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "prune";
        if (All) yield return "--all";
        if (Force) yield return "--force";
        if (Verbose) yield return "--verbose";
        if (!string.IsNullOrEmpty(KeepStorage)) { yield return "--keep-storage"; yield return KeepStorage!; }
        foreach (var (k, v) in Filters) { yield return "--filter"; yield return $"{k}={v}"; }
    }
}

/// <summary>Settings for <c>docker buildx rm</c> — remove a builder.</summary>
public sealed class DockerBuildxRmSettings : DockerBuildxBaseSettings
{
    public List<string> BuilderNames { get; } = [];
    public bool KeepState { get; set; }
    public bool KeepDaemon { get; set; }
    public bool Force { get; set; }
    public bool AllInactive { get; set; }

    public DockerBuildxRmSettings AddBuilderName(string name) { BuilderNames.Add(name); return this; }
    public DockerBuildxRmSettings SetKeepState(bool v = true) { KeepState = v; return this; }
    public DockerBuildxRmSettings SetKeepDaemon(bool v = true) { KeepDaemon = v; return this; }
    public DockerBuildxRmSettings SetForce(bool v = true) { Force = v; return this; }
    public DockerBuildxRmSettings SetAllInactive(bool v = true) { AllInactive = v; return this; }

    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "rm";
        if (KeepState) yield return "--keep-state";
        if (KeepDaemon) yield return "--keep-daemon";
        if (Force) yield return "--force";
        if (AllInactive) yield return "--all-inactive";
        foreach (var n in BuilderNames) yield return n;
    }
}

/// <summary>Settings for <c>docker buildx stop</c>.</summary>
public sealed class DockerBuildxStopSettings : DockerBuildxBaseSettings
{
    public string? BuilderName { get; set; }
    public DockerBuildxStopSettings SetBuilderName(string? name) { BuilderName = name; return this; }
    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "stop";
        if (!string.IsNullOrEmpty(BuilderName)) yield return BuilderName!;
    }
}

/// <summary>Settings for <c>docker buildx version</c>.</summary>
public sealed class DockerBuildxVersionSettings : DockerBuildxBaseSettings
{
    protected override IEnumerable<string> BuildSubVerbArguments()
    {
        yield return "version";
    }
}
