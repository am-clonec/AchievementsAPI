using BepInEx.Configuration;

namespace AchievementsAPI.API;

public enum ToastPosition { BottomCenter, BottomLeft, BottomRight, TopCenter, TopLeft, TopRight }

/// <summary>
/// Configuration for achievement toast notifications, backed by the plugin's BepInEx config file.
/// </summary>
public static class AchievementsConfig
{
    /// <summary>
    /// Whether achievement toasts are shown at all. Unlocking still happens when disabled.
    /// </summary>
    public static ConfigEntry<bool> ToastsEnabled { get; private set; } = null!;

    /// <summary>
    /// Seconds a toast stays fully visible on screen.
    /// </summary>
    public static ConfigEntry<float> ToastDuration { get; private set; } = null!;

    /// <summary>
    /// Seconds a rare (rarity above 0) toast stays fully visible on screen.
    /// </summary>
    public static ConfigEntry<float> RareToastDuration { get; private set; } = null!;

    /// <summary>
    /// Whether a sound plays when an achievement unlock toast appears.
    /// </summary>
    public static ConfigEntry<bool> ToastSoundEnabled { get; private set; } = null!;

    /// <summary>
    /// Volume of the achievement unlock sound.
    /// </summary>
    public static ConfigEntry<float> ToastVolume { get; private set; } = null!;

    /// <summary>
    /// Where achievement toasts appear on screen.
    /// </summary>
    public static ConfigEntry<ToastPosition> Position { get; private set; } = null!;

    /// <summary>
    /// Whether a normal click opens the achievement from a toast (for trackpads).
    /// </summary>
    public static ConfigEntry<bool> TrackpadMode { get; private set; } = null!;


    internal static void Bind(ConfigFile config)
    {
        const string section = "Toasts";

        ToastsEnabled = config.Bind(section, nameof(ToastsEnabled), true,
            "Whether achievement toast popups are shown.");

        ToastDuration = config.Bind(section, nameof(ToastDuration), 4.4f,
            new ConfigDescription(
                "Seconds an achievement toast stays fully visible on screen.",
                new AcceptableValueRange<float>(1f, 10f)));

        RareToastDuration = config.Bind(section, nameof(RareToastDuration), 6.0f,
            new ConfigDescription(
                "Seconds a rare (rarity above 0) achievement toast stays fully visible on screen.",
                new AcceptableValueRange<float>(1f, 10f)));

        ToastSoundEnabled = config.Bind(section, nameof(ToastSoundEnabled), true,
            "Whether a sound plays when an achievement unlock toast appears.");

        ToastVolume = config.Bind(section, nameof(ToastVolume), 0.5f,
            new ConfigDescription(
                "Volume of the achievement unlock sound.",
                new AcceptableValueRange<float>(0f, 1f)));

        Position = config.Bind(section, nameof(Position), ToastPosition.BottomCenter,
            "Where achievement toasts appear on screen.");

        TrackpadMode = config.Bind(section, nameof(TrackpadMode), false,
            "Open achievements from a toast with a normal click instead of middle-click (for trackpads).");

    }
}
