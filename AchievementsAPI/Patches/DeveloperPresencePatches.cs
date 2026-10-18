using System.Collections;
using AchievementsAPI.API;
using HarmonyLib;
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
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    [HarmonyPostfix]
    public static void OnIntroDestroyedPatch()
    {
        DeveloperPresence.Announce();
    }
}
