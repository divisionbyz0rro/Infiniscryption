using BepInEx;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using InscryptionAPI.Card;

namespace Infiniscryption.VanillaStackable.Patchers
{
    public static class SampleCards
    {
        internal static void RegisterCustomCards(Harmony harmony)
        {
            CardInfo card = ScriptableObject.CreateInstance<CardInfo>();
            card.name = "Super_Sharp_Porcupine";
            card.displayedName = "Smelly Pokeypine";
            card.baseAttack = 0;
            card.baseHealth = 10;
            card.metaCategories = new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer };
            card.cardComplexity = CardComplexity.Advanced;
            card.temple = CardTemple.Nature;
            card.description = "Super Sharp!";
            card.hideAttackAndHealth = true;
            card.SetPortrait(Resources.Load<Texture2D>("art/cards/portraits/portrait_porcupine"));
            card.abilities = new() { Ability.Sharp, Ability.Sharp, Ability.DebuffEnemy, Ability.DebuffEnemy };
            CardManager.Add(card);

            harmony.PatchAll(typeof(SampleCards));
        }

        [HarmonyPatch(typeof(RunState), "InitializeStarterDeckAndItems")]
        [HarmonyPostfix]
        public static void AddPokeypine()
        {
            for (int i = 0; i < 8; i++)
                RunState.Run.playerDeck.AddCard(CardLoader.GetCardByName("Super_Sharp_Porcupine"));
        }
    }
}