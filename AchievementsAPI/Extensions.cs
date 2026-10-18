using System;
using System.Collections;
using UnityEngine;

// Completely stole this from MiraAPI, thanks hehe

namespace AchievementsAPI;

public static class Extensions
{
    /// <summary>
    /// Destroys the <see cref="GameObject"/> properly.
    /// </summary>
    /// <param name="obj">The <see cref="GameObject"/> to destroy.</param>
    /// <param name="clearGc">Whether to run the garbage collector immediately.</param>
    public static void DeepDestroy(this GameObject obj, bool clearGc = true)
    { 
        var requiredVersion = new Version(2026, 6, 5);
        var version = Version.Parse(Application.version);
        var needsDeepDestroy = version >= requiredVersion;
        if (needsDeepDestroy)
        {
            Coroutines.Start(Nuke(obj, clearGc));
        }
        else
        {
            obj?.Destroy();
        }
    }

    private static IEnumerator Nuke(GameObject? go, bool clearGc)
    {
        if (go == null)
            yield break;

        try
        {
            go.transform.SetParent(null, false);
        }
        catch
        {
            // ignored
        }

        try
        {
            go.SetActive(false);
        }
        catch
        {
            // ignored
        }

        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null)
                continue;

            try
            {
                mb.StopAllCoroutines();
            }
            catch
            {
                // ignored
            }

            try
            {
                mb.enabled = false;
            }
            catch
            {
                // ignored
            }
        }

        foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
                continue;

            try
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat != null)
                        UnityEngine.Object.Destroy(mat);
                }
            }
            catch
            {
                // ignored
            }
        }

        foreach (var filter in go.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter == null)
                continue;

            try
            {
                var mesh = filter.mesh;
                if (mesh != null)
                    UnityEngine.Object.Destroy(mesh);
            }
            catch
            {
                // ignored
            }
        }

        UnityEngine.Object.Destroy(go);
        yield return null;
        if (clearGc)
        {
            yield return CoFreeResources();
        }
    }

    /// <summary>
    /// Clears up the Garbage Collector manually if necessary.
    /// </summary>
    public static void ClearGarbageCollector()
    {
        Coroutines.Start(CoFreeResources());
    }

    private static IEnumerator CoFreeResources()
    {
        yield return Resources.UnloadUnusedAssets();

        GC.Collect(0, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(0, GCCollectionMode.Forced, blocking: true);
    }
}