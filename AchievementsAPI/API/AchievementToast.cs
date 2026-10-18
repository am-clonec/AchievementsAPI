using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;

namespace AchievementsAPI.API;

/// <summary>
/// Entry point for showing achievement toasts. Callers build an immutable
/// <see cref="ToastRequest"/> snapshot which is rendered by <see cref="XboxToast"/>.
/// </summary>
public class AchievementToast
{
    /// <summary>
    /// An immutable snapshot of a toast's contents, captured at enqueue time so
    /// deferred toasts display the values they were earned with.
    /// </summary>
    public sealed record ToastRequest(string Title, string Subtitle, Sprite? Icon, int Rarity, bool IsUnlock, Sprite? BgSprite, string AchievementId);

    public static void ShowAndDeleteToast(BaseAchievement achievement)
    {
        Show(new ToastRequest(AchievementsStrings.GetText(AchievementsStrings.ToastUnlocked),
            achievement.GetName(), achievement.Icon,
            achievement.Rarity, true, achievement.ToastBgSprite, achievement.Id));
    }

    public static void ShowAndDeleteToast(CountAchievement achievement, bool unlocked = false)
    {
        string title;
        string subtitle;
        if (achievement.Hidden && achievement.HideProgress && !unlocked)
        {
            title = AchievementsStrings.GetText(AchievementsStrings.ToastProgressed);
            subtitle = AchievementsStrings.GetText(AchievementsStrings.HiddenAchievement);
        }
        else
        {
            title = AchievementsStrings.GetText(unlocked ? AchievementsStrings.ToastUnlocked : AchievementsStrings.ToastProgressed);
            subtitle = $"{achievement.GetName()} ({achievement.CurrentValue}/{achievement.RequiredValue})";
        }

        Show(new ToastRequest(title, subtitle, achievement.Icon,
            achievement.Rarity, unlocked, achievement.ToastBgSprite, achievement.Id));
    }

    public static IEnumerator CoShowAndDeleteToast(BaseAchievement achievement)
    {
        ShowAndDeleteToast(achievement);
        yield break;
    }

    public static IEnumerator CoShowAndDeleteToast(CountAchievement achievement, bool unlocked = false)
    {
        ShowAndDeleteToast(achievement, unlocked);
        yield break;
    }

    /// <summary>
    /// Enqueues a toast request for display.
    /// </summary>
    public static void Show(ToastRequest request)
    {
        var icon = SpriteTools.PreserveSprite(request.Icon);
        var background = SpriteTools.PreserveSprite(request.BgSprite);
        XboxToast.Enqueue(request with { Icon = icon, BgSprite = background });
    }

    /// <summary>
    /// Shows a preview toast, used by the settings panel. Uses the ExampleTab's
    /// first achievement when available, otherwise a hardcoded preview.
    /// </summary>
    public static void ShowPreview()
    {
        var achievement = AchievementsManager.Tabs.OfType<ExampleTab>().FirstOrDefault()
            ?.GetAchievements().FirstOrDefault();
        if (achievement != null)
        {
            ShowAndDeleteToast(achievement);
            return;
        }

        Show(new ToastRequest(AchievementsStrings.GetText(AchievementsStrings.ToastUnlocked),
            AchievementsStrings.GetText(AchievementsStrings.PreviewAchievement),
            Assets.LoadSprite("ExampleIcon"),
            0, true, null, "preview"));
    }

    private static AudioSource? _chimeSource;
    private static AudioSource? _previewSource;
    private static float _lastPreviewTime = float.NegativeInfinity;
    private const float PreviewInterval = 0.12f;

    /// <summary>
    /// Plays the short volume tick at <paramref name="volume"/>, at most once every
    /// <see cref="PreviewInterval"/> seconds so a fast drag sounds like discrete steps
    /// rather than noise; calls inside that window are dropped.
    /// </summary>
    public static void PlayVolumePreview(float volume)
    {
        if (Time.unscaledTime - _lastPreviewTime < PreviewInterval) return;
        var clip = Assets.VolumeTick;
        if (clip == null) return;
        _lastPreviewTime = Time.unscaledTime;
        if (_previewSource == null) _previewSource = CreateChimeSource();
        _previewSource.Stop();
        _previewSource.clip = clip;
        _previewSource.volume = Mathf.Clamp01(volume);
        _previewSource.Play();
    }

    /// <summary>
    /// Plays the unlock chime, if enabled in the config, through a dedicated
    /// <see cref="AudioSource"/> so the configured volume is honored.
    /// </summary>
    public static void PlayUnlockSound()
    {
        PlayUnlockSound(0);
    }

    /// <summary>
    /// Plays the unlock chime for the given rarity: the rare chime for epic
    /// and legendary achievements (rarity 2 or above, falling back to the lock
    /// chime if it failed to load), the standard lock chime otherwise.
    /// </summary>
    public static void PlayUnlockSound(int rarity)
    {
        if (!AchievementsConfig.ToastSoundEnabled.Value) return;
        var clip = rarity >= 2 ? Assets.RareChime ?? Assets.LockChime : Assets.LockChime;
        if (clip == null) return;
        // Unity null check: a destroyed source is recreated
        if (_chimeSource == null) _chimeSource = CreateChimeSource();
        _chimeSource.PlayOneShot(clip, AchievementsConfig.ToastVolume.Value);
    }

    private static AudioSource CreateChimeSource()
    {
        var go = new GameObject("AchievementSound") { hideFlags = HideFlags.HideInHierarchy };
        UnityEngine.Object.DontDestroyOnLoad(go);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.volume = 1f;
        source.spatialBlend = 0f;
        return source;
    }

    private static TMP_FontAsset? _font;

    /// <summary>
    /// Resolves the TMP font used by toasts: the font from the bundle toast prefab,
    /// falling back to <see cref="TMP_Settings.defaultFontAsset"/>.
    /// </summary>
    public static TMP_FontAsset? ResolveFont()
    {
        if (_font != null) return _font;
        var tmp = Assets.achievementToastPrefab != null
            ? Assets.achievementToastPrefab.transform.Find("AchievementName")?.GetComponent<TextMeshProUGUI>()
            : null;
        _font = tmp != null && tmp.font != null ? tmp.font : TMP_Settings.defaultFontAsset;
        return _font;
    }
}
