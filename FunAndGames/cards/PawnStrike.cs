using InscryptionAPI.Card;
using DiskCardGame;
using Infiniscryption.FunAndGames;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Infiniscryption.Core.Helpers;
using System.Linq;
using InscryptionAPI.Triggers;

namespace Infiniscryption.FunAndGames.Cards
{
    public class PawnStrike : ExtendedAbilityBehaviour
    {
        public static Ability ID { get; private set; }
        public override Ability Ability => ID;

        public override bool RespondsToGetOpposingSlots() => true;

        public override bool RemoveDefaultAttackSlot() => this.Card.Slot.opposingSlot.Card != null;

        public override List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots)
        {
            // If the slot across from the pawn is empty, do nothing!
            if (this.Card.Slot.opposingSlot.Card == null)
                return new();

            // Otherwise, attack the weaker of the two adjacent slots
            // but only if they DO have a card
            List<CardSlot> adjacentSlots = BoardManager.Instance.GetAdjacentSlots(this.Card.Slot.opposingSlot).Where(s => s.Card != null).ToList();
            
            if (adjacentSlots.Count == 0)
                return new();

            if (adjacentSlots.Count == 1)
                return new() { adjacentSlots[0] };

            if (adjacentSlots[0].Card.Health < adjacentSlots[1].Card.Health)
                return new() { adjacentSlots[0] };

            return new() { adjacentSlots[1] };
        }

        internal static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Pawn Strike";
            info.rulebookDescription = "[creature] attacks the opponent if the slot opposite is empty. Otherwise, if there is one or more diagonally adjacent creatures, [creature] will attack the weakest.";
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_pawn_strike"));
            info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Rulebook);

            ID = AbilityManager.Add(GamesPlugin.PluginGuid, info, typeof(PawnStrike), AssetHelper.LoadTexture("ability_pawn_strike")).Id;
        }
    }
}