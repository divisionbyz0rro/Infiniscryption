using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Ascension;
using InscryptionAPI.Saves;
using System;

namespace Infiniscryption.Curses.Patchers
{
    public static class GoldenPeltStart
    {
        public static AscensionChallenge ID {get; private set;}

        public static CardTemple ScreenState 
        { 
            get
            {
                string value = ModdedSaveManager.SaveData.GetValue("zorro.inscryption.infiniscryption.p03kayceerun", "ScreenState");
                if (string.IsNullOrEmpty(value))
                    return CardTemple.Nature;

                return (CardTemple)Enum.Parse(typeof(CardTemple), value);
            }
        }

        public static bool IsP03Run
        {
            get 
            {
                if (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part3"))
                    return true;

                if (ScreenState == CardTemple.Tech)
                    return true;

                if (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part1"))
                    return false;

                if (AscensionSaveData.Data != null && AscensionSaveData.Data.currentRun != null && AscensionSaveData.Data.currentRun.playerLives > 0)
                    return ModdedSaveManager.SaveData.GetValueAsBoolean("zorro.inscryption.infiniscryption.p03kayceerun", "IsP03Run");

                return false;
            }
        }

        public static void Register(Harmony harmony)
        {
            var fc = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Golden Beginnings",
                "Start the game with the ability to draft a rare card",
                -10,
                AssetHelper.LoadTexture("assist_golden_pelt"),
                ChallengeManager.HAPPY_ACTIVATED_SPRITE,
                stackable: true
            );

            fc.SetFlags("P03");
            ID = fc.Challenge.challengeType;

            harmony.PatchAll(typeof(GoldenPeltStart));
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "zorro.inscryption.infiniscryption.kayceestarters" })]
        private static void AddGoldenPeltToStart(ref AscensionSaveData __instance)
        {
            string rareDraftCard = IsP03Run ? "P03KCM_Draft_Token_Rare" : "PeltGolden";

            for (int i = 0; i < AscensionSaveData.Data.GetNumChallengesOfTypeActive(ID); i++)
                __instance.currentRun.playerDeck.AddCard(CardLoader.GetCardByName(rareDraftCard));
        }
            

        [HarmonyPatch(typeof(Part3SaveData), nameof(Part3SaveData.Initialize))]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "zorro.inscryption.infiniscryption.p03kayceerun" })]
        private static void AddTokenToStart(ref Part3SaveData __instance)
        {
            string rareDraftCard = IsP03Run ? "P03KCM_Draft_Token_Rare" : "PeltGolden";

            if (SaveFile.IsAscension)
            {

                for (int i = 0; i < AscensionSaveData.Data.GetNumChallengesOfTypeActive(ID); i++)
                    __instance.deck.AddCard(CardLoader.GetCardByName(rareDraftCard));
            }
            
        }
    }
}