
namespace AchievementsAPI.API;

/// <summary>
/// Every piece of player-facing text the API renders itself (menu chrome, toasts and the
/// built-in categories), as Reactor <see cref="StringNames"/> so all of it can be translated
/// through the localization providers registered with Reactor's
/// <c>Reactor.Localization.LocalizationManager</c>. The English text is registered as the
/// hard-coded fallback; a provider with a higher priority — like a translation pack — wins.
/// </summary>
public static class AchievementsStrings
{
    public static readonly StringNames MenuTitle = Create("Achievements");
    public static readonly StringNames Settings = Create("Settings");
    public static readonly StringNames SkeldPoints = Create("Skeld Points");
    public static readonly StringNames Back = Create("Back");
    public static readonly StringNames Preview = Create("Preview");

    public static readonly StringNames FilterAll = Create("All");
    public static readonly StringNames FilterCompleted = Create("Completed");
    public static readonly StringNames FilterInProgress = Create("In Progress");
    public static readonly StringNames FilterLocked = Create("Locked");

    public static readonly StringNames SearchPlaceholder = Create("Search achievements...");
    public static readonly StringNames EmptyNoResults = Create("No achievements match your search");
    public static readonly StringNames FilterAllMods = Create("All mods");

    public static readonly StringNames EmptyAllUnlocked = Create("Everything unlocked!");
    public static readonly StringNames EmptyNothingHere = Create("Nothing here yet");

    public static readonly StringNames StatusUnlocked = Create("Unlocked");
    public static readonly StringNames StatusLocked = Create("Locked");
    public static readonly StringNames DetailRarity = Create("Rarity");
    public static readonly StringNames DetailStatus = Create("Status");
    public static readonly StringNames DetailProgress = Create("Progress");
    public static readonly StringNames HiddenAchievement = Create("Hidden Achievement");

    public static readonly StringNames RarityCommon = Create("Common");
    public static readonly StringNames RarityRare = Create("Rare");
    public static readonly StringNames RarityEpic = Create("Epic");
    public static readonly StringNames RarityLegendary = Create("Legendary");

    public static readonly StringNames ToastUnlocked = Create("Achievement Unlocked!");
    public static readonly StringNames ToastProgressed = Create("Achievement Progressed!");
    public static readonly StringNames ToastHintController = Create("Hold Y to view");
    public static readonly StringNames ToastHintClick = Create("Click to view");
    public static readonly StringNames ToastHintMiddleClick = Create("Middle-click to view");
    public static readonly StringNames PreviewAchievement = Create("Preview Achievement");

    public static readonly StringNames CategoryGeneral = Create("General");
    public static readonly StringNames CategoryCrewmate = Create("Crewmate");
    public static readonly StringNames CategoryImpostor = Create("Impostor");
    public static readonly StringNames CategoryNeutral = Create("Neutral");

    public static readonly StringNames SettingToasts = Create("Show toasts");
    public static readonly StringNames SettingProgressToasts = Create("Progress toasts");
    public static readonly StringNames SettingSound = Create("Play sound");
    public static readonly StringNames SettingVolume = Create("Volume");
    public static readonly StringNames SettingDuration = Create("Toast duration");
    public static readonly StringNames SettingRareDuration = Create("Rare duration");
    public static readonly StringNames SettingPosition = Create("Position");
    public static readonly StringNames SettingTrackpadMode = Create("Trackpad mode");
    public static readonly StringNames SettingResetAll = Create("Reset all achievements");
    public static readonly StringNames ResetAllButton = Create("Reset");
    public static readonly StringNames ResetAllTitle = Create("Wipe everything?");
    public static readonly StringNames ResetAllBody = Create("All your achievements and progress will be gone for good. No takebacks.");
    public static readonly StringNames ResetAllConfirm = Create("I'm ready");
    public static readonly StringNames ResetAllCancel = Create("Nevermind");

    public static readonly StringNames ExampleTabName = Create("Example Tab");
    public static readonly StringNames DeveloperTabName = Create("Developer Tab");

    private static StringNames Create(string text) => Strings.Register(text);

    /// <summary>
    /// The <see cref="StringNames"/> of a rarity level.
    /// </summary>
    public static StringNames RarityName(int rarity) => rarity switch
    {
        1 => RarityRare,
        2 => RarityEpic,
        3 => RarityLegendary,
        _ => RarityCommon
    };

    /// <summary>
    /// Resolves a <see cref="StringNames"/> in the current language. This goes through the game's
    /// <see cref="TranslationController"/>, which Reactor patches to query every registered
    /// localization provider, so the result follows the current language and any translation packs.
    /// </summary>
    /// <param name="stringName">The string name to resolve.</param>
    /// <returns>
    /// The translated text, or an empty string if the translation system is not up yet
    /// (before the game boots into the main menu).
    /// </returns>
    public static string GetText(StringNames stringName)
        => TranslationController.Instance != null
            ? TranslationController.Instance.GetString(stringName)
            : string.Empty;
}
