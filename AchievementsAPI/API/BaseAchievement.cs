    using System;
    using System.Reflection;
    using Reactor.Localization.Utilities;
    using UnityEngine;

    namespace AchievementsAPI.API;

    /// <summary>
    /// Base Achievement class, used to define achievements.
    /// </summary>
    public class BaseAchievement
    {
        /// <summary>
        /// The achievement's name, as a translatable <see cref="StringNames"/>.
        /// Resolve it in the current language with <see cref="GetName"/>.
        /// </summary>
        public StringNames Name { get; }
        /// <summary>
        /// The achievement's description, as a translatable <see cref="StringNames"/>.
        /// Resolve it in the current language with <see cref="GetDescription"/>.
        /// </summary>
        public StringNames Description { get; }
        /// <summary>
        /// The achievement's icon's path
        /// </summary>
        public string IconPath;

        /// <summary>
        /// Gets or sets the achievement's icon
        /// </summary>
        public virtual Sprite Icon => _sprite;

        /// <summary>
        /// Gets the background sprite for the achievement in the achievements menu.
        /// </summary>
        public virtual Sprite? MenuBgSprite => null;

        /// <summary>
        /// Gets the background sprite for the achievement in the toast pop up.
        /// No longer used by the current toast; kept for compatibility.
        /// </summary>
        public virtual Sprite? ToastBgSprite => null;

        /// <summary>
        /// Gets the offset for the achievement's icon in the achievements menu.
        /// </summary>
        public virtual Vector3 MenuIconOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's title in the achievements menu.
        /// </summary>
        public virtual Vector3 MenuTitleOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's description in the achievements menu.
        /// </summary>
        public virtual Vector3 MenuDescOffset => new(0, 0);


        /// <summary>
        /// Gets the offset for the achievement's sub-icon, if any.
        /// </summary>
        public virtual Vector3 MenuSubIconOffset => new(0, 0);

        /// <summary>
        /// Gets the scale for the achievement's sub-icon, if any.
        /// </summary>
        public virtual Vector3 MenuSubIconScale => new(0.5f, 0.5f, 1);

        /// <summary>
        /// Gets the achievement's sub-icon, if any.
        /// Gets the achievement's sub-icon, if any. Defaults to <see cref="SubIcon"/>.
        /// </summary>
        public virtual Sprite? MenuSubIcon => null;

    /// <summary>
    /// Gets the offset for the achievement's "Obtained" text in the toast pop up.
    /// </summary>
    public virtual Vector3 ToastObtainedOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's title in the toast pop up.
        /// </summary>
        public virtual Vector3 ToastTitleOffset => new(0, 0);

        /// <summary>
        /// Gets the offset for the achievement's icon in the toast pop up.
        /// </summary>
        public virtual Vector3 ToastIconOffset => new(0, 0);

        /// <summary>
        /// Whether the achievement's background becomes colored.
        /// </summary>
        public bool RarityOnBgSprite = true;

        private Sprite _sprite;
        public bool Unlocked;

        /// <summary>
        /// When this achievement was unlocked (UTC), or null if locked or unlocked before this was tracked.
        /// </summary>
        public DateTime? UnlockedAt => Unlocked ? AchievementStorage.GetData(this).UnlockedAt : null;
        /// <summary>
        /// When the achievement is automatically reset back to its locked/unprogressed state.
        /// </summary>
        public AchResetTrigger ResetTrigger = AchResetTrigger.Never;
        /// <summary>
        /// When the achievement's toast popup is shown relative to game events.
        /// </summary>
        public AchToastTiming ToastTiming = AchToastTiming.Immediate;
        /// <summary>
        /// The achievement's rarity:
        /// 0 = default (common)
        /// 1 = rare (blue)
        /// 2 = epic (purple)
        /// 3 = legendary (yellow)
        /// </summary>
        public int Rarity;
        /// <summary>
        /// Points awarded when this achievement is unlocked, shown as "Skeld Points"
        /// (with the ringed S emblem) in the achievements menu. Defaults from
        /// <see cref="Rarity"/>: 10 (common), 25 (rare), 50 (epic), 100 (legendary), multiplied by
        /// <see cref="PointsMultiplier"/>. The menu shows <see cref="BasePoints"/> with an "xN" badge.
        /// </summary>
        public virtual int Points => BasePoints * PointsMultiplier;
        /// <summary>
        /// Points before any multiplier. Override to customize per achievement.
        /// </summary>
        public virtual int BasePoints => Rarity switch { 1 => 25, 2 => 50, 3 => 100, _ => 10 };
        /// <summary>
        /// Multiplier applied to <see cref="BasePoints"/>; <see cref="Hidden"/> achievements are worth double.
        /// </summary>
        public virtual int PointsMultiplier => Hidden ? 2 : 1;
        /// <summary>
        /// Whether the achievement is hidden or not (hidden achievements get the default icon and have their name and description set to "Hidden Achievement" until unlocked)
        /// </summary>
        public bool Hidden;
        /// <summary>
        /// Id of the shared category this achievement is listed under in the Achievements Menu
        /// (see <see cref="AchievementCategories"/>). Null uses the owning tab's <see cref="AchievementsTab.CategoryId"/>.
        /// Set via object initializer: <c>new MyAchievement(...) { Category = AchievementCategories.Impostor }</c>.
        /// </summary>
        public string? Category;
        /// <summary>
        /// Whether to hide the achievement's rarity (if the achievement is hidden)
        /// </summary>
        public bool HideRarity;
        /// <summary>
        /// Tags for this achievement, used for filtering and searching. Mods can add custom tags
        /// (e.g. "Imposter", "Glitch") to group achievements by role, modifier, or other criteria.
        /// </summary>
        public HashSet<string> Tags = new();
        public Assembly Assembly;
        public string Id;
        /// <summary>
        /// Method to unlock this achievement.
        /// </summary>
        /// <param name="showOnUI">Shows an unlock animation on the hud.</param>
        /// <param name="doStorageUpdate">Indicates whether to update the storage again. Used to make CountAchievements properly update.</param>
        public void Unlock(bool showOnUI = true, bool doStorageUpdate = true)
        {
            if (showOnUI && !Unlocked) AchievementToastQueue.Enqueue(this, true);
            if (Unlocked) return; //Won't unlock an already unlocked achievement
            Unlocked = true;
            if (doStorageUpdate) AchievementStorage.AchievementStorageUpdate(this, true);

        }
        /// <summary>
        /// Resets this achievement back to its locked state and writes through to storage.
        /// </summary>
        /// <param name="save">Whether to save to storage immediately. Pass false when batching resets.</param>
        public virtual void Reset(bool save = true)
        {
            AchievementToastQueue.Dequeue(this);
            Unlocked = false;
            AchievementStorage.AchievementDataReset(this, false, save);
        }
        /// <summary>
        /// Resolves the achievement's <see cref="Name"/> in the current language
        /// (see <see cref="AchievementsStrings.GetText"/>).
        /// </summary>
        public string GetName() => AchievementsStrings.GetText(Name);
        /// <summary>
        /// Resolves the achievement's <see cref="Description"/> in the current language
        /// (see <see cref="AchievementsStrings.GetText"/>).
        /// </summary>
        public string GetDescription() => AchievementsStrings.GetText(Description);
        /// <summary>
        /// Creates a new achievement with the specified properties.
        /// </summary>
        /// <param name="name">The achievement's name.</param>
        /// <param name="description">The achievement's description.</param>
        /// <param name="iconPath">The path to the achievement's icon.</param>
        /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
        /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
        /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
        /// <param name="assembly">The assembly that owns this achievement.</param>
        /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
        public BaseAchievement(StringNames name, StringNames description, string iconPath, int rarity = 0, bool hidden = false, bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        {
            Name = name;
            Description = description;
            IconPath = iconPath;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            _sprite = SpriteTools.LoadSpriteFromPath(IconPath, Assembly, 100);
            Id = BuildId(Assembly, name);
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag.ToLowerInvariant());
                }
            }
        }
        /// <summary>
        /// Creates a new achievement with the specified properties.
        /// </summary>
        /// <param name="name">The achievement's name.</param>
        /// <param name="description">The achievement's description.</param>
        /// <param name="icon">The achievement's icon sprite.</param>
        /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
        /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
        /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
        /// <param name="assembly">The assembly that owns this achievement.</param>
        /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
        public BaseAchievement(StringNames name, StringNames description, Sprite icon, int rarity = 0, bool hidden = false, bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        {
            Name = name;
            Description = description;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            _sprite = icon;
            Id = BuildId(Assembly, name);
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag.ToLowerInvariant());
                }
            }
        }
        /// <summary>
        /// Creates a new achievement with the specified properties.
        /// </summary>
        /// <param name="name">The achievement's name.</param>
        /// <param name="description">The achievement's description.</param>
        /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
        /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
        /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
        /// <param name="assembly">The assembly that owns this achievement.</param>
        /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
        public BaseAchievement(StringNames name, StringNames description, int rarity = 0, bool hidden = false, bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        {
            Name = name;
            Description = description;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            Id = BuildId(Assembly, name);
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag.ToLowerInvariant());
                }
            }
        }
        /// <summary>
        /// Creates a new achievement with the specified properties.
        /// </summary>
        /// <param name="name">The achievement's name (plain text).</param>
        /// <param name="description">The achievement's description (plain text).</param>
        /// <param name="iconPath">The path to the achievement's icon.</param>
        /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
        /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
        /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
        /// <param name="assembly">The assembly that owns this achievement.</param>
        /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
        public BaseAchievement(string name, string description, string iconPath, int rarity = 0, bool hidden = false, bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        {
            Name = CustomStringName.CreateAndRegister(name);
            Description = CustomStringName.CreateAndRegister(description);
            IconPath = iconPath;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            _sprite = SpriteTools.LoadSpriteFromPath(IconPath, Assembly, 100);
            Id = BuildId(Assembly, name);
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag.ToLowerInvariant());
                }
            }
        }
        /// <summary>
        /// Creates a new achievement with the specified properties.
        /// </summary>
        /// <param name="name">The achievement's name (plain text).</param>
        /// <param name="description">The achievement's description (plain text).</param>
        /// <param name="icon">The achievement's icon sprite.</param>
        /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
        /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
        /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
        /// <param name="assembly">The assembly that owns this achievement.</param>
        /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
        public BaseAchievement(string name, string description, Sprite icon, int rarity = 0, bool hidden = false, bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        {
            Name = CustomStringName.CreateAndRegister(name);
            Description = CustomStringName.CreateAndRegister(description);
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            _sprite = icon;
            Id = BuildId(Assembly, name);
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag.ToLowerInvariant());
                }
            }
        }
        /// <summary>
        /// Creates a new achievement with the specified properties.
        /// </summary>
        /// <param name="name">The achievement's name (plain text).</param>
        /// <param name="description">The achievement's description (plain text).</param>
        /// <param name="rarity">The achievement's rarity (0 = common, 1 = rare, 2 = epic, 3 = legendary).</param>
        /// <param name="hidden">Whether the achievement is hidden until unlocked.</param>
        /// <param name="hideRarity">Whether to hide the achievement's rarity.</param>
        /// <param name="assembly">The assembly that owns this achievement.</param>
        /// <param name="tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
        public BaseAchievement(string name, string description, int rarity = 0, bool hidden = false, bool hideRarity = false, Assembly? assembly = null, string[]? tags = null)
        {
            Name = CustomStringName.CreateAndRegister(name);
            Description = CustomStringName.CreateAndRegister(description);
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            Id = BuildId(Assembly, name);
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag.ToLowerInvariant());
                }
            }
        }

        // The Id is the stable identity used for storage lookups, so it must not change
        // with the display language. String constructors keep the raw name (so existing
        // save files keep matching); StringNames constructors use the enum value, so
        // register your StringNames deterministically (all at plugin load).
        private static string BuildId(Assembly assembly, string name) => $"{assembly.GetName().Name}_{name}";
        private static string BuildId(Assembly assembly, StringNames name) => $"{assembly.GetName().Name}_{name.ToString()}";
    }
