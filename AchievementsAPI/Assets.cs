using System.Reflection;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace AchievementsAPI;

public class Assets
{
    public static AssetBundle assetBundle { get; set; } = AssetBundleManager.Load("achievements");
    public static GameObject achievementToastCanvasPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("ToastCanvas").DontDestroy().DontDestroyOnLoad().DontDestroyOnLoad();
    public static GameObject achievementToastPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("Toast").DontDestroy().DontDestroyOnLoad().DontDestroyOnLoad();
    public static Sprite StarSprite { get; set; } = assetBundle.LoadAsset<Sprite>("Star");

    private static AudioClip? _lockChime;
    /// <summary>
    /// The chime played when an achievement unlock toast appears.
    /// </summary>
    public static AudioClip? LockChime => Clip(ref _lockChime, "AchievementsAPI.Resources.LockChime.wav");

    private static AudioClip? _rareChime;
    /// <summary>
    /// The chime played when a rare (rarity above 0) achievement unlock toast appears.
    /// </summary>
    public static AudioClip? RareChime => Clip(ref _rareChime, "AchievementsAPI.Resources.RareChime.wav");

    // `cache == null` is a Unity null check, so a destroyed clip is reloaded.
    private static AudioClip? Clip(ref AudioClip? cache, string resource)
        => cache == null ? cache = WavLoader.LoadFromResource(typeof(Assets).Assembly, resource) : cache;
}