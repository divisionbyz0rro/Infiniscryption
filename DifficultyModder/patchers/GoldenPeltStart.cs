using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Challenges;

namespace Infiniscryption.Curses.Patchers
{
    public static class GoldenPeltStart
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                InfiniscryptionCursePlugin.PluginGuid,
                "Golden Beginnings",
                "Start the game with a golden pelt in your deck",
                -10,
                AssetHelper.LoadTexture("assist_golden_pelt"),
                stackable: true
            );

            harmony.PatchAll(typeof(GoldenPeltStart));
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "zorro.inscryption.infiniscryption.kayceestarters" })]
        public static void AddGoldenPeltToStart(ref AscensionSaveData __instance)
        {
            for (int i = 0; i < AscensionSaveData.Data.GetNumChallengesOfTypeActive(ID); i++)
                __instance.currentRun.playerDeck.AddCard(CardLoader.GetCardByName("PeltGolden"));
            
        }
    }
}