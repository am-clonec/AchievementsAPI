using AchievementsAPI.API;
using HarmonyLib;

namespace AchievementsAPI.Patches.MainMenu;
[HarmonyPatch]
public class MainMenuPatches
{
    private static bool _storageInitialized = false;
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    [HarmonyPatch(Priority.Last)]
    [HarmonyPostfix]
    public static void OnMainMenuAwakePostfix(MainMenuManager __instance)
    {
        MainMenuButtons.SetUp(__instance);
        if (!_storageInitialized)
        {
            AchievementStorage.Load();
            _storageInitialized = true;
            foreach (var tab in AchievementsManager.Tabs)
            {
                AchievementStorage.AchievementStorageGet(tab);
            }

            Coroutines.Start(XboxToast.CoPrewarm());

            if (AchievementsManager.Tabs.Count == 1)
            {
                AchievementsTabSingleton<ExampleTab>.Instance.achievement.Unlock();
            }
        }
    }
}