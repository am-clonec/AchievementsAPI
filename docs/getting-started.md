# Getting started

There are two ways to build on the Achievements API, depending on whether you want to ship it as a required mod or keep it optional for your players.

| | Hard dependency | Soft dependency |
| --- | --- | --- |
| Mod works without Achievements API | No | Yes |
| Achievement style | `AchievementsTab` subclass with properties | `SoftAchievementsTab` + runtime achievements |
| Type safety | Full compile-time | Runtime-constructed |
| Recommended for | Most mods | Mods where achievements are a bonus |

## Hard dependency (recommended for most mods)

Reference `AchievementsAPI` from your mod and subclass `AchievementsTab`. Achievements are declared as properties and discovered automatically when your plugin loads, with no registration call needed. This gives you full compile-time type safety and the simplest API:

```csharp
using Reactor.Localization.Utilities;

[BepInAutoPlugin]
[BepInDependency(AchievementsAPIPlugin.Id)] // RequiredDependency (default)
public class MyPlugin : BasePlugin
{
    public override void Load() { }
}

public class MyTab : AchievementsTab
{
    private static readonly StringNames tabName = CustomStringName.CreateAndRegister("My Mod");
    private static readonly StringNames firstWin = CustomStringName.CreateAndRegister("First Win");
    private static readonly StringNames firstWinDesc = CustomStringName.CreateAndRegister("Win a game.");
    private static readonly StringNames busyBee = CustomStringName.CreateAndRegister("Busy Bee");
    private static readonly StringNames busyBeeDesc = CustomStringName.CreateAndRegister("Complete 10 tasks.");

    public override StringNames Name => tabName;
    public override string IconPath => "MyMod.Resources.Icon.png";

    public BaseAchievement FirstWin { get; set; } =
        new(firstWin, firstWinDesc, "MyMod.Resources.FirstWin.png", rarity: 1);

    public CountAchievement TenTasks { get; set; } =
        new(busyBee, busyBeeDesc, "MyMod.Resources.Tasks.png", 0, 10);
}
```

Unlock an achievement with `achievement.Unlock()`. For a `CountAchievement`, use `Increment(amount)` or `SetValue(value)`; it unlocks on its own once `RequiredValue` is reached.

The icon can be an embedded resource path or a `Sprite`. Constructor options cover rarity (0 common, 1 rare, 2 epic, 3 legendary), `hidden`, `hideRarity`, and for count achievements `hideProgress` and how progress persists (`AchPersistence`).

Access a tab from elsewhere with `AchievementsTabSingleton<MyTab>.Instance`.

## Soft dependency (optional Achievements API)

If you want users to run your mod **without** Achievements API installed, keep it as a `SoftDependency` and use runtime-constructed achievements instead of subclasses. This is the pattern used by [Town of Us Mira](https://github.com/AU-Avengers/TOU-Mira) and [Mira Unleashed](https://github.com/rewalo/TownOfUsMiraUnleashed/).

Mark the dependency soft in your plugin and register tabs/achievements only when the plugin is actually loaded:

```csharp
using BepInEx.Unity.IL2CPP;

[BepInAutoPlugin]
[BepInDependency("AchievementsAPI", BepInDependency.DependencyFlags.SoftDependency)]
public class MyPlugin : BasePlugin
{
    public override void Load()
    {
        if (!IL2CPPChainloader.Instance.Plugins.ContainsKey("AchievementsAPI"))
        {
            return;
        }

        RegisterAchievements(); // safe to touch API types now
    }
}
```

Because `AchievementsTab` subclasses in your assembly are still touched by BepInEx/MiraAPI assembly scanning, use `SoftAchievementsTab` + `RuntimeAchievement`/`RuntimeCountAchievement` instead of deriving your own classes:

```csharp
// Only call this method when the plugin is loaded (the JIT won't resolve the
// AchievementsAPI types until this frame actually executes).
private void RegisterAchievements()
{
    var tab = new SoftAchievementsTab("My Mod", categoryId: "general");
    AchievementsManager.RegisterTab(tab);

    tab.RegisterAchievement(new RuntimeAchievement("First Win", "Win a game.", rarity: 1,
        assembly: typeof(MyPlugin).Assembly));

    tab.RegisterAchievement(new RuntimeCountAchievement("Busy Bee", "Complete 10 tasks.", 0, 10,
        AchPersistence.ThroughoutRounds, rarity: 1, assembly: typeof(MyPlugin).Assembly));
}
```

Two details to get right:

- Pass your own `Assembly`. Achievement ids are built as `{AssemblyName}_{AchievementName}`, and the id is what save files match against. The constructors default to `Assembly.GetCallingAssembly()`, which is correct from inside the method, but passing it explicitly keeps that obvious and protects you if the call ever moves behind a helper in another assembly.
- Keep the JIT boundary. Anything that touches AchievementsAPI types must live behind a method that only runs when the plugin is present. Descriptor-driven registration (build a list, then register it in one guarded method) keeps the dependency away from your gameplay code.

If you want a ready-made helper for this pattern, see `TownOfUs.Achievements.AchievementsAPIHelper` in TOU-Mira.

## Registering achievements at runtime

Even from a hard dependency you don't need properties. Register achievements on a tab at any point after it exists:

```csharp
var tab = new SoftAchievementsTab(CustomStringName.CreateAndRegister("My Tab"));
AchievementsManager.RegisterTab(tab);
tab.RegisterAchievement(myAchievement);
```

`AchievementsManager.GetTab(stringName)` looks a tab up by name, and `tab.GetAchievements()` enumerates everything on it (property-declared first, then runtime-registered, de-duplicated by id). `RegisterAchievement` loads the achievement's stored state immediately, so it's safe to call late.
