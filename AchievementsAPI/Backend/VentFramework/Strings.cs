using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace AchievementsAPI.Backend;

// Replacement for Reactor's CustomStringName: vanilla StringNames ids are non-negative,
// so custom ids are allocated descending from a large negative start value.
internal static class Strings
{
    private static readonly Dictionary<StringNames, string> Texts = new();

    private static int _nextId = -1_000_000;

    public static StringNames Register(string text)
    {
        var name = (StringNames) _nextId--;
        Texts[name] = text;
        return name;
    }

    internal static bool TryGet(StringNames id, out string text) => Texts.TryGetValue(id, out text!);
}

// Mirrors Reactor.Localization.Patches.GetStringPatch: serves our custom StringNames
// before the vanilla translation lookup runs.
[HarmonyPatch]
internal static class CustomStringNamePatch
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new System.Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetStringWithDefault), new System.Type[] { typeof(StringNames), typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    [HarmonyPrefix]
    private static bool StringNamesPatch(StringNames id, out string __result) =>
        !Strings.TryGet(id, out __result!);
}
