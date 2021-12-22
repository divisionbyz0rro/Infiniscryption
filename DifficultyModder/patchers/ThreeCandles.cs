using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Challenges;

namespace Infiniscryption.Curses.Patchers
{
    public static class ThreeCandles
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                InfiniscryptionCursePlugin.PluginGuid,
                "Extra Candle",
                "You are given an extra life",
                -20,
                AssetHelper.LoadTexture("assist_three_candles")
            );

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