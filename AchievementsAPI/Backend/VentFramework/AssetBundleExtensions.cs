using Il2CppInterop.Runtime;
using UnityEngine;

namespace AchievementsAPI.Backend;

internal static class AssetBundleExtensions
{
    // https://github.com/NuclearPowered/Reactor/blob/master/Reactor/Utilities/Extensions/AssetBundleExtensions.cs
    public static T? LoadAsset<T>(this AssetBundle bundle, string name) where T : UnityEngine.Object
    {
        return bundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
    }
}
