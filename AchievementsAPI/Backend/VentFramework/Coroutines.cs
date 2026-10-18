using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

namespace AchievementsAPI.Backend;

// Port of https://github.com/NuclearPowered/Reactor/blob/master/Reactor/Utilities/Coroutines.cs
internal static class Coroutines
{
    [RegisterInIl2Cpp]
    private sealed class CoroutineRunner : MonoBehaviour
    {
        public CoroutineRunner(IntPtr ptr) : base(ptr)
        {
        }
    }

    private static readonly Dictionary<IEnumerator, Coroutine> Handles = new();

    private static CoroutineRunner? _runner;

    private static CoroutineRunner Runner => _runner ??= CreateRunner();

    private static CoroutineRunner CreateRunner()
    {
        var gameObject = new GameObject("AchievementsAPI.CoroutineRunner")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        return gameObject.AddComponent<CoroutineRunner>();
    }

    public static IEnumerator Start(IEnumerator coroutine)
    {
        Handles[coroutine] = Runner.StartCoroutine(Wrap(coroutine));
        return coroutine;
    }

    public static void Stop(IEnumerator coroutine)
    {
        if (Handles.Remove(coroutine, out var handle))
        {
            Runner.StopCoroutine(handle);
        }
    }

    private static IEnumerator Wrap(IEnumerator coroutine)
    {
        while (coroutine.MoveNext())
        {
            yield return coroutine.Current;
        }

        Handles.Remove(coroutine);
    }
}
