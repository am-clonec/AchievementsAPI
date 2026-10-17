# Integrating with the API

The API exposes achievement data, configuration settings, and UI theming for other mods to use.

## Achievements

`AchievementsManager` provides `AchievementInfo` snapshots for
registered achievements, including resolved text and icons, category,
owning mod, rarity, points, unlock state, unlock timestamp, and progress
for `CountAchievement`s. Hidden achievements use the same masking rules
as the menu.

``` csharp
var all = AchievementsManager.GetAllAchievements();
var info = AchievementsManager.GetInfo("MyMod_MyAchievement");
var impostor = AchievementsManager.GetAchievements(AchievementCategories.Impostor);
var stats = AchievementsManager.GetStats();
```

`GetAllAchievements()` returns achievements from selectable tabs.
`GetInfo()` returns `null` if the ID isn't registered.

Listen for unlocks, progress changes, and resets with the static events:

``` csharp
AchievementsManager.AchievementUnlocked += a => Log(a.Id);
AchievementsManager.AchievementProgressed += c =>
    Log($"{c.CurrentValue}/{c.RequiredValue}");
AchievementsManager.AchievementReset += a => Log("reset");
```

## Settings

`AchievementsConfig` exposes the settings used by the menu as BepInEx
`ConfigEntry` properties. Read or update them directly:

``` csharp
AchievementsConfig.ToastVolume.Value = 0.3f;

AchievementsConfig.SettingChanged += entry =>
    Log($"Setting changed: {entry.Definition.Key}");

AchievementsConfig.ResetToDefaults();
```

Changes are saved and applied immediately. `AchievementsConfig.Entries`
returns the bound settings in the same order they appear on the Settings
page, which you can use to build a custom settings UI.

## Theming

Set `AchievementsTheme.Current` to customize the menu, settings page,
dialogs, and achievement toasts. Start with `AchievementsTheme.Default`
and override only the properties you need.

``` csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    Accent = new Color32(180, 80, 60, 255),
    AccentHover = new Color32(220, 120, 90, 255),
};
```

The theme is read when UI elements are created. Existing UI isn't
updated when the theme changes; the new values apply the next time the
menu opens or a toast appears. `AchievementsTheme.Changed` fires
whenever `Current` is assigned.

### Colors

The theme exposes these color properties:

  -----------------------------------------------------------------------
  Properties                          Purpose
  ----------------------------------- -----------------------------------
  `Accent`, `AccentHover`,            Primary interactive elements
  `AccentPressed`                     

  `Danger`, `DangerHover`,            Destructive actions
  `DangerPressed`                     

  `PrimaryText`, `SecondaryText`      Text

  `Track`, `Rim`                      Progress and decorative elements

  `CommonLabel`, `Rare`, `Epic`,      Rarity labels
  `Legendary`                         

  `Surface`, `CardFace`               UI backgrounds
  -----------------------------------------------------------------------

Override any combination using the same `with` syntax shown above.

### Sprites and fonts

Assign custom sprites to replace the built-in artwork. Any property you
don't override keeps its default value.

``` csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    MenuBackground = LoadSprite("MyMod.Resources.MenuBackground.png"),
    CardBackground = LoadSprite("MyMod.Resources.CardBackground.png"),
    RarityDisc = LoadSprite("MyMod.Resources.RarityDisc.png"),
    ProgressBarBackground = LoadSprite("MyMod.Resources.ProgressBar.png"),
    ButtonBackground = LoadSprite("MyMod.Resources.ButtonBackground.png"),
    CloseButtonIcon = LoadSprite("MyMod.Resources.CloseIcon.png"),
    SearchIcon = LoadSprite("MyMod.Resources.SearchIcon.png"),
    SearchClearIcon = LoadSprite("MyMod.Resources.SearchClearIcon.png"),
    ToastBackground = LoadSprite("MyMod.Resources.ToastBackground.png"),
};
```

To change the font used throughout the UI:

``` csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    Font = myCustomFontAsset,
};
```

### Layout and typography

Layout properties control element dimensions, padding, spacing, and
positions. Font-size properties control text throughout the UI.

``` csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    MenuWidth = 1600f,
    MenuHeight = 1000f,
    MenuScale = 0.95f,
    MenuPadding = 60f,
    MenuSpacing = 25f,

    CardHeight = 110f,
    CardPadding = 20f,
    CardIconSize = 75f,

    ButtonWidth = 200f,
    ButtonHeight = 52f,
    ButtonPadding = 15f,

    ToastWidth = 600f,
    ToastHeight = 100f,
    ToastDiscSize = 110f,
    ToastIconSize = 80f,
    ToastEdgeOffset = 150f,

    DialogWidth = 700f,
    DialogHeight = 360f,
    DialogPadding = 40f,

    TitleFontSize = 42f,
    HeaderFontSize = 32f,
    BodyFontSize = 24f,
    CaptionFontSize = 18f,
    CardNameFontSize = 26f,
    CardDescFontSize = 20f,
};
```

Position properties use `Vector2` values. For example:

``` csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    CloseButtonPosition = new Vector2(-20f, -50f),
    TitleIconPosition = new Vector2(120f, -75f),
    TitlePosition = new Vector2(150f, -50f),
    FilterRowPosition = new Vector2(200f, -75f),

    CardIconPosition = new Vector2(60f, 0f),
    CardNameOffsetMin = new Vector2(120f, -55f),
    CardNameOffsetMax = new Vector2(-200f, -25f),
    CardDescOffsetMin = new Vector2(120f, -85f),
    CardDescOffsetMax = new Vector2(-200f, -60f),
    CardProgressOffsetMin = new Vector2(120f, 35f),
    CardProgressOffsetMax = new Vector2(-200f, 45f),
    CardProgressLabelPosition = new Vector2(-200f, 47f),

    DialogTitlePosition = new Vector2(0f, -40f),
    DialogBodyPosition = new Vector2(0f, -110f),
    DialogButtonY = 70f,
    DialogButtonXOffset = 120f,
};
```

Other position properties control toolbar bounds and offsets. Use IDE
completion on `AchievementsTheme.Default` to browse the full set of
available properties.

## Example

Set the theme in your plugin's `Load()` method:

``` csharp
public override void Load()
{
    AchievementsTheme.Current = AchievementsTheme.Default with
    {
        Accent = new Color32(180, 80, 60, 255),
        AccentHover = new Color32(220, 120, 90, 255),
        Surface = new Color32(70, 45, 40, 255),
        CardFace = new Color32(50, 30, 25, 255),

        MenuBackground = LoadSprite("MyMod.Resources.WoodPanel.png"),
        ButtonBackground = LoadSprite("MyMod.Resources.WoodButton.png"),

        MenuWidth = 1700f,
        MenuHeight = 1050f,
        CardHeight = 120f,
        TitleFontSize = 44f,
    };
}

private static Sprite LoadSprite(string path)
{
    return SpriteTools.LoadSpriteFromPath(
        path,
        typeof(MyPlugin).Assembly,
        100);
}
```
