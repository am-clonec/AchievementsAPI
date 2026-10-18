using System;
using AchievementsAPI.API;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace AchievementsAPI;

/// <summary>
/// Flushes any pending debounced achievements save when the game quits.
/// </summary>
[RegisterInIl2Cpp]
public sealed class AchievementStorageFlusher(IntPtr ptr) : MonoBehaviour(ptr)
{
    private void OnApplicationQuit()
    {
        AchievementStorage.FlushPendingSave();
    }
}
