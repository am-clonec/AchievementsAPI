using System;
using TMPro;
using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// The palette and surface overrides used by the achievements menu, settings page, dialogs and
/// toasts. Mods restyle the UI by assigning <see cref="Current"/> (typically
/// <c>AchievementsTheme.Current = AchievementsTheme.Default with { Accent = ... }</c>) at plugin load.
/// Changes take effect the next time the menu is opened or a toast is shown; nothing already on
/// screen is repainted.
/// </summary>
public sealed record AchievementsTheme
{
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
    /// <summary>Replaces the framed menu window background (9-sliced); null keeps the built-in art.</summary>
    public Sprite? MenuBackground { get; init; }
    /// <summary>Replaces the font on every text the API creates; null uses the game font.</summary>
    public TMP_FontAsset? Font { get; init; }

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
