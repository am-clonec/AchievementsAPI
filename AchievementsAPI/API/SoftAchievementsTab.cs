using System;
using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// A concrete <see cref="AchievementsTab"/> that can be created at runtime without subclassing.
/// </summary>
public class SoftAchievementsTab : AchievementsTab
{
    private readonly StringNames _name;
    private readonly Color? _tabColor;
    private readonly Sprite? _icon;
    private readonly string? _categoryId;
    private readonly string? _modName;
    private readonly Func<Sprite>? _modIcon;

    /// <summary>
    /// Creates a new soft-registered achievements tab.
    /// </summary>
    /// <param name="name">The name of the tab, displayed in UI, as a translatable <see cref="StringNames"/>.</param>
    /// <param name="tabColor">The color of the Achievements Menu background for this tab.</param>
    /// <param name="icon">The icon of the tab in the Achievements Menu.</param>
    /// <param name="categoryId">The shared category this tab's achievements are listed under; null uses
    /// <see cref="AchievementCategories.General"/>.</param>
    /// <param name="modName">The mod this tab belongs to; null uses the BepInEx plugin name.</param>
    /// <param name="modIcon">Lazily resolves the icon shown next to <paramref name="modName"/> in group
    /// headers; null uses the tab icon.</param>
    public SoftAchievementsTab(StringNames name, Color? tabColor = null, Sprite? icon = null,
        string? categoryId = null, string? modName = null, Func<Sprite?>? modIcon = null)
    {
        _name = name;
        _tabColor = tabColor;
        _icon = icon;
        _categoryId = categoryId;
        _modName = modName;
        _modIcon = modIcon;
    }

    /// <summary>
    /// Creates a new soft-registered achievements tab from plain text,
    /// registered as a hard-coded English string.
    /// </summary>
    /// <param name="name">The name of the tab, displayed in UI.</param>
    /// <param name="tabColor">The color of the Achievements Menu background for this tab.</param>
    /// <param name="icon">The icon of the tab in the Achievements Menu.</param>
    /// <param name="categoryId">The shared category this tab's achievements are listed under; null uses
    /// <see cref="AchievementCategories.General"/>.</param>
    /// <param name="modName">The mod this tab belongs to; null uses the BepInEx plugin name.</param>
    /// <param name="modIcon">Lazily resolves the icon shown next to <paramref name="modName"/> in group
    /// headers; null uses the tab icon.</param>
    public SoftAchievementsTab(string name, Color? tabColor = null, Sprite? icon = null,
        string? categoryId = null, string? modName = null, Func<Sprite?>? modIcon = null)
        : this(Strings.Register(name), tabColor, icon, categoryId, modName, modIcon)
    {
    }

    /// <inheritdoc />
    public override StringNames Name => _name;

    /// <inheritdoc />
    public override string CategoryId => _categoryId ?? base.CategoryId;

    /// <inheritdoc />
    public override string ModName => _modName ?? base.ModName;

    /// <inheritdoc />
    public override Sprite ModIcon => _modIcon?.Invoke() ?? base.ModIcon;

    /// <inheritdoc />
    public override Color GetTabColor()
    {
        return _tabColor ?? base.GetTabColor();
    }

    /// <inheritdoc />
    public override Sprite GetIcon()
    {
        return _icon ? _icon : base.GetIcon();
    }
}
