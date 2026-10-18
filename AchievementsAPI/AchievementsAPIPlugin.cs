using System.Collections.Generic;
using AchievementsAPI.API;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
#if !VENT_FRAMEWORK
using Reactor;
#endif
using UnityEngine;

namespace AchievementsAPI;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
#if VENT_FRAMEWORK
[BepInDependency(VentLib.Vents.Id)]
#else
[BepInDependency(ReactorPlugin.Id)]
#endif
public partial class AchievementsAPIPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    /// <summary>
    /// The loaded plugin instance.
    /// </summary>
    public static AchievementsAPIPlugin Instance { get; private set; } = null!;

    public override void Load()
    {
        Instance = this;
        AchievementsConfig.Bind(Config);
        AchievementsManager.Initialize();
        Harmony.PatchAll();

        var flusher = new GameObject("AchievementStorageFlusher");
        flusher.DontDestroyOnLoad();
        flusher.AddComponent<AchievementStorageFlusher>();
    }
}