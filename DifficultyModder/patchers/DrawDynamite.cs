using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Infiniscryption.Curses.Cards;
using Infiniscryption.Curses.Sequences;
using InscryptionAPI.Ascension;

namespace Infiniscryption.Curses.Patchers
{
    public static class DrawDynamite
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Exploding Cards",
                "Dynamite is added to your deck after every boss battle",
                5,
                AssetHelper.LoadTexture("challenge_dynamite"),
                AssetHelper.LoadTexture("activated_challenge_dynamite")
            ).Challenge.challengeType;

            harmony.PatchAll(typeof(DrawDynamite));
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPrefix]
        public static void AddDynamiteToDeck()
        {
            if (AscensionSaveData.Data.ChallengeIsActive(ID) && TurnManager.Instance.opponent is Part1BossOpponent && TurnManager.Instance.PlayerIsWinner())
            {
                AscensionSaveData.Data.currentRun.playerDeck.AddCard(CardLoader.GetCardByName(ProspectorBossHardOpponent.DYNAMITE));
            }
        }

        [HarmonyPatch(typeof(CardRemoveSequencer), nameof(CardRemoveSequencer.GetValidCards))]
        [HarmonyPostfix]
        private static void DontAllowSacrificeDynamite(ref List<CardInfo> __result)
        {
            __result.RemoveAll(ci => ci.HasAbility(Dynamite.AbilityID));
        }
    }
}