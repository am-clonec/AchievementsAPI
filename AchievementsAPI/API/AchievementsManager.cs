using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using Reactor.Localization.Utilities;

namespace AchievementsAPI.API;

public static class AchievementsManager
{
    public static List<AchievementsTab> Tabs = new();

    /// <summary>
    /// Shared categories shown in the Achievements Menu, seeded with the built-ins from
    /// <see cref="AchievementCategories"/>. Mods may add more via <see cref="GetOrCreateCategory"/>.
    /// </summary>
    public static List<AchievementCategory> Categories = new()
    {
        new AchievementCategory(AchievementCategories.General, AchievementsStrings.CategoryGeneral, order: 0),
        new AchievementCategory(AchievementCategories.Crewmate, AchievementsStrings.CategoryCrewmate, order: 10),
        new AchievementCategory(AchievementCategories.Impostor, AchievementsStrings.CategoryImpostor, order: 20),
        new AchievementCategory(AchievementCategories.Neutral, AchievementsStrings.CategoryNeutral, order: 30),
    };

    /// <summary>
    /// Gets a category by id, or null if none is registered.
    /// </summary>
    public static AchievementCategory? GetCategory(string id)
    {
        return Categories.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Gets the category with the given id, creating it if needed. If it already exists the existing
    /// entry is returned untouched, so several mods can declare the same category safely.
    /// </summary>
    public static AchievementCategory GetOrCreateCategory(string id, StringNames name, Func<UnityEngine.Sprite?>? icon = null, int order = 100)
    {
        var existing = GetCategory(id);
        if (existing != null) return existing;
        var category = new AchievementCategory(id, name, icon, order);
        Categories.Add(category);
        return category;
    }

    /// <summary>
    /// Gets the category with the given id, creating it from plain text (registered as a
    /// hard-coded English string) if needed. If it already exists the existing entry is
    /// returned untouched, so several mods can declare the same category safely.
    /// </summary>
    public static AchievementCategory GetOrCreateCategory(string id, string name, Func<UnityEngine.Sprite?>? icon = null, int order = 100)
    {
        return GetOrCreateCategory(id, CustomStringName.CreateAndRegister(name), icon, order);
    }

    /// <summary>
    /// Resolves the category an achievement is displayed under: its own <see cref="BaseAchievement.Category"/>,
    /// else its tab's <see cref="AchievementsTab.CategoryId"/>. An id nobody registered is created on the
    /// fly (display name = the id, trophy icon) so a mod can introduce a category just by naming it;
    /// register it with <see cref="GetOrCreateCategory"/> to give it a proper name, icon and order.
    /// </summary>
    public static AchievementCategory ResolveCategory(AchievementsTab tab, BaseAchievement achievement)
    {
        var id = achievement.Category ?? tab.CategoryId;
        if (string.IsNullOrWhiteSpace(id)) id = AchievementCategories.General;
        return GetOrCreateCategory(id, id);
    }

    /// <summary>
    /// Looks up the BepInEx plugin name for an assembly, or null if it isn't a loaded plugin.
    /// </summary>
    public static string? GetPluginName(Assembly assembly)
    {
        return IL2CPPChainloader.Instance?.Plugins.Values
            .FirstOrDefault(p => p.Instance?.GetType().Assembly == assembly)?.Metadata.Name;
    }

    /// <summary>
    /// Registers an achievements tab. Does nothing if a tab of the same type is already
    /// registered (unless it is a <see cref="SoftAchievementsTab"/>).
    /// </summary>
    /// <param name="tab">The tab to register.</param>
    public static void RegisterTab(AchievementsTab tab)
    {
        if (tab is not SoftAchievementsTab && Tabs.Any(x => x.GetType() == tab.GetType())) return;
        Tabs.Add(tab);
    }

    /// <summary>
    /// Gets a registered tab by its name.
    /// </summary>
    /// <param name="name">The <see cref="AchievementsTab.Name"/> of the tab.</param>
    /// <returns>The tab, or null if no tab with that name is registered.</returns>
    public static AchievementsTab? GetTab(StringNames name)
    {
        return Tabs.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Finds a registered achievement by Id across all tabs.
    /// </summary>
    public static (AchievementsTab Tab, BaseAchievement Achievement)? FindAchievement(string id)
    {
        foreach (var tab in Tabs)
        {
            var achievement = tab.GetAchievements().FirstOrDefault(x => x.Id == id);
            if (achievement != null) return (tab, achievement);
        }
        return null;
    }

    public static void Initialize()
    {
        RegisterTab(new ExampleTab());
        RegisterTab(new DeveloperTab());
        IL2CPPChainloader.Instance.PluginLoad += (info, assembly, arg3) => InitializeForAssembly(assembly);
    }

    private static void InitializeForAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(AchievementsTab)) && !x.IsAbstract);
        foreach (var type in types)
        {
            RegisterTab(Activator.CreateInstance(type) as AchievementsTab);
        }
    }
}