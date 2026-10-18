using System.Collections.Generic;
using UnityEngine;

namespace AchievementsAPI;

public class Assets
{
    public static AssetBundle assetBundle { get; set; } = Bundles.Load("achievements");
    public static GameObject achievementToastCanvasPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("ToastCanvas").DontDestroy().DontDestroyOnLoad().DontDestroyOnLoad();
    public static GameObject achievementToastPrefab { get; set; } = assetBundle.LoadAsset<GameObject>("Toast").DontDestroy().DontDestroyOnLoad().DontDestroyOnLoad();
    public static Sprite StarSprite { get; set; } = assetBundle.LoadAsset<Sprite>("Star");

    private static AudioClip? _lockChime;
    /// <summary>
    /// The chime played when an achievement unlock toast appears.
    /// </summary>
    public static AudioClip? LockChime => Clip(ref _lockChime, "LockChime");

    private static AudioClip? _rareChime;
    /// <summary>
    /// The chime played when a rare (rarity above 0) achievement unlock toast appears.
    /// </summary>
    public static AudioClip? RareChime => Clip(ref _rareChime, "RareChime");

    private static AudioClip? _volumeTick;
    /// <summary>
    /// The short tick played while dragging the toast volume slider.
    /// </summary>
    public static AudioClip? VolumeTick => Clip(ref _volumeTick, "VolumeTick");

    // `cache == null` is a Unity null check, so a destroyed clip is reloaded.
    private static AudioClip? Clip(ref AudioClip? cache, string assetName)
    {
        if (cache != null) return cache;
        cache = assetBundle.LoadAsset<AudioClip>(assetName);
        cache?.DontUnload();
        return cache;
    }

    /// <summary>
    /// Loads a sprite shipped in the achievements asset bundle by its asset
    /// name (file name without extension). Sprites are kept alive across scene
    /// loads and cached.
    /// </summary>
    public static Sprite LoadSprite(string assetName)
    {
        if (_spriteCache.TryGetValue(assetName, out var sprite) && sprite != null)
            return sprite;
        sprite = assetBundle.LoadAsset<Sprite>(assetName);
        SpriteTools.PreserveSprite(sprite);
        _spriteCache[assetName] = sprite;
        return sprite;
    }

    /// <summary>
    /// Loads a texture shipped in the achievements asset bundle by its asset
    /// name (file name without extension).
    /// </summary>
    public static Texture2D LoadTexture(string assetName)
        => assetBundle.LoadAsset<Texture2D>(assetName);

    private static readonly Dictionary<string, Sprite> _spriteCache = new();
}