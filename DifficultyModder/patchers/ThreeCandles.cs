using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using InscryptionAPI.Helpers;
using InscryptionAPI.Ascension;

namespace Infiniscryption.Curses.Patchers
{
    public static class ThreeCandles
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            var fc = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Extra Candle",
                "You are given an extra life",
                -30,
                TextureHelper.GetImageAsTexture("assist_three_candles.png", typeof(DrawDynamite).Assembly),
                ChallengeManager.HAPPY_ACTIVATED_SPRITE
            );
            
            //fc.SetFlags("P03"); this isn't really compatible anymore with the new lives system i think
            ID = fc.Challenge.challengeType;

            harmony.PatchAll(typeof(ThreeCandles));
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        public static void SetNumberOfLives(ref AscensionSaveData __instance)
        {
            if (AscensionSaveData.Data.ChallengeIsActive(ID))
            {
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.LessLives))
                {
                    __instance.currentRun.playerLives = 2;
                    __instance.currentRun.maxPlayerLives = 2;
                }
                else
                {
                    __instance.currentRun.playerLives = 3;
                    __instance.currentRun.maxPlayerLives = 3;
                }
            }
        }
    }
}