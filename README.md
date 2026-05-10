# Tamp.Docker

Docker CLI wrapper for [Tamp](https://github.com/tamp-build/tamp).

| Package | Docker CLI | Status |
|---|---|---|
| [`Tamp.Docker.V27`](src/Tamp.Docker.V27) | 27.x | preview |

Verbs covered: `login`, `logout`, `build`, `tag`, `push`, `pull`.
Credentials are typed as `Secret` so they're registered with the
runner's redaction table.

Requires `Tamp.Core ≥ 1.0.0`.

## Why a separate repo

Docker's CLI ships every couple weeks and the major has bumped recently
(27.x is current). Coupling `tamp` core's release cadence to Docker's
release schedule would force a `tamp` release every time a new Docker
flag landed. Per the satellite-repo convention, `tamp-docker` tracks
Docker's release cadence independently.

## Install

In your build script's `Directory.Packages.props`:

```xml
<PackageVersion Include="Tamp.Docker.V27" Version="0.0.1-alpha" />
```

In `build/Build.csproj`:

```xml
<PackageReference Include="Tamp.Docker.V27" />
```

## Quick example

```csharp
using Tamp;
using Tamp.Docker.V27;

class Build : TampBuild
{
    public static int Main(string[] args) => Execute<Build>(args);

    [NuGetPackage("docker", UseSystemPath = true)]
    readonly Tool DockerTool = null!;

    // Until TAM-78 lands [Secret] env-var resolution in Tamp.Core 1.0.1,
    // load credentials manually from env:
    static readonly Secret? DockerHubPassword =
        Environment.GetEnvironmentVariable("DOCKER_HUB_PASSWORD") is { Length: > 0 } v
            ? new Secret("Docker Hub password", v) : null;

    Target Login => _ => _
        .Requires(() => DockerHubPassword != null)
        .Executes(() => Docker.Login(DockerTool, s => s
            .SetUsername("brewingcoder")
            .SetPassword(DockerHubPassword!)));

    Target BuildImage => _ => _.Executes(() => Docker.Build(DockerTool, s => s
        .SetTag("brewingcoder/myapp:latest")
        .SetContext(".")));

    Target PushImage => _ => _
        .DependsOn(nameof(Login), nameof(BuildImage))
        .Executes(() => Docker.Push(DockerTool, s => s
            .SetTarget("brewingcoder/myapp:latest")));
}
```

## See also

- [tamp](https://github.com/tamp-build/tamp) — the core framework
- [Tamp ADR 0002](https://github.com/tamp-build/tamp/blob/main/docs/adr/0002-package-naming-convention.md) — package naming convention

## License

[MIT](LICENSE) — same as `tamp` core.
