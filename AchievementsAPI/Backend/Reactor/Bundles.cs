using UnityEngine;

namespace AchievementsAPI.Backend;

internal static class Bundles
{
    public static AssetBundle Load(string name) => Reactor.Utilities.AssetBundleManager.Load(name);
}
