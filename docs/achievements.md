# Defining achievements

All achievements derive from `BaseAchievement`; progressable ones use `CountAchievement`. Both come in subclass form (properties on an `AchievementsTab`) and runtime form (`RuntimeAchievement` / `RuntimeCountAchievement`). They behave the same either way.

## BaseAchievement

Every constructor takes a name, a description, and either an icon (embedded resource path or `Sprite`) or nothing (falls back to the default icon). Names and descriptions can be plain `string`s or `StringNames`; see [localization](localization.md) for why `StringNames` is the better pick.

```csharp
public BaseAchievement FirstWin { get; set; } =
    new("First Win", "Win a game.", "MyMod.Resources.FirstWin.png", rarity: 1, hidden: false);
```

Common constructor options:

| Parameter | Default | Effect |
| --- | --- | --- |
| `rarity` | `0` | 0 common, 1 rare, 2 epic, 3 legendary. Drives the tint, the chime, the toast duration and the point value |
| `hidden` | `false` | Shows as "Hidden Achievement" until unlocked |
| `hideRarity` | `false` | Hides the rarity tint as well (only relevant when hidden) |
| `assembly` | calling assembly | Sets the assembly half of the achievement's id |
| `tags` | `null` | Optional tags for filtering and searching (e.g. role names, modifier names) |

The full identity is `{AssemblyName}_{Name}` and never changes with language, which is what keeps save files stable. The name half is the raw text for string constructors and the enum value for `StringNames` constructors.

### State and unlocking

- `Unlock()` flips `Unlocked`, shows the toast (unless `showOnUI: false`) and writes through to storage immediately.
- `UnlockedAt` is the UTC time the achievement unlocked, or null while locked.
- `Reset()` returns it to locked (and zeroes progress on count achievements). Set `ResetTrigger` to have this happen automatically; see [game events](game-events.md).

### Tags

Achievements can have tags for filtering and searching. This is useful for grouping achievements by role, modifier, or other criteria. Tags are case-insensitive when searching.

```csharp
// Adding tags when creating an achievement
public BaseAchievement ImposterKill = new("Imposter Kill", "Kill as Imposter.", icon,
    tags: new[] { "Imposter", "Killer" });

// For count achievements
public CountAchievement FiveKills = new("Five Kills", "Kill 5 players.", icon, 0, 5,
    tags: new[] { "Imposter", "Killer" });
```

You can search for achievements by tags using `AchievementsManager`:

```csharp
// Find all achievements with a specific tag
var impostorAchievements = AchievementsManager.FindAchievementsByTag("Imposter");

// Find achievements with any of the specified tags
var killerOrSaboteur = AchievementsManager.FindAchievementsByAnyTag("Killer", "Saboteur");

// Find achievements with all of the specified tags
var impostorKillers = AchievementsManager.FindAchievementsByAllTags("Imposter", "Killer");
```

All search methods return a `List<(AchievementsTab Tab, BaseAchievement Achievement)>` containing matching achievements along with their parent tabs.

### Skeld Points

Each achievement is worth Skeld Points based on rarity: 10 common / 25 rare / 50 epic / 100 legendary, times `PointsMultiplier` (which doubles for hidden achievements). The menu shows the earned and total score next to a Gamerscore-style emblem, per-rarity completion, and each row's value in a pill. Override `BasePoints` or `PointsMultiplier` to customize.

### Sub-icon badges

Any achievement can show a small secondary sprite (e.g. a role icon) layered on its row in the achievements menu, with no subclassing needed:

```csharp
tab.RegisterAchievement(new RuntimeAchievement("Hitman", "Prophecy your target's death.", sprite)
{
    SubIcon = roleIconSprite,
    SubIconOffset = new Vector3(-40f, -70f),
    SubIconScale = new Vector3(0.6f, 0.6f, 1f)
});
```

`SubIcon`, `SubIconOffset` and `SubIconScale` are plain settable fields on `BaseAchievement`. Subclass-based mods can instead override the `MenuSubIcon` / `MenuSubIconOffset` / `MenuSubIconScale` properties for dynamic behavior (e.g. lazily loading bundle assets). Default scale is `(0.5, 0.5, 1)`.

Set `MenuSubIconTooltip` (or `MenuSubIconTooltipProvider` on the runtime variants) to show a small hover tooltip over the badge — handy for telling the player which role or modifier the icon belongs to. Null or empty means no tooltip.

### Art and layout overrides

Everything the menu and toasts look up is virtual:

| Override | Default | Notes |
| --- | --- | --- |
| `Icon` | sprite from constructor | Also used by toasts |
| `MenuBgSprite` | `null` | Row background in the menu |
| `ToastBgSprite` | `null` | Unused by the current toast; kept for compatibility |
| `MenuIconOffset`, `MenuTitleOffset`, `MenuDescOffset` | `(0, 0)` | Fine-tuning for custom art |
| `MenuSubIcon*` | from `SubIcon*` fields | Sub-icon badge (above) |
| `MenuSubIconTooltip` | `null` | Hover tooltip on the sub-icon badge (above) |
| `ToastObtainedOffset`, `ToastTitleOffset`, `ToastIconOffset` | `(0, 0)` | Toast layout |
| `RarityOnBgSprite` | `true` | Set to false to keep the background untinted |

The runtime variants expose the same surface as fields and delegates instead of overrides: `IconProvider`, `MenuBgProvider`, `ToastBgProvider`, `MenuSubIconProvider`, `MenuSubIconTooltipProvider`, plus `*Override` offset fields. Providers are invoked lazily, so you can defer asset-bundle loads until the menu actually asks for the sprite.

## CountAchievement

Adds progress on top of the base. `RequiredValue` is the goal; `SetValue(value)` / `Increment(amount)` move `CurrentValue` and unlock automatically at the goal. A progress update enqueues a toast each time. An unlock toast drops any progress toasts still queued for the same achievement, and [deferred](game-events.md#deferred-toasts) updates collapse into a single toast with the final value.

```csharp
public CountAchievement TenTasks { get; set; } =
    new(busyBee, busyBeeDesc, "MyMod.Resources.Tasks.png", 0, 10);

// later, from gameplay code:
TenTasks.Increment(1);
```

Count-specific constructor options:

| Parameter | Default | Effect |
| --- | --- | --- |
| `currentValue` / `requiredValue` | (required) | Starting progress and goal. Stored progress from a previous session is clamped into `[0, RequiredValue]` |
| `progressPersists` | `ThroughoutSessions` | How long progress survives; see below |
| `hideProgress` | `false` | Hides the progress bar (and the count in toasts) for hidden achievements |

`AchPersistence` controls when unfinished progress resets:

| Value | Progress resets |
| --- | --- |
| `ThroughoutSessions` | Never (survives game restarts) |
| `ThroughoutRounds` | When the match ends |
| `ResetOnRoundStart` | When a new round starts |
| `ResetOnMeetingStart` | When a meeting is called |

Completed achievements are never touched by these resets. An unlocked `CountAchievement` stays unlocked, and its stored progress stays at the goal.

What fires when, plus the exact Harmony patch points, is in [game events](game-events.md).

## RuntimeAchievement / RuntimeCountAchievement

The runtime variants exist for achievements built from descriptors, config, or another mod's API, and are the only option under a [soft dependency](getting-started.md#soft-dependency-optional-achievements-api), where your own subclasses would get touched by assembly scanning.

Beyond the provider and offset-override fields listed above they behave identically to the base classes: same ids, same storage, same menu treatment. Construct them on a `SoftAchievementsTab`:

```csharp
tab.RegisterAchievement(new RuntimeCountAchievement("Busy Bee", "Complete 10 tasks.", 0, 10,
    AchPersistence.ThroughoutSessions, rarity: 1, assembly: typeof(MyPlugin).Assembly,
    tags: new[] { "Tasks", "Crewmate" })
{
    IconProvider = () => MyAssets.LoadSprite("busy-bee"),
});
```
