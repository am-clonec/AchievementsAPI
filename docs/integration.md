# Integrating with the API

Everything here works from any mod that references the API. No menu patching required.

## Reading achievements

`AchievementsManager` exposes display-ready snapshots of every registered achievement. `AchievementInfo` carries the resolved name, description and icon, the owning tab/category/mod, rarity, points, unlock state and timestamp, and progress for `CountAchievement`s. Hidden achievements are masked exactly the way the menu masks them.

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

The toast settings shown on the menu's Settings page are plain BepInEx `ConfigEntry`s on `AchievementsConfig`. You can read and write them directly. Changes save and apply immediately:

```csharp
AchievementsConfig.ToastVolume.Value = 0.3f;
AchievementsConfig.SettingChanged += entry => Log($"setting changed: {entry.Definition.Key}");
AchievementsConfig.ResetToDefaults();
```

`AchievementsConfig.Entries` lists every bound setting in the order the settings page shows them. Use this to build your own settings UI.

## Theming

Assign `AchievementsTheme.Current` at plugin load to completely replace the menu, settings page, dialogs and toasts UI. The theme system allows you to replace every UI element, not just colors. It's a record, so start from `Default` and override what you need:

```csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    Accent = new Color32(180, 80, 60, 255),
    AccentHover = new Color32(220, 120, 90, 255),
};
```

The theme is read when UI is built. Changes take effect the next time the menu is opened or a toast is shown. Nothing already on screen is repainted. `AchievementsTheme.Changed` fires after each assignment.

### Colors

Override any of the 15 color properties:

```csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    Accent = new Color32(180, 80, 60, 255),
    AccentHover = new Color32(220, 120, 90, 255),
    AccentPressed = new Color32(140, 60, 50, 255),
    Danger = new Color32(200, 50, 50, 255),
    DangerHover = new Color32(240, 80, 80, 255),
    DangerPressed = new Color32(160, 40, 40, 255),
    PrimaryText = new Color32(255, 255, 255, 255),
    SecondaryText = new Color32(200, 200, 200, 255),
    Track = new Color32(120, 120, 120, 255),
    Rim = new Color32(115, 117, 120, 255),
    CommonLabel = new Color32(235, 235, 235, 255),
    Rare = new Color32(110, 180, 220, 255),
    Epic = new Color32(165, 120, 220, 255),
    Legendary = new Color32(220, 190, 100, 255),
    Surface = new Color32(65, 68, 68, 255),
    CardFace = new Color32(45, 44, 45, 255),
};
```

### Sprites

Replace any UI element with your own sprites. Leave null to use the built-in art:

```csharp
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

### Fonts

Replace the font on all text:

```csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    Font = myCustomFontAsset,
};
```

### Layouts

Control the size and spacing of UI elements:

```csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    // Menu layout
    MenuWidth = 1600f,
    MenuHeight = 1000f,
    MenuScale = 0.95f,
    MenuPadding = 60f,
    MenuSpacing = 25f,

    // Card layout
    CardHeight = 110f,
    CardPadding = 20f,
    CardIconSize = 75f,

    // Button layout
    ButtonWidth = 200f,
    ButtonHeight = 52f,
    ButtonPadding = 15f,

    // Toast layout
    ToastHeight = 100f,
    ToastWidth = 600f,
    ToastDiscSize = 110f,
    ToastIconSize = 80f,
    ToastEdgeOffset = 150f,

    // Dialog layout
    DialogWidth = 700f,
    DialogHeight = 360f,
    DialogPadding = 40f,
};
```

### Font Sizes

Control text sizes throughout the UI:

```csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    TitleFontSize = 42f,
    HeaderFontSize = 32f,
    BodyFontSize = 24f,
    CaptionFontSize = 18f,
    CardNameFontSize = 26f,
    CardDescFontSize = 20f,
};
```

### Positions

Control the exact placement of UI elements:

```csharp
AchievementsTheme.Current = AchievementsTheme.Default with
{
    // Menu positions
    CloseButtonPosition = new Vector2(-20f, -50f),
    TitleIconPosition = new Vector2(120f, -75f),
    TitlePosition = new Vector2(150f, -50f),
    FilterRowPosition = new Vector2(200f, -75f),
    ToolbarOffsetMin = new Vector2(480f, -200f),
    ToolbarOffsetMax = new Vector2(-100f, -150f),

    // Card positions
    CardIconPosition = new Vector2(60f, 0f),
    CardIconSize = new Vector2(60f, 60f),
    CardDiscSize = 80f,
    CardNameOffsetMin = new Vector2(120f, -55f),
    CardNameOffsetMax = new Vector2(-200f, -25f),
    CardDescOffsetMin = new Vector2(120f, -85f),
    CardDescOffsetMax = new Vector2(-200f, -60f),
    CardProgressOffsetMin = new Vector2(120f, 35f),
    CardProgressOffsetMax = new Vector2(-200f, 45f),
    CardProgressLabelPosition = new Vector2(-200f, 47f),

    // Dialog positions
    DialogTitlePosition = new Vector2(0f, -40f),
    DialogBodyPosition = new Vector2(0f, -110f),
    DialogButtonY = 70f,
    DialogButtonXOffset = 120f,
};
```

### Complete Example

A complete theme customization:

```csharp
public override void Load()
{
    AchievementsTheme.Current = AchievementsTheme.Default with
    {
        // Custom color scheme
        Accent = new Color32(180, 80, 60, 255),
        AccentHover = new Color32(220, 120, 90, 255),
        Surface = new Color32(70, 45, 40, 255),
        CardFace = new Color32(50, 30, 25, 255),

        // Custom sprites
        MenuBackground = LoadSprite("MyMod.Resources.WoodPanel.png"),
        ButtonBackground = LoadSprite("MyMod.Resources.WoodButton.png"),

        // Larger UI
        MenuWidth = 1700f,
        MenuHeight = 1050f,
        CardHeight = 120f,

        // Larger text
        TitleFontSize = 44f,
        BodyFontSize = 26f,

        // Adjusted positions
        TitlePosition = new Vector2(150f, -50f),
        CardIconPosition = new Vector2(65f, 0f),
        CardNameOffsetMin = new Vector2(125f, -55f),
    };
}

private static Sprite LoadSprite(string path)
{
    return SpriteTools.LoadSpriteFromPath(path, typeof(MyPlugin).Assembly, 100);
}
```
