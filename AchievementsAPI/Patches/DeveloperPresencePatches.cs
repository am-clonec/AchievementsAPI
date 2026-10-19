using System.Collections;
using System.Reflection;
using AchievementsAPI.API;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;

namespace AchievementsAPI.Patches;

// Announces the local player as a developer (if registered via DeveloperPresence.RegisterLocalDeveloper)
// and tracks the match-played unlock for DeveloperTab.PlayedWithDeveloper.

[HarmonyPatch]
public static class DeveloperPresencePatches
{
    // I joined/created a lobby
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void OnLobbyStartPatch()
    {
        DeveloperPresence.ResetMatch();
        DeveloperPresence.Announce();
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    [HarmonyPostfix]
    public static void OnEndGamePatch()
    {
        DeveloperPresence.OnMatchEnded();
    }

    // Someone else joined: give them time to spawn our PlayerControl before the
    // RPC lands; an RPC to a net object the receiver hasn't spawned yet is dropped.
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    [HarmonyPostfix]
    public static void OnPlayerJoinedPatch()
    {
        Coroutines.Start(CoAnnounceDelayed());
    }

    private static IEnumerator CoAnnounceDelayed()
    {
        yield return new WaitForSeconds(3f);
        DeveloperPresence.Announce();
    }

    // Guaranteed everyone is spawned by the time the intro ends
    [HarmonyPatch]
    public static class OnIntroDestroyedPatch
    {
        public static MethodBase TargetMethod() => 
            StateMachineWrapper<IntroCutscene>.GetStateMachineMoveNext(nameof(IntroCutscene.CoBegin))!;

        public static void Postfix(Il2CppObjectBase __instance)
        {
            if (new StateMachineWrapper<IntroCutscene>(__instance).State != -1) return;
            DeveloperPresence.Announce();
        }
    }
}
