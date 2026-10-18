using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// Achievement class for achievements that can increment.
/// </summary>
public class CountAchievement : BaseAchievement
{
    /// <summary>
    /// The current progress for this achievement
    /// </summary>
    public int CurrentValue;
    /// <summary>
    /// The required progress to unlock this achievement.
    /// </summary>
    public int RequiredValue;
    /// <summary>
    /// Defines how the progress persists between games.
    /// </summary>
    public AchPersistence ProgressPersists;
    /// <summary>
    /// Whether to hide the achievement's progress (if the achievement is hidden)
    /// </summary>
    public bool HideProgress;
    /// <summary>
    /// Creates a new count achievement with the specified properties.
    /// </summary>
    /// <param name="name">The achievement's name.</param>
    /// <param name="description">The achievement's description.</param>
    /// <param name="iconPath">The path to the achievement's icon.</param>
    /// <param name="currentValue">The current progress value.</param>
    /// <param name="requiredValue">The required progress to unlock.</param>
    /// <param name="progressPersists">How progress persists between games.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="hideProgress">Whether to hide the progress bar for hidden achievements.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public CountAchievement(StringNames name, StringNames description, string iconPath, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null, string[]? tags = null) : base(name, description, iconPath, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly(), tags)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    /// <summary>
    /// Creates a new count achievement with the specified properties.
    /// </summary>
    /// <param name="name">The achievement's name.</param>
    /// <param name="description">The achievement's description.</param>
    /// <param name="icon">The achievement's icon sprite.</param>
    /// <param name="currentValue">The current progress value.</param>
    /// <param name="requiredValue">The required progress to unlock.</param>
    /// <param name="progressPersists">How progress persists between games.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="hideProgress">Whether to hide the progress bar for hidden achievements.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public CountAchievement(StringNames name, StringNames description, Sprite icon, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null, string[]? tags = null) : base(name, description, icon, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly(), tags)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    /// <summary>
    /// Creates a new count achievement with the specified properties.
    /// </summary>
    /// <param name="name">The achievement's name.</param>
    /// <param name="description">The achievement's description.</param>
    /// <param name="currentValue">The current progress value.</param>
    /// <param name="requiredValue">The required progress to unlock.</param>
    /// <param name="progressPersists">How progress persists between games.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="hideProgress">Whether to hide the progress bar for hidden achievements.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public CountAchievement(StringNames name, StringNames description, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null, string[]? tags = null) : base(name, description, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly(), tags)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    /// <summary>
    /// Creates a new count achievement with the specified properties.
    /// </summary>
    /// <param name="name">The achievement's name (plain text).</param>
    /// <param name="description">The achievement's description (plain text).</param>
    /// <param name="iconPath">The path to the achievement's icon.</param>
    /// <param name="currentValue">The current progress value.</param>
    /// <param name="requiredValue">The required progress to unlock.</param>
    /// <param name="progressPersists">How progress persists between games.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="hideProgress">Whether to hide the progress bar for hidden achievements.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public CountAchievement(string name, string description, string iconPath, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null, string[]? tags = null) : base(name, description, iconPath, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly(), tags)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    /// <summary>
    /// Creates a new count achievement with the specified properties.
    /// </summary>
    /// <param name="name">The achievement's name (plain text).</param>
    /// <param name="description">The achievement's description (plain text).</param>
    /// <param name="icon">The achievement's icon sprite.</param>
    /// <param name="currentValue">The current progress value.</param>
    /// <param name="requiredValue">The required progress to unlock.</param>
    /// <param name="progressPersists">How progress persists between games.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="hideProgress">Whether to hide the progress bar for hidden achievements.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public CountAchievement(string name, string description, Sprite icon, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null, string[]? tags = null) : base(name, description, icon, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly(), tags)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    /// <summary>
    /// Creates a new count achievement with the specified properties.
    /// </summary>
    /// <param name="name">The achievement's name (plain text).</param>
    /// <param name="description">The achievement's description (plain text).</param>
    /// <param name="currentValue">The current progress value.</param>
    /// <param name="requiredValue">The required progress to unlock.</param>
    /// <param name="progressPersists">How progress persists between games.</param>
    /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
    /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
    /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
    /// <param name="hideProgress">Whether to hide the progress bar for hidden achievements.</param>
    /// <param name="assembly">The assembly that owns this achievement.</param>
    /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
    public CountAchievement(string name, string description, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null, string[]? tags = null) : base(name, description, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly(), tags)
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    /// <summary>
    /// Resets this achievement back to its locked state and zeroes its progress, writing through to storage.
    /// </summary>
    /// <param name="save">Whether to save to storage immediately. Pass false when batching resets.</param>
    public override void Reset(bool save = true)
    {
        AchievementToastQueue.Dequeue(this);
        Unlocked = false;
        CurrentValue = 0;
        AchievementStorage.AchievementDataReset(this, 0, false, save);
    }
    /// <summary>
    /// Method to increment the progress of this achievement.
    /// </summary>
    /// <param name="count">The amount to increment by.</param>
    /// <param name="showOnUI">Shows an unlock animation on the hud.</param>
    public void Increment(int count, bool showOnUI = true)
    {
        SetValue(count + CurrentValue, showOnUI);
    }
    /// <summary>
    /// Method to set the progress of this achievement.
    /// </summary>
    /// <param name="value">The progress value.</param>
    /// <param name="showOnUI">Shows an unlock animation on the hud.</param>
    public void SetValue(int value, bool showOnUI = true)
    {
        CurrentValue = value;
        if (CurrentValue >= RequiredValue)
        {
            Unlock(false, false);
            AchievementStorage.AchievementStorageUpdate(this, value, true);
            if (showOnUI) AchievementToastQueue.Enqueue(this, true);
            return;
        }
        if (showOnUI && !Unlocked && !Hidden) AchievementToastQueue.Enqueue(this, false);
        AchievementStorage.AchievementStorageUpdate(this, value, Unlocked);
    }
}

public enum AchPersistence
{
    ThroughoutSessions,
    ThroughoutRounds,
    ResetOnRoundStart,
    ResetOnMeetingStart
}