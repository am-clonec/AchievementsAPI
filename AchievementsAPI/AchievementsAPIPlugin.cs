using System.Collections.Generic;
using AchievementsAPI.API;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace AchievementsAPI;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
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