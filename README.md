# Tamp.Docker

Docker CLI wrappers for [Tamp](https://github.com/tamp-build/tamp).

| Package | Docker CLI | Status |
|---|---|---|
| [`Tamp.Docker.V27`](src/Tamp.Docker.V27) | 27.x | live |

Verbs covered: `login`, `logout`, `build`, `tag`, `push`, `pull`. Credentials are
typed as `Secret` so they're registered with the runner's redaction table.

## Why a separate repo

Docker's CLI ships every couple weeks and the major has bumped recently
(27.x is current). Coupling Tamp core's release cadence to Docker's
release schedule would force a Tamp release every time a new Docker
flag landed. Splitting `tamp-docker` out lets each Docker major pin
its own wrapper version independently.

## Quick example

```csharp
using Tamp;
using Tamp.Docker.V27;

class Build : TampBuild
{
    public static int Main(string[] args) => Execute<Build>(args);

    [NuGetPackage("docker", UseSystemPath = true)]
    readonly Tool Docker = null!;

    [Secret("Docker Hub password", EnvironmentVariable = "DOCKER_HUB_PASSWORD")]
    readonly Secret DockerHubPassword = null!;

    Target Login => _ => _.Executes(() => Tamp.Docker.V27.Docker.Login(Docker, s => s
        .SetUsername("brewingcoder")
        .SetPassword(DockerHubPassword)));

    Target BuildImage => _ => _.Executes(() => Tamp.Docker.V27.Docker.Build(Docker, s => s
        .SetTag("brewingcoder/myapp:latest")
        .SetContext(".")));

    Target PushImage => _ => _
        .DependsOn(nameof(Login), nameof(BuildImage))
        .Executes(() => Tamp.Docker.V27.Docker.Push(Docker, s => s
            .SetTarget("brewingcoder/myapp:latest")));
}
```

## License

[MIT](LICENSE) — same as `tamp` core.
