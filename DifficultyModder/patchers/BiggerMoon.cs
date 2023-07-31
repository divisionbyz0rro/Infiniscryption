using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using Infiniscryption.Curses.Cards;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Patchers
{
    public static class BiggerMoon
    {
        public const string MOON = "!GIANTCARD_MOON";
        public const string PIRATESHIP = "!GIANTCARD_SHIP";

        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Full Moon",
                "Giant cards like the moon have more attack and health",
                20,
                TextureHelper.GetImageAsTexture("challenge_bigger_moon.png", typeof(BiggerMoon).Assembly),
                TextureHelper.GetImageAsTexture("activated_challenge_bigger_moon.png", typeof(BiggerMoon).Assembly)
            ).Challenge.challengeType;

            
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(ID))
                {
                    cards.CardByName(MOON).baseAttack = 2;
                    cards.CardByName(MOON).baseHealth = 80;

                    cards.CardByName(PIRATESHIP).baseAttack = 3;
                    cards.CardByName(PIRATESHIP).baseHealth = 120;
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