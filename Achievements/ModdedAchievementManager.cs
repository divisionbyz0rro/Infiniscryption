using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BepInEx.Bootstrap;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Regions;
using InscryptionAPI.Saves;
using InscryptionAPI.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infiniscryption.Achievements
{
    /// <summary>
    /// Management class for achievements
    /// </summary>
    [HarmonyPatch]
    public static class ModdedAchievementManager
    {
        private static readonly AudioClip DefaultAudioClip = SoundManager.LoadAudioClip("achievement_default.wav");

        private static readonly Rect ICON_RECT = new (0, 0, 22, 22);
        private static readonly Vector2 ICON_PIVOT = new (0.5f, 0.5f);

        /// <summary>
        /// Enumerates the group of achievements
        /// </summary>
        public enum AchievementGroup
        {
            StoryAchievements = 0,
            KayceesModAchievements = 1
        }

        /// <summary>
        /// Defines a group of achievements
        /// </summary>
        public class AchievementGroupDefinition
        {
            /// <summary>
            /// Unique identifier for the achievement group
            /// </summary>
            public AchievementGroup ID { get; internal set; }

            /// <summary>
            /// Name of the achievement group
            /// </summary>
            public string EnglishGroupName { get; internal set; }

            /// <summary>
            /// The audio pop that plays when the achievement happens
            /// </summary>
            public string AudioCue { get; set; }

            /// <summary>
            /// Indicates if this is a group of default achievements
            /// </summary>
            public bool IsDefault { get; internal set; }

            /// <summary>
            /// The sprite displayed when any achievement in this group is locked/hidden
            /// </summary>
            public Sprite LockedSprite { get; internal set; }

            /// <summary>
            /// All achievements in this group
            /// </summary>
            public List<AchievementDefinition> Achievements => new(AllAchievements.Where(ad => ad.GroupID == this.ID));
        }

        // Defines an achievement
        public class AchievementDefinition
        {
            /// <summary>
            /// Unique identifier for this achievement
            /// </summary>
            public Achievement ID { get; internal set; }

            /// <summary>
            /// The group this achievement belongs to
            /// </summary>
            public AchievementGroup GroupID { get; internal set; }

            /// <summary>
            /// Name of the achievement
            /// </summary>
            public string EnglishName { get; internal set; }

            /// <summary>
            /// Description of the achievement
            /// </summary>
            /// <value></value>
            public string EnglishDescription { get; internal set; }

            /// <summary>
            /// The icon for the achievement
            /// </summary>
            public Sprite IconSprite { get; internal set; }

            /// <summary>
            /// Indicates if this is a secret achievement (if so, the description is hidden until it is unlocked)
            /// </summary>
            public bool Secret { get; internal set; }

            /// <summary>
            /// Indicates if this achievement is unlocked or not
            /// </summary>
            /// <value></value>
            public bool IsUnlocked => SaveManager.SaveFile.unlockedAchievements != null && SaveManager.SaveFile.unlockedAchievements.Contains(this.ID);
        }

        // Not using the base//new pattern because we are not support the event pattern
        // I don't believe mods should modify other mods achievements
        private static List<AchievementDefinition> AllAchievements = new();
        private static List<AchievementGroupDefinition> AllAchievementGroups = new();

        // These are all of the audio clips we need to patch in
        private static List<AudioClip> clipsToPatchIn = new () { DefaultAudioClip };

        static ModdedAchievementManager()
        {
            AllAchievementGroups.Clear();
            NewGroup(AchievementGroup.KayceesModAchievements, "Kaycee's Mod Achievements", DefaultAudioClip.name, TextureHelper.GetImageAsTexture("KMOD_LOCKED.png", typeof(ModdedAchievementManager).Assembly), true);
            NewGroup(AchievementGroup.StoryAchievements, "Story Achievements", null, null, true);

            AllAchievements.Clear();
            foreach (Achievement ach in Enum.GetValues(typeof(Achievement)))
            {
                var defaultDefn = AchievementsList.m_Achievements.FirstOrDefault(at => at.m_eAchievementID == ach);
                if (defaultDefn == null)
                {
                    AchievementsPlugin.Log.LogWarning($"Could not find a definition for achievement {ach}");
                    continue;
                }
                AchievementGroup grp = ach >= Achievement.KMOD_CHALLENGELEVEL1 ? AchievementGroup.KayceesModAchievements : AchievementGroup.StoryAchievements;
                Texture2D icon = grp == AchievementGroup.KayceesModAchievements ? TextureHelper.GetImageAsTexture($"{ach}.png", typeof(ModdedAchievementManager).Assembly) : null;
                New(
                    ach,
                    defaultDefn.m_strName,
                    defaultDefn.m_strDescription,
                    true,
                    grp,
                    icon
                );
            }
        }

        internal static AchievementGroupDefinition NewGroup(AchievementGroup id, string name, string audioCue, Texture2D lockedTexture, bool isDefault)
        {
            if (AllAchievementGroups.Any(agd => agd.ID == id))
                throw new InvalidOperationException($"An achievement group with ID {id} already exists!");

            if (lockedTexture != null && (lockedTexture.width != 22 || lockedTexture.height != 22))
                throw new InvalidOperationException("Locked icon texture must be 22x22 pixels");

            Sprite locked = lockedTexture == null ? GroupById(AchievementGroup.KayceesModAchievements).LockedSprite : Sprite.Create(lockedTexture, ICON_RECT, ICON_PIVOT);

            AchievementGroupDefinition newGroup = new () {
                ID = id,
                EnglishGroupName = name,
                AudioCue = audioCue,
                LockedSprite = locked
            };

            AllAchievementGroups.Add(newGroup);
            return newGroup;
        }

        /// <summary>
        /// Creates a new achievement group, throwing an error if it already exists
        /// </summary>
        /// <param name="modGuid">The guid of the mod creating this group</param>
        /// <param name="englishGroupName">The english name of the group</param>
        /// <param name="audioCue">The audio cue that will play when achievements are unlocked (or null if no cue)</param>
        /// <param name="lockedTexture">The icon to display when secret achievements in this group are not unlocked</param>
        public static AchievementGroupDefinition NewGroup(string modGuid, string englishGroupName, string audioCue, Texture2D lockedTexture)
        {
            return NewGroup(
                GuidManager.GetEnumValue<AchievementGroup>(modGuid, englishGroupName),
                englishGroupName,
                audioCue,
                lockedTexture,
                false
            );
        }

        /// <summary>
        /// Creates a new achievement group, throwing an error if it already exists
        /// </summary>
        /// <param name="modGuid">The guid of the mod creating this group</param>
        /// <param name="englishGroupName">The english name of the group</param>
        /// <param name="audioCue">The audio cue that will play when achievements are unlocked (or null if no cue)</param>
        /// <param name="lockedTexture">The icon to display when secret achievements in this group are not unlocked</param>
        public static AchievementGroupDefinition NewGroup(string modGuid, string englishGroupName, AudioClip audioCue, Texture2D lockedTexture)
        {
            if (!clipsToPatchIn.Any(ac => ac.name.Equals(audioCue.name)))
                clipsToPatchIn.Add(audioCue);

            return NewGroup(
                GuidManager.GetEnumValue<AchievementGroup>(modGuid, englishGroupName),
                englishGroupName,
                audioCue.name,
                lockedTexture,
                false
            );
        }

        /// <summary>
        /// Creates a new achievement group, throwing an error if it already exists
        /// </summary>
        /// <param name="modGuid">The guid of the mod creating this group</param>
        /// <param name="englishGroupName">The english name of the group</param>
        /// <param name="lockedTexture">The icon to display when secret achievements in this group are not unlocked</param>
        public static AchievementGroupDefinition NewGroup(string modGuid, string englishGroupName, Texture2D lockedTexture)
        {
            return NewGroup(
                GuidManager.GetEnumValue<AchievementGroup>(modGuid, englishGroupName),
                englishGroupName,
                DefaultAudioClip.name,
                lockedTexture,
                false
            );
        }

        /// <summary>
        /// Creates a new achievement group, throwing an error if it already exists
        /// </summary>
        /// <param name="modGuid">The guid of the mod creating this group</param>
        /// <param name="englishGroupName">The english name of the group</param>
        public static AchievementGroupDefinition NewGroup(string modGuid, string englishGroupName)
        {
            return NewGroup(
                GuidManager.GetEnumValue<AchievementGroup>(modGuid, englishGroupName),
                englishGroupName,
                DefaultAudioClip.name,
                null,
                false
            );
        }

        internal static AchievementDefinition New(Achievement id, string achievementEnglishName, string achievementEnglishDescription, bool secret, AchievementGroup groupId, Texture2D icon)
        {
            if (AllAchievements.Any(agd => agd.ID == id))
                throw new InvalidOperationException($"An achievement group with ID {id} already exists!");

            if (icon != null && (icon.width != 22 || icon.height != 22))
                throw new InvalidOperationException("Achievement icon texture must be 22x22 pixels");

            AchievementDefinition def = new() {
                ID = id,
                GroupID = groupId,
                EnglishName = achievementEnglishName,
                EnglishDescription = achievementEnglishDescription,
                IconSprite = Sprite.Create(icon, ICON_RECT, ICON_PIVOT),
                Secret = secret
            };

            AllAchievements.Add(def);
            return def;
        }

        /// <summary>
        /// Creates a new achievement, throwing an error if it exists already
        /// </summary>
        /// <param name="modGuid">The mod creating the achievement</param>
        /// <param name="achievementEnglishName">The english name of the achievement</param>
        /// <param name="achievementEnglishDescription">The english description of the achievement</param>
        /// <param name="secret">Indicates if the achievement is kept secret until unlocked</param>
        /// <param name="groupId">The group the achievement belongs to</param>
        /// <param name="icon">The icon for the achievement</param>
        public static AchievementDefinition New(string modGuid, string achievementEnglishName, string achievementEnglishDescription, bool secret, AchievementGroup groupId, Texture2D icon)
        {
            return New(
                GuidManager.GetEnumValue<Achievement>(modGuid, achievementEnglishName),
                achievementEnglishName,
                achievementEnglishDescription,
                secret,
                groupId,
                icon
            );
        }

        /// <summary>
        /// Gets a previously defined achievement
        /// </summary>
        public static AchievementDefinition AchievementById(Achievement id) => AllAchievements.FirstOrDefault(ad => ad.ID == id);

        /// <summary>
        /// Gets a previously defined achievement group
        /// </summary>
        public static AchievementGroupDefinition GroupById(AchievementGroup id) => AllAchievementGroups.FirstOrDefault(agd => agd.ID == id);

        /// <summary>
        /// Gets a previously defined achievement group belonging to a specific achievement
        /// </summary>
        public static AchievementGroupDefinition GroupByAchievementId(Achievement id)
        {
            AchievementDefinition def = AchievementById(id);
            if (def == null)
                return null;

            return GroupById(def.GroupID);
        }

        [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.Unlock))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static bool Unlock(Achievement achievementID)
        {
            bool isDefaultAchievement = GroupByAchievementId(achievementID).IsDefault;

            // This is a patch to make sure we don't accidentally invoke the platform's achievement handler
            // for a custom achievement.
            if (SaveManager.SaveFile.unlockedAchievements != null 
                && !SaveManager.SaveFile.unlockedAchievements.Contains(achievementID))
            {
                SaveManager.SaveFile.unlockedAchievements.Add(achievementID);
                if (AchievementPopupHandler.Instance != null)
                    AchievementPopupHandler.Instance.TryShowUnlockAchievement(achievementID);
            }

            // Only invoke the platform handler if the achievement is default
            if (isDefaultAchievement && AchievementManager.platformHandler != null)
            {
                AchievementManager.platformHandler.Unlock(achievementID);
            }
            return false;
        }

        [HarmonyPatch(typeof(AudioController), nameof(AudioController.Awake))]
        [HarmonyPostfix]
        internal static void LoadMyCustomAudio(ref AudioController __instance)
        {
            foreach (var clip in clipsToPatchIn)
                if (clip != null && !__instance.SFX.Any(ac => ac.name.Equals(clip.name, StringComparison.InvariantCultureIgnoreCase)))
                    __instance.SFX.Add(clip);
        }
    }
}