using System;
using System.Reflection;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// Well-known category ids. Every mod shares these; use them for <see cref="AchievementsTab.CategoryId"/>
/// or <see cref="BaseAchievement.Category"/> so achievements from different mods land in the same place.
/// </summary>
public static class AchievementCategories
{
    public const string General = "general";
    public const string Crewmate = "crewmate";
    public const string Impostor = "impostor";
    public const string Neutral = "neutral";
}

/// <summary>
/// A shared category shown in the left column of the Achievements Menu. Categories are owned by the API,
/// not by any one mod: any mod can put achievements into any category, and the menu groups the entries
/// inside a category by the mod they came from.
/// </summary>
public sealed class AchievementCategory
{
    private Func<Sprite?>? _iconProvider;
    private Sprite? _icon;

    /// <summary>
    /// Creates a category. Prefer <see cref="AchievementsManager.GetOrCreateCategory(string, StringNames, Func{Sprite?}, int)"/> so two mods declaring
    /// the same id share one entry.
    /// </summary>
    /// <param name="id">Stable, lower-case id (e.g. "crewmate"). Never shown to players.</param>
    /// <param name="name">Display name, as a translatable <see cref="StringNames"/>.</param>
    /// <param name="icon">Lazy icon provider; null falls back to the API trophy.</param>
    /// <param name="order">Sort order in the left column; built-ins use 0-30.</param>
    public AchievementCategory(string id, StringNames name, Func<Sprite?>? icon = null, int order = 100)
    {
        Id = id;
        Name = name;
        Order = order;
        _iconProvider = icon;
    }

    /// <summary>
    /// Creates a category from plain text, registered as a hard-coded English string.
    /// Prefer <see cref="AchievementsManager.GetOrCreateCategory(string, StringNames, Func{Sprite?}, int)"/> so two mods declaring
    /// the same id share one entry.
    /// </summary>
    /// <param name="id">Stable, lower-case id (e.g. "crewmate"). Never shown to players.</param>
    /// <param name="name">Display name.</param>
    /// <param name="icon">Lazy icon provider; null falls back to the API trophy.</param>
    /// <param name="order">Sort order in the left column; built-ins use 0-30.</param>
    public AchievementCategory(string id, string name, Func<Sprite?>? icon = null, int order = 100)
        : this(id, CustomStringName.CreateAndRegister(name), icon, order)
    {
    }

    public string Id { get; }

    /// <summary>
    /// The display name, as a translatable <see cref="StringNames"/> (see <see cref="AchievementsStrings.GetText"/>).
    /// </summary>
    public StringNames Name { get; set; }
    public int Order { get; set; }

    /// <summary>
    /// Sets the icon provider, replacing any existing one.
    /// </summary>
    public void SetIcon(Func<Sprite?> icon)
    {
        _iconProvider = icon;
        _icon = null;
    }

    /// <summary>
    /// Sets the icon provider only if none is set yet. Lets mods supply artwork for the built-in
    /// categories without fighting over it.
    /// </summary>
    public void SetDefaultIcon(Func<Sprite?> icon)
    {
        if (_iconProvider == null) SetIcon(icon);
    }

    /// <summary>
    /// Resolves the icon, loading it lazily and keeping it alive across scene changes.
    /// </summary>
    public Sprite GetIcon()
    {
        if (_icon == null && _iconProvider != null) _icon = SpriteTools.PreserveSprite(_iconProvider());
        return _icon != null ? _icon : Fallback;
    }

    private static Sprite? _fallback;
    private static Sprite Fallback => _fallback ??= Assets.LoadSprite("AchievementsIcon");
}
