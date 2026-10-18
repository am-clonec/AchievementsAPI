using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace AchievementsAPI.Backend;

// Port of https://github.com/NuclearPowered/Reactor/blob/master/Reactor/Utilities/AssetBundleManager.cs
internal static class Bundles
{
    private const string BundleExtension = ".bundle";

    private static string GetTargetName(bool includeArchitecture)
    {
        string operatingSystem;

        if (OperatingSystem.IsWindows())
        {
            operatingSystem = "win";
        }
        else if (OperatingSystem.IsLinux())
        {
            operatingSystem = "linux";
        }
        else if (OperatingSystem.IsAndroid())
        {
            operatingSystem = "android";
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        if (!includeArchitecture)
        {
            return operatingSystem;
        }

        return $"{operatingSystem}-{RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException(),
        }}";
    }

    private static string GetFileName(string assetBundleName, bool includeArchitecture) =>
        $"{assetBundleName}-{GetTargetName(includeArchitecture)}{BundleExtension}";

    private static bool TryFindFile(Assembly assembly, string fileName, [NotNullWhen(true)] out string? path)
    {
        var pluginDirectoryPath = Path.GetDirectoryName(assembly.Location);
        if (pluginDirectoryPath != null)
        {
            var filePath = Path.Combine(pluginDirectoryPath, fileName);
            if (File.Exists(filePath))
            {
                path = filePath;
                return true;
            }
        }

        path = null;
        return false;
    }

    private static bool TryLoadResource(Assembly assembly, string fileName, [NotNullWhen(true)] out Il2CppStructArray<byte>? data)
    {
        var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(n => n.EndsWith(fileName, StringComparison.Ordinal));
        if (resourceName != null)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Resource stream was null");

            data = stream.ReadFully();
            return true;
        }

        data = null;
        return false;
    }

    /// <summary>
    /// Synchronously loads an <see cref="AssetBundle"/> named <paramref name="name"/> from the calling assembly.
    /// </summary>
    /// <exception cref="IOException">Couldn't find an asset bundle named <paramref name="name"/>.</exception>
    public static AssetBundle Load(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileName = GetFileName(name, includeArchitecture: true);

        if (TryFindFile(assembly, fileName, out var filePath))
        {
            return AssetBundle.LoadFromFile(filePath);
        }

        if (TryLoadResource(assembly, fileName, out var data))
        {
            return AssetBundle.LoadFromMemory(data);
        }

        throw new IOException($"Couldn't find an asset bundle named {fileName}");
    }
}
