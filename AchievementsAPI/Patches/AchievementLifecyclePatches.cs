using System.Linq;
using AchievementsAPI.API;
using HarmonyLib;

namespace AchievementsAPI.Patches;

// For resetting achievements that don't persist, applying reset triggers, and flushing deferred toasts

[HarmonyPatch]
public static class AchievementLifecycle
{
    internal static void OnGameEnd()
    {
        ResetProgressPersistence(achievement => achievement.ProgressPersists != AchPersistence.ThroughoutSessions);
        ResetTriggered(AchResetTrigger.OnGameEnd);
        AchievementToastQueue.Flush(AchToastTiming.OnGameEnd);
        AchievementToastQueue.FlushAll();
        AchievementStorage.FlushPendingSave();
    }

    internal static void OnMeetingStart()
    {
        ResetProgressPersistence(achievement => achievement.ProgressPersists == AchPersistence.ResetOnMeetingStart);
        ResetTriggered(AchResetTrigger.OnMeetingStart);
        AchievementToastQueue.Flush(AchToastTiming.OnMeetingStart);
    }

    internal static void OnMeetingEnd()
    {
        ResetTriggered(AchResetTrigger.OnMeetingEnd);
        AchievementToastQueue.Flush(AchToastTiming.OnMeetingEnd);
    }

    internal static void OnRoundStart()
    {
        ResetProgressPersistence(achievement => achievement.ProgressPersists == AchPersistence.ResetOnRoundStart);
        ResetTriggered(AchResetTrigger.OnRoundStart);
        AchievementToastQueue.Flush(AchToastTiming.OnRoundStart);
    }

    private static void ResetProgressPersistence(System.Func<CountAchievement, bool> predicate)
    {
        var any = false;
        foreach (var achievement in AchievementsManager.Tabs
                     .SelectMany(tab => tab.GetAchievements())
                     .OfType<CountAchievement>())
        {
            if (achievement.CurrentValue != achievement.RequiredValue && predicate(achievement))
            {
                AchievementStorage.AchievementDataReset(achievement, 0, null, false);
                any = true;
            }
        }

        if (any) AchievementStorage.Save();
    }

    private static void ResetTriggered(AchResetTrigger trigger)
    {
        var any = false;
        foreach (var achievement in AchievementsManager.Tabs.SelectMany(tab => tab.GetAchievements()))
        {
            if (achievement.ResetTrigger != trigger) continue;
            achievement.Reset(false);
            any = true;
        }

        if (any) AchievementStorage.Save();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnGameEnd))]
    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.OnMatchExited))]
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPrefix]
    public static void OnGameEndPatch()
    {
        OnGameEnd();
    }

    // Progress persistence reset only, for match start and tutorial start (existing behaviour)
    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.OnMatchStart))]
    [HarmonyPatch(typeof(TutorialManager), nameof(TutorialManager.Awake))]
    [HarmonyPrefix]
    public static void OnMatchStartPatch()
    {
        ResetProgressPersistence(achievement => achievement.ProgressPersists != AchPersistence.ThroughoutSessions);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void OnMeetingCalledPatch()
    {
        OnMeetingStart();
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    [HarmonyPostfix]
    public static void OnMeetingClosedPatch()
    {
        OnMeetingEnd();
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    [HarmonyPrefix]
    public static void OnRoundStartPatch()
    {
        OnRoundStart();
    }
}
