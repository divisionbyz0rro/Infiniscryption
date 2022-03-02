using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Infiniscryption.Curses.Cards;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Patchers
{
    public static class BiggerMoon
    {
        public const string MOON = "!GIANTCARD_MOON";

        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Bigger Moon",
                "The moon is bigger",
                20,
                AssetHelper.LoadTexture("challenge_bigger_moon"),
                AssetHelper.LoadTexture("activated_challenge_bigger_moon")
            ).challengeType;

            
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(ID))
                {
                    cards.CardByName(MOON).baseAttack = 2;
                    cards.CardByName(MOON).baseHealth = 80;
                }

                return cards;
            };

            harmony.PatchAll(typeof(BiggerMoon));
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.CreateCardInSlot))]
        [HarmonyPostfix]
        private static IEnumerator ShowChallengeWhenMakingMoon(IEnumerator sequence, CardInfo info)
        {
            if (info.name.Equals(MOON) && SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ID))
                ChallengeActivationUI.TryShowActivation(ID);

            
            yield return sequence;
        }
    }
}