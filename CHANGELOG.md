# Changelog

All notable changes to **Tamp.Docker** are recorded here. Format loosely follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) with semver pinning.

## 0.3.1

- Object-init overloads on every Docker wrapper (TAM-161 satellite fanout).
  Each fluent `Docker.*` / `Docker.Compose.*` / `Docker.Buildx.*` verb now has
  a parallel overload that takes the settings object directly, enabling C#
  collection-initializer authoring style alongside the canonical fluent form.
  Lifecycle compose verbs (`Start`/`Stop`/`Restart`/`Kill`/`Pause`/`Unpause`)
  remain fluent-only because they require an internal subverb token the
  object-init can't supply.
