namespace Tamp.Docker.V27;

/// <summary>
/// Tamp wrapper for the Docker 27.x CLI. Each method returns a
/// <see cref="CommandPlan"/> the runner dispatches or prints; nothing is
/// executed at call time.
/// </summary>
public static class Docker
{
    public static CommandPlan Login(Action<DockerLoginSettings> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new DockerLoginSettings();
        configure(s);
        return s.ToCommandPlan();
    }

    public static CommandPlan Logout(Action<DockerLogoutSettings>? configure = null)
    {
        var s = new DockerLogoutSettings();
        configure?.Invoke(s);
        return s.ToCommandPlan();
    }

    /// <summary>
    /// <c>docker buildx build</c> — the modern BuildKit-backed build entry point. Routes through
    /// <see cref="Buildx.Build"/>. This is the canonical <c>Docker.Build</c> in 0.3.0+; the legacy
    /// (pre-BuildKit) builder is available as <see cref="LegacyBuild"/>.
    /// </summary>
    /// <remarks>
    /// Modern Dockerfiles using <c>RUN --mount=type=cache</c>, <c>RUN --mount=type=secret</c>,
    /// <c># syntax=docker/dockerfile:1.x</c> frontends, or named build contexts REQUIRE BuildKit
    /// and fail under the legacy builder with a Dockerfile-syntax error. In 2026, the legacy
    /// builder is the exotic case — this default reflects that.
    /// </remarks>
    public static CommandPlan Build(Action<DockerBuildxBuildSettings> configure)
        => Buildx.Build(configure);

    /// <summary>
    /// <c>docker build</c> — the legacy (pre-BuildKit) builder. Use when you know your Dockerfile
    /// does NOT use BuildKit-only syntax and you specifically need the legacy build path. Modern
    /// callers should use <see cref="Build"/> (which routes to BuildKit) instead.
    /// </summary>
    public static CommandPlan LegacyBuild(Action<DockerLegacyBuildSettings> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new DockerLegacyBuildSettings();
        configure(s);
        return s.ToCommandPlan();
    }

    public static CommandPlan Tag(Action<DockerTagSettings> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new DockerTagSettings();
        configure(s);
        return s.ToCommandPlan();
    }

    public static CommandPlan Push(Action<DockerPushSettings> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new DockerPushSettings();
        configure(s);
        return s.ToCommandPlan();
    }

    public static CommandPlan Pull(Action<DockerPullSettings> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var s = new DockerPullSettings();
        configure(s);
        return s.ToCommandPlan();
    }

    /// <summary>
    /// Sub-facade for <c>docker compose &lt;verb&gt;</c>. Sits under the
    /// same Docker namespace so callers see <c>Docker.Compose.Up(...)</c>
    /// without a separate import.
    /// </summary>
    public static class Compose
    {
        public static CommandPlan Up(Action<DockerComposeUpSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Make(configure);
        }

        public static CommandPlan Down(Action<DockerComposeDownSettings>? configure = null) => Make(configure);
        public static CommandPlan BuildImages(Action<DockerComposeBuildSettings>? configure = null) => Make(configure);
        public static CommandPlan Logs(Action<DockerComposeLogsSettings>? configure = null) => Make(configure);
        public static CommandPlan Ps(Action<DockerComposePsSettings>? configure = null) => Make(configure);
        public static CommandPlan Pull(Action<DockerComposePullSettings>? configure = null) => Make(configure);
        public static CommandPlan Push(Action<DockerComposePushSettings>? configure = null) => Make(configure);

        public static CommandPlan Exec(Action<DockerComposeExecSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Make(configure);
        }

        public static CommandPlan Run(Action<DockerComposeRunSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Make(configure);
        }

        public static CommandPlan Config(Action<DockerComposeConfigSettings>? configure = null) => Make(configure);

        public static CommandPlan Start(Action<DockerComposeLifecycleSettings>? configure = null) => Lifecycle("start", configure);
        public static CommandPlan Stop(Action<DockerComposeLifecycleSettings>? configure = null) => Lifecycle("stop", configure);
        public static CommandPlan Restart(Action<DockerComposeLifecycleSettings>? configure = null) => Lifecycle("restart", configure);
        public static CommandPlan Kill(Action<DockerComposeLifecycleSettings>? configure = null) => Lifecycle("kill", configure);
        public static CommandPlan Pause(Action<DockerComposeLifecycleSettings>? configure = null) => Lifecycle("pause", configure);
        public static CommandPlan Unpause(Action<DockerComposeLifecycleSettings>? configure = null) => Lifecycle("unpause", configure);

        private static CommandPlan Lifecycle(string subverb, Action<DockerComposeLifecycleSettings>? configure)
        {
            var s = new DockerComposeLifecycleSettings();
            s.SetSubverb(subverb);
            configure?.Invoke(s);
            return s.ToCommandPlan();
        }

        private static CommandPlan Make<T>(Action<T>? configure) where T : DockerComposeBaseSettings, new()
        {
            var s = new T();
            configure?.Invoke(s);
            return s.ToCommandPlan();
        }
    }

    /// <summary>
    /// Sub-facade for <c>docker buildx &lt;verb&gt;</c>. Multi-platform
    /// build orchestration that lives alongside the base CLI but with a
    /// different shape (named builders, cache backends, bake files).
    /// </summary>
    public static class Buildx
    {
        public static CommandPlan Build(Action<DockerBuildxBuildSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Make(configure);
        }

        public static CommandPlan Bake(Action<DockerBuildxBakeSettings>? configure = null) => Make(configure);
        public static CommandPlan Create(Action<DockerBuildxCreateSettings>? configure = null) => Make(configure);
        public static CommandPlan Ls(Action<DockerBuildxLsSettings>? configure = null) => Make(configure);

        public static CommandPlan Use(Action<DockerBuildxUseSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Make(configure);
        }

        public static CommandPlan Inspect(Action<DockerBuildxInspectSettings>? configure = null) => Make(configure);
        public static CommandPlan Prune(Action<DockerBuildxPruneSettings>? configure = null) => Make(configure);

        public static CommandPlan Rm(Action<DockerBuildxRmSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Make(configure);
        }

        public static CommandPlan Stop(Action<DockerBuildxStopSettings>? configure = null) => Make(configure);
        public static CommandPlan Version(Action<DockerBuildxVersionSettings>? configure = null) => Make(configure);

        private static CommandPlan Make<T>(Action<T>? configure) where T : DockerBuildxBaseSettings, new()
        {
            var s = new T();
            configure?.Invoke(s);
            return s.ToCommandPlan();
        }
    }
}
