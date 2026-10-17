# Integrating with the API

Everything here works from any mod that references the API — no menu patching required.

## Reading achievements

`AchievementsManager` exposes display-ready snapshots of every registered achievement. `AchievementInfo` carries the resolved name, description and icon, the owning tab/category/mod, rarity, points, unlock state and timestamp, and progress for `CountAchievement`s — with hidden achievements masked exactly the way the menu masks them.

```csharp
var all = AchievementsManager.GetAllAchievements();           // selectable tabs only
var one = AchievementsManager.GetInfo("MyMod_MyAchievement"); // null if not registered
var inCategory = AchievementsManager.GetAchievements(AchievementCategories.Impostor);
var stats = AchievementsManager.GetStats();                   // totals, points, per-rarity counts
```

Three static events track state changes:

```csharp
AchievementsManager.AchievementUnlocked  += a => Log(a.Id);
AchievementsManager.AchievementProgressed += c => Log($"{c.CurrentValue}/{c.RequiredValue}");
AchievementsManager.AchievementReset     += a => Log("reset");
```

## Settings

The toast settings shown on the menu's Settings page are plain BepInEx `ConfigEntry`s on `AchievementsConfig`, so you can read and write them directly — changes save and apply immediately:

```csharp
AchievementsConfig.ToastVolume.Value = 0.3f;
AchievementsConfig.SettingChanged += entry => Log($"setting changed: {entry.Definition.Key}");
AchievementsConfig.ResetToDefaults();
```

`AchievementsConfig.Entries` lists every bound setting in the order the settings page shows them, handy for building your own settings UI.
