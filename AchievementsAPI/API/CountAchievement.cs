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
    public CountAchievement(StringNames name, StringNames description, string iconPath, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null) : base(name, description, iconPath, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly())
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    public CountAchievement(StringNames name, StringNames description, Sprite icon, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null) : base(name, description, icon, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly())
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    public CountAchievement(StringNames name, StringNames description, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null) : base(name, description, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly())
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    public CountAchievement(string name, string description, string iconPath, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null) : base(name, description, iconPath, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly())
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    public CountAchievement(string name, string description, Sprite icon, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null) : base(name, description, icon, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly())
    {
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
    public CountAchievement(string name, string description, int currentValue, int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false, bool hideProgress = false, System.Reflection.Assembly? assembly = null) : base(name, description, rarity, hidden, hideRarity, assembly ?? System.Reflection.Assembly.GetCallingAssembly())
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
        AchievementsManager.RaiseReset(this);
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
            if (showOnUI && !Hidden) AchievementToastQueue.Enqueue(this, true);
            AchievementsManager.RaiseProgressed(this);
            return;
        }
        if (showOnUI && !Unlocked) AchievementToastQueue.Enqueue(this, false);
        AchievementStorage.AchievementStorageUpdate(this, value, Unlocked);
        AchievementsManager.RaiseProgressed(this);
    }
}

public enum AchPersistence
{
    ThroughoutSessions,
    ThroughoutRounds,
    ResetOnRoundStart,
    ResetOnMeetingStart
}