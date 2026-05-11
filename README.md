# Tamp.Docker

Docker CLI wrapper for [Tamp](https://github.com/tamp-build/tamp).

| Package | Docker CLI | Status |
|---|---|---|
| [`Tamp.Docker.V27`](src/Tamp.Docker.V27) | 27.x | preview |

Verbs covered: `login`, `logout`, `build` (BuildKit by default in 0.3.0+),
`tag`, `push`, `pull`, full `compose` sub-facade (up/down/build/logs/ps/pull/exec/...),
full `buildx` sub-facade (build/bake/create/ls/use/inspect/prune/rm/stop/version).
Credentials are typed as `Secret` so they're registered with the runner's redaction table.

Requires `Tamp.Core ≥ 1.0.8` (for the `[FromPath]` attribute).

## Why a separate repo

Docker's CLI ships every couple weeks and the major has bumped recently
(27.x is current). Coupling `tamp` core's release cadence to Docker's
release schedule would force a `tamp` release every time a new Docker
flag landed. Per the satellite-repo convention, `tamp-docker` tracks
Docker's release cadence independently.

## Install

```xml
<PackageVersion Include="Tamp.Docker.V27" Version="0.3.0" />
```

```xml
<PackageReference Include="Tamp.Docker.V27" />
```

## Breaking change in 0.3.0 — `Docker.Build` now uses BuildKit (TAM-117)

In 2026 reality, modern Dockerfiles routinely use BuildKit-only syntax —
`RUN --mount=type=cache`, `RUN --mount=type=secret`, `# syntax=docker/dockerfile:1.x`
frontends, named build contexts — all of which the legacy `docker build`
builder rejects with a Dockerfile-syntax error. The legacy builder is
the exotic case now, not the default.

In 0.3.0:

- **`Docker.Build(...)`** routes to BuildKit (`docker buildx build`) — accepts `DockerBuildxBuildSettings`.
- **`Docker.LegacyBuild(...)`** (new name) is the old pre-BuildKit builder for callers that specifically need it.
- **`Docker.Buildx.Build(...)`** still exists as the explicit form — equivalent to `Docker.Build` in 0.3.0+.

Migrating from 0.2.0:

| Old (0.2.0) | New (0.3.0) | Reason |
|---|---|---|
| `Docker.Build(s => s.SetContext("."))` | `Docker.Build(s => s.SetContext("."))` | Compiles; routes to BuildKit. Works for any modern Dockerfile. |
| `Docker.Build(s => s.SetQuiet(true))` | `Docker.LegacyBuild(s => s.SetQuiet(true))` | `SetQuiet` is legacy-only; switch to LegacyBuild. |
| `Docker.Build(s => s.SetPlatform("linux/amd64"))` | `Docker.Build(s => s.AddPlatform("linux/amd64"))` | BuildKit accepts multiple `--platform`s — collection-shaped. |
| `Docker.Build(s => s.SetOutputType("type=tar"))` | `Docker.Build(s => s.AddOutput("type=tar,dest=out.tar"))` | BuildKit `--output` is collection-shaped. |

Compile errors will tell you exactly which calls need to switch.

## Quick example

```csharp
using Tamp;
using Tamp.Docker.V27;

class Build : TampBuild
{
    public static int Main(string[] args) => Execute<Build>(args);

    [FromPath("docker")] readonly Tool DockerTool = null!;

    [Secret("Docker Hub password", EnvironmentVariable = "DOCKER_HUB_PASSWORD")]
    readonly Secret DockerHubPassword = null!;

    Target Login => _ => _
        .Requires(() => DockerHubPassword != null)
        .Executes(() => Docker.Login(s => s
            .SetUsername("brewingcoder")
            .SetPassword(DockerHubPassword)));

    // Routes to docker buildx build — handles `RUN --mount=type=cache`
    // and other BuildKit-only syntax out of the box.
    Target BuildImage => _ => _.Executes(() => Docker.Build(s => s
        .SetContext(".")
        .SetDockerfile("./Dockerfile")
        .AddTag("brewingcoder/myapp:latest")
        .AddPlatform("linux/amd64")));

    Target PushImage => _ => _
        .DependsOn(nameof(Login), nameof(BuildImage))
        .Executes(() => Docker.Push(s => s.SetTarget("brewingcoder/myapp:latest")));
}
```

## When to use `Docker.LegacyBuild`

Almost never. Specifically:

- You're targeting an ancient Docker daemon (pre-19.03) that lacks BuildKit.
- Your Dockerfile uses syntax that only the legacy builder accepts (rare).
- You explicitly need the legacy builder's behavior for parity with an existing pipeline.

If none of those apply, use `Docker.Build`.

## See also

- [tamp](https://github.com/tamp-build/tamp) — the core framework
- [Tamp ADR 0002](https://github.com/tamp-build/tamp/blob/main/docs/adr/0002-package-naming-convention.md) — package naming convention
- [BuildKit syntax reference](https://docs.docker.com/build/buildkit/dockerfile-frontend/) — what `Docker.Build` now supports natively

## License

[MIT](LICENSE) — same as `tamp` core.
