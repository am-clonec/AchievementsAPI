using System;
using TMPro;
using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// Complete theming system for the achievements menu, settings page, dialogs and toasts.
/// Mods can replace every UI element including colors, sprites, fonts, and layouts.
/// Assign <see cref="Current"/> at plugin load (typically
/// <c>AchievementsTheme.Current = AchievementsTheme.Default with { Accent = ... }</c>).
/// Changes take effect the next time the menu is opened or a toast is shown; nothing already on
/// screen is repainted.
/// </summary>
public sealed record AchievementsTheme
{
    // COLORS
    public Color32 Accent { get; init; } = new(74, 127, 121, 255);
    public Color32 AccentHover { get; init; } = new(126, 203, 195, 255);
    public Color32 AccentPressed { get; init; } = new(56, 96, 92, 255);
    public Color32 Danger { get; init; } = new(196, 64, 64, 255);
    public Color32 DangerHover { get; init; } = new(232, 96, 96, 255);
    public Color32 DangerPressed { get; init; } = new(140, 44, 44, 255);
    /// <summary>Headline text: titles, names, button labels.</summary>
    public Color32 PrimaryText { get; init; } = new(255, 255, 255, 255);
    /// <summary>Secondary text: captions, descriptions, values.</summary>
    public Color32 SecondaryText { get; init; } = new(190, 190, 190, 255);
    /// <summary>Slider tracks, toggle-off tracks and progress-bar backgrounds.</summary>
    public Color32 Track { get; init; } = new(112, 112, 112, 255);
    /// <summary>Neutral ring/rim for common (rarity 0) achievements.</summary>
    public Color32 Rim { get; init; } = new(112, 114, 118, 255);
    /// <summary>Label color used for "Common" in rarity breakdowns.</summary>
    public Color32 CommonLabel { get; init; } = new(230, 230, 230, 255);
    public Color32 Rare { get; init; } = new(104, 172, 212, 255);
    public Color32 Epic { get; init; } = new(158, 112, 214, 255);
    public Color32 Legendary { get; init; } = new(214, 184, 92, 255);
    /// <summary>Fill of buttons, pills, row strips and stat boxes.</summary>
    public Color32 Surface { get; init; } = new(61, 64, 64, 255);
    /// <summary>Face of achievement cards, toast pills and icon discs.</summary>
    public Color32 CardFace { get; init; } = new(41, 40, 41, 255);

    // SPRITES
    /// <summary>Replaces the framed menu window background (9-sliced); null keeps the built-in art.</summary>
    public Sprite? MenuBackground { get; init; }
    /// <summary>Replaces achievement card background; null keeps the built-in art.</summary>
    public Sprite? CardBackground { get; init; }
    /// <summary>Replaces rarity ring/disc background; null keeps the built-in art.</summary>
    public Sprite? RarityDisc { get; init; }
    /// <summary>Replaces progress bar background; null keeps the built-in art.</summary>
    public Sprite? ProgressBarBackground { get; init; }
    /// <summary>Replaces button background; null keeps the built-in art.</summary>
    public Sprite? ButtonBackground { get; init; }
    /// <summary>Replaces close button icon; null keeps the built-in art.</summary>
    public Sprite? CloseButtonIcon { get; init; }
    /// <summary>Replaces search icon; null keeps the built-in art.</summary>
    public Sprite? SearchIcon { get; init; }
    /// <summary>Replaces search clear icon; null keeps the built-in art.</summary>
    public Sprite? SearchClearIcon { get; init; }
    /// <summary>Replaces toast pill background; null keeps the built-in art.</summary>
    public Sprite? ToastBackground { get; init; }

    // FONTS
    /// <summary>Replaces the font on every text the API creates; null uses the game font.</summary>
    public TMP_FontAsset? Font { get; init; }

    // LAYOUT - Menu
    /// <summary>Menu panel width in pixels.</summary>
    public float MenuWidth { get; init; } = 1560f;
    /// <summary>Menu panel height in pixels.</summary>
    public float MenuHeight { get; init; } = 960f;
    /// <summary>Menu panel scale.</summary>
    public float MenuScale { get; init; } = 0.92f;
    /// <summary>Menu padding in pixels.</summary>
    public float MenuPadding { get; init; } = 48f;
    /// <summary>Spacing between menu elements.</summary>
    public float MenuSpacing { get; init; } = 20f;

    // LAYOUT - Cards
    /// <summary>Achievement card height in pixels.</summary>
    public float CardHeight { get; init; } = 100f;
    /// <summary>Achievement card padding in pixels.</summary>
    public float CardPadding { get; init; } = 16f;
    /// <summary>Icon size in achievement cards.</summary>
    public float CardIconSize { get; init; } = 68f;

    // LAYOUT - Buttons
    /// <summary>Button width in pixels.</summary>
    public float ButtonWidth { get; init; } = 180f;
    /// <summary>Button height in pixels.</summary>
    public float ButtonHeight { get; init; } = 48f;
    /// <summary>Button padding in pixels.</summary>
    public float ButtonPadding { get; init; } = 12f;

    // LAYOUT - Toasts
    /// <summary>Toast pill height in pixels.</summary>
    public float ToastHeight { get; init; } = 96f;
    /// <summary>Toast pill width in pixels.</summary>
    public float ToastWidth { get; init; } = 560f;
    /// <summary>Toast disc size in pixels.</summary>
    public float ToastDiscSize { get; init; } = 100f;
    /// <summary>Toast icon size in pixels.</summary>
    public float ToastIconSize { get; init; } = 76f;
    /// <summary>Toast edge offset from screen edge in pixels.</summary>
    public float ToastEdgeOffset { get; init; } = 140f;

    // LAYOUT - Dialogs
    /// <summary>Dialog width in pixels.</summary>
    public float DialogWidth { get; init; } = 680f;
    /// <summary>Dialog height in pixels.</summary>
    public float DialogHeight { get; init; } = 340f;
    /// <summary>Dialog padding in pixels.</summary>
    public float DialogPadding { get; init; } = 36f;

    // FONT SIZES
    /// <summary>Title text size.</summary>
    public float TitleFontSize { get; init; } = 38f;
    /// <summary>Header text size.</summary>
    public float HeaderFontSize { get; init; } = 30f;
    /// <summary>Body text size.</summary>
    public float BodyFontSize { get; init; } = 22f;
    /// <summary>Caption text size.</summary>
    public float CaptionFontSize { get; init; } = 16f;
    /// <summary>Card name text size.</summary>
    public float CardNameFontSize { get; init; } = 24f;
    /// <summary>Card description text size.</summary>
    public float CardDescFontSize { get; init; } = 18f;

    /// <summary>Color for a rarity tier (0 = <see cref="Rim"/>).</summary>
    public Color RarityColor(int rarity) => rarity switch { 1 => Rare, 2 => Epic, 3 => Legendary, _ => Rim };

    public static AchievementsTheme Default { get; } = new();

    private static AchievementsTheme _current = Default;
    /// <summary>The active theme. Assigning a new theme invalidates the cached surface sprites.</summary>
    public static AchievementsTheme Current
    {
        get => _current;
        set { _current = value ?? Default; ToastSprites.InvalidateThemed(); Changed?.Invoke(_current); }
    }
    /// <summary>Raised after <see cref="Current"/> is assigned.</summary>
    public static event Action<AchievementsTheme>? Changed;
}
