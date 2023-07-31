using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using Infiniscryption.Curses.Cards;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;
using Infiniscryption.Achievements;
using UnityEngine;

namespace Infiniscryption.Curses.Patchers
{
    [HarmonyPatch]
    internal static class CursedAchievements
    {
        internal static Achievement HIGH_LEVEL { get; private set; }
        internal static Achievement LOW_LEVEL { get; private set; }
        internal static Achievement DOUBLE_CHAOS { get; private set; }
        internal static Achievement TRIPLE_HAUNT { get; private set; }
        internal static Achievement SUPER_CELLO { get; private set; }
        internal static Achievement SHARK_POP { get; private set; }
        internal static Achievement HOT_POTATO { get; private set; }

        internal static void Register()
        {
            var groupId = ModdedAchievementManager.NewGroup(CursePlugin.PluginGuid, "Cursed Achievements", TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(CursedAchievements).Assembly)).ID;

            LOW_LEVEL = ModdedAchievementManager.New(
                CursePlugin.PluginGuid, 
                "Here For A Good Time", 
                "Complete a run with a negative challenge level", 
                false, 
                groupId,
                TextureHelper.GetImageAsTexture("achievement_251.png", typeof(CursedAchievements).Assembly)
            ).ID;

            HIGH_LEVEL = ModdedAchievementManager.New(
                CursePlugin.PluginGuid, 
                "Here For A Hard Time", 
                "Complete a run with a challenge level above 250", 
                false, 
                groupId,
                TextureHelper.GetImageAsTexture("achievement_251.png", typeof(CursedAchievements).Assembly)
            ).ID;

            DOUBLE_CHAOS = ModdedAchievementManager.New(
                CursePlugin.PluginGuid, 
                "Utter Insanity", 
                "Complete a run with two Chaotic Enemy skulls active", 
                false, 
                groupId,
                TextureHelper.GetImageAsTexture("achievement_double_chaos.png", typeof(CursedAchievements).Assembly)
            ).ID;

            TRIPLE_HAUNT = ModdedAchievementManager.New(
                CursePlugin.PluginGuid, 
                "Too Spooky", 
                "Reach the maximum deathcard haunt level", 
                false, 
                groupId,
                TextureHelper.GetImageAsTexture("achievement_triple_haunt.png", typeof(CursedAchievements).Assembly)
            ).ID;

            SUPER_CELLO = ModdedAchievementManager.New(
                CursePlugin.PluginGuid, 
                "That's No Moon", 
                "Defeat the Limoncello with the Bigger Moon skull active", 
                false, 
                groupId,
                TextureHelper.GetImageAsTexture("achievement_pirate.png", typeof(CursedAchievements).Assembly)
            ).ID;

            SHARK_POP = ModdedAchievementManager.New(
                CursePlugin.PluginGuid, 
                "'Tis But A Flesh Wound", 
                "Retrieve a damaged card from the belly of a Mega Shark", 
                false, 
                groupId,
                TextureHelper.GetImageAsTexture("achievement_sharkpop.png", typeof(CursedAchievements).Assembly)
            ).ID;

            HOT_POTATO = ModdedAchievementManager.New(
                CursePlugin.PluginGuid, 
                "Munitions Expert", 
                "Safely dispose of a stick of dynamite", 
                false, 
                groupId,
                TextureHelper.GetImageAsTexture("achievement_safety.png", typeof(CursedAchievements).Assembly)
            ).ID;

            CursePlugin.Log.LogDebug($"Cursed Achievements Have Been Loaded. Group number is {groupId}");
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TryUnlockAchievements))]
        [HarmonyPostfix]
        private static void AscensionCompleteAchievements()
        {
            if (AscensionSaveData.Data.GetNumChallengesOfTypeActive(RandomSigils.ID) >= 2)
                AchievementManager.Unlock(DOUBLE_CHAOS);

            if (AscensionSaveData.Data.GetActiveChallengePoints() > 250)
                AchievementManager.Unlock(HIGH_LEVEL);

            if (AscensionSaveData.Data.GetActiveChallengePoints() < 0)
                AchievementManager.Unlock(LOW_LEVEL);

            if (AscensionSaveData.Data.ChallengeIsActive(BiggerMoon.ID) && AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.FinalBoss))
                AchievementManager.Unlock(SUPER_CELLO);
        }
    }
}
