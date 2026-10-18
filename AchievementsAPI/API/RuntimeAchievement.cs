using System;
using System.Reflection;
using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// A runtime-configurable achievement: the art and offset overrides of <see cref="BaseAchievement"/>
/// are supplied as fields and delegates instead of subclassing. Intended for mods that build
/// achievements dynamically (from descriptors, config, or another mod's API) without declaring
/// their own achievement classes.
/// </summary>
public class RuntimeAchievement : BaseAchievement
{
    /// <summary>
    /// Lazily resolves the achievement's icon. Falls back to the sprite/path given to the constructor.
    /// </summary>
    public Func<Sprite?>? IconProvider;

    /// <summary>Lazily resolves the background sprite in the achievements menu.</summary>
    public Func<Sprite?>? MenuBgProvider;

    /// <summary>Lazily resolves the background sprite in the toast popup.</summary>
    public Func<Sprite>? ToastBgProvider;

    /// <summary>Lazily resolves the sub-icon in the achievements menu.</summary>
    public Func<Sprite?>? MenuSubIconProvider;

    /// <summary>Lazily resolves the sub-icon hover tooltip in the achievements menu.</summary>
    public Func<string?>? MenuSubIconTooltipProvider;

    public Vector3? MenuIconOffsetOverride;
    public Vector3? MenuTitleOffsetOverride;
    public Vector3? MenuDescOffsetOverride;
    public Vector3? MenuSubIconOffsetOverride;
    public Vector3? MenuSubIconScaleOverride;
    public Vector3? ToastIconOffsetOverride;

    /// <summary>
    /// Creates a new runtime-configurable achievement.
    /// </summary>
    /// <param name="name">The achievement's name.</param>
    /// <param name="description">The achievement's description.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public RuntimeAchievement(StringNames name, StringNames description, int rarity = 0, bool hidden = false,
        bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        : base(name, description, rarity, hidden, hideRarity, assembly, tags)
    {
    }

    /// <summary>
    /// Creates a new runtime-configurable achievement.
    /// </summary>
    /// <param name="name">The achievement's name (plain text).</param>
    /// <param name="description">The achievement's description (plain text).</param>
    /// <param name="icon">The achievement's icon sprite.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public RuntimeAchievement(string name, string description, Sprite icon, int rarity = 0, bool hidden = false,
        bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        : base(name, description, icon, rarity, hidden, hideRarity, assembly, tags)
    {
    }

    /// <summary>
    /// Creates a new runtime-configurable achievement.
    /// </summary>
    /// <param name="name">The achievement's name (plain text).</param>
    /// <param name="description">The achievement's description (plain text).</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public RuntimeAchievement(string name, string description, int rarity = 0, bool hidden = false,
        bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        : base(name, description, rarity, hidden, hideRarity, assembly, tags)
    {
    }

    /// <inheritdoc />
    public override Sprite Icon => IconProvider?.Invoke() ?? base.Icon;

    /// <inheritdoc />
    public override Sprite? MenuBgSprite => MenuBgProvider?.Invoke() ?? base.MenuBgSprite;

    /// <inheritdoc />
    public override Sprite? ToastBgSprite => ToastBgProvider?.Invoke() ?? base.ToastBgSprite;

    /// <inheritdoc />
    public override Sprite? MenuSubIcon => MenuSubIconProvider?.Invoke() ?? base.MenuSubIcon;

    /// <inheritdoc />
    public override string? MenuSubIconTooltip => MenuSubIconTooltipProvider?.Invoke() ?? base.MenuSubIconTooltip;

    /// <inheritdoc />
    public override Vector3 MenuIconOffset => MenuIconOffsetOverride ?? base.MenuIconOffset;

    /// <inheritdoc />
    public override Vector3 MenuTitleOffset => MenuTitleOffsetOverride ?? base.MenuTitleOffset;

    /// <inheritdoc />
    public override Vector3 MenuDescOffset => MenuDescOffsetOverride ?? base.MenuDescOffset;

    /// <inheritdoc />
    public override Vector3 MenuSubIconOffset => MenuSubIconOffsetOverride ?? base.MenuSubIconOffset;

    /// <inheritdoc />
    public override Vector3 MenuSubIconScale => MenuSubIconScaleOverride ?? base.MenuSubIconScale;

    /// <inheritdoc />
    public override Vector3 ToastIconOffset => ToastIconOffsetOverride ?? base.ToastIconOffset;
}

/// <summary>
/// A runtime-configurable <see cref="CountAchievement"/>: same customization surface as
/// <see cref="RuntimeAchievement"/>, for progress-tracked achievements built without subclassing.
/// </summary>
public class RuntimeCountAchievement : CountAchievement
{
    /// <inheritdoc cref="RuntimeAchievement.IconProvider"/>
    public Func<Sprite?>? IconProvider;

    /// <inheritdoc cref="RuntimeAchievement.MenuBgProvider"/>
    public Func<Sprite?>? MenuBgProvider;

    /// <inheritdoc cref="RuntimeAchievement.ToastBgProvider"/>
    public Func<Sprite?>? ToastBgProvider;

    /// <inheritdoc cref="RuntimeAchievement.MenuSubIconProvider"/>
    public Func<Sprite?>? MenuSubIconProvider;

    /// <inheritdoc cref="RuntimeAchievement.MenuSubIconTooltipProvider"/>
    public Func<string?>? MenuSubIconTooltipProvider;

    public Vector3? MenuIconOffsetOverride;
    public Vector3? MenuTitleOffsetOverride;
    public Vector3? MenuDescOffsetOverride;
    public Vector3? MenuSubIconOffsetOverride;
    public Vector3? MenuSubIconScaleOverride;
    public Vector3? ToastIconOffsetOverride;

    public RuntimeCountAchievement(StringNames name, StringNames description, int currentValue, int requiredValue,
        AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false,
        bool hideRarity = false, bool hideProgress = false, Assembly? assembly = null, string[]? tags = null)
        : base(name, description, currentValue, requiredValue, progressPersists, rarity, hidden, hideRarity, hideProgress, assembly, tags)
    {
    }

    public RuntimeCountAchievement(string name, string description, Sprite icon, int currentValue, int requiredValue,
        AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false,
        bool hideRarity = false, bool hideProgress = false, Assembly? assembly = null, string[]? tags = null)
        : base(name, description, icon, currentValue, requiredValue, progressPersists, rarity, hidden, hideRarity, hideProgress, assembly, tags)
    {
    }

    /// <inheritdoc />
    public override Sprite Icon => IconProvider?.Invoke() ?? base.Icon;

    /// <inheritdoc />
    public override Sprite? MenuBgSprite => MenuBgProvider?.Invoke() ?? base.MenuBgSprite;

    /// <inheritdoc />
    public override Sprite? ToastBgSprite => ToastBgProvider?.Invoke() ?? base.ToastBgSprite;

    /// <inheritdoc />
    public override Sprite? MenuSubIcon => MenuSubIconProvider?.Invoke() ?? base.MenuSubIcon;

    /// <inheritdoc />
    public override string? MenuSubIconTooltip => MenuSubIconTooltipProvider?.Invoke() ?? base.MenuSubIconTooltip;

    /// <inheritdoc />
    public override Vector3 MenuIconOffset => MenuIconOffsetOverride ?? base.MenuIconOffset;

    /// <inheritdoc />
    public override Vector3 MenuTitleOffset => MenuTitleOffsetOverride ?? base.MenuTitleOffset;

    /// <inheritdoc />
    public override Vector3 MenuDescOffset => MenuDescOffsetOverride ?? base.MenuDescOffset;

    /// <inheritdoc />
    public override Vector3 MenuSubIconOffset => MenuSubIconOffsetOverride ?? base.MenuSubIconOffset;

    /// <inheritdoc />
    public override Vector3 MenuSubIconScale => MenuSubIconScaleOverride ?? base.MenuSubIconScale;

    /// <inheritdoc />
    public override Vector3 ToastIconOffset => ToastIconOffsetOverride ?? base.ToastIconOffset;
}
