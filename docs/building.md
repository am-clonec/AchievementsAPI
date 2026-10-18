# Building and internals

Notes for working on the API itself, as opposed to building against it.

## Project layout

```
AchievementsAPI.sln
├── AchievementsAPI/              the mod
│   ├── API/                      the public surface (tabs, achievements, storage, config, toasts)
│   ├── Components/               Unity-side UI: menu panels, the Xbox-style toast, input handling
│   ├── Patches/                  Harmony patches (lifecycle events, menu button, options menu)
│   ├── Resources/                embedded png/wav assets and the UI asset bundles
│   └── DeveloperTab.cs, ExampleTab.cs   built-in tabs
└── AchievementsAPI.Developer/    the developer-only companion plugin (below)
```

The API targets .NET 6 against `AmongUs.GameLibs.Steam` and depends on Reactor and BepInEx IL2CPP (see `AchievementsAPI.csproj`; the extra feeds live in `nuget.config`). UI prefabs ship as platform-specific asset bundles (`achievements-win.bundle`, `achievements-android.bundle`) loaded at runtime by `Assets.cs`.

## Building

```
dotnet build AchievementsAPI.sln
```

Set the `AmongUs` MSBuild property to a game install and the build copies the DLL into `BepInEx/plugins/` automatically:

```
dotnet build AchievementsAPI.sln -p:AmongUs="C:\Program Files (x86)\Steam\steamapps\common\Among Us"
```

Versions come from `build.cake`: local builds are `{VersionPrefix}-dev` (`2.0.0-dev`), CI builds get a `ci.{run}` suffix, and tag builds use the tag with the leading `v` stripped (`v2.0.0` builds `2.0.0`).

## CI

`.github/workflows/main.yml` builds pushes and PRs to `master` with Cake on Ubuntu and uploads `AchievementsAPI.dll` as an artifact. `vf.yml` does the same for the `vf` branch, producing `AchievementsAPI-VF.dll`.

## Releases

Pushing a tag of the form `vMAJOR.MINOR.PATCH` or `vMAJOR.MINOR.PATCH-prerelease` (for example `v2.0.0` or `v2.0.0-rc.1`) runs `.github/workflows/release.yml`, which:

1. validates the tag format and derives the version from it,
2. builds and packs with `build.cake --target=Pack`,
3. creates a GitHub release named after the tag with auto-generated notes (categories come from `.github/release.yml`; tags with a prerelease suffix are marked as pre-releases), attaching `AchievementsAPI.dll`, `CloneC.AchievementsAPI.{version}.nupkg` and `SHA256SUMS.txt`,
4. pushes the package to nuget.org if a `NUGET_API_KEY` repository secret is configured (skipped otherwise).

To cut a release:

```
git tag v2.0.0
git push origin v2.0.0
```

The developer plugin (`AchievementsAPI.Developer`) is intentionally not released; see below.

## The developer plugin

`AchievementsAPI.Developer` is a separate, developer-only plugin that announces the local player as a developer to the lobby. Mechanism:

1. A developer builds the project with their own name in `DeveloperPlugin`'s `DeveloperName` and keeps the DLL private. It's marked `ReactorModFlags(ModFlags.None)`, so it doesn't participate in mod-handshake requirements; nobody else in the lobby needs (or should have) it.
2. On load it calls `DeveloperPresence.RegisterLocalDeveloper(name)`. The API then announces the name over a custom RPC at three points: when a lobby starts, a few seconds after each new player joins (so their client has spawned the developer's `PlayerControl`; an RPC to a not-yet-spawned net object gets dropped), and when the intro cutscene ends.
3. Other players' clients log that a developer is present, and when the match ends everyone who played with them unlocks "Among Developers" (legendary, hidden) from `DeveloperTab`.

`DeveloperTab` has `IsSelectable => PlayedWithDeveloper.Unlocked`, so it stays out of the menu and the Skeld Points totals until the achievement is earned. Players who never meet a developer never see it.

## ExampleTab

`ExampleTab` ("Getting Started!") demonstrates reset triggers and deferred toasts, and doubles as the settings page's preview toast source. It self-hides (`IsSelectable`) as soon as any real mod registers a tab, so it only shows up for players who installed the API on its own.

## Contributor notes

- Public API additions need XML doc comments. The comments on `BaseAchievement`, `AchievementsTab` and `AchievementsManager` are the reference the other [docs pages](README.md) are written from, and they show up in consumers' IDEs.
- Achievement ids are `{AssemblyName}_{Name}`; never change how ids are built for existing constructors (save files key off them, see [storage](storage.md)).
- Clicks must not fall through the menu or toasts to game UI. Keep that invariant in mind when touching the `MenuInput`/`PassiveButton` patches.
- Save-file changes go through `AchievementStorage`. Keep the corrupt-file backup path working when altering the format.
