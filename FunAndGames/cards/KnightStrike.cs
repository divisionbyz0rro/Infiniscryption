using InscryptionAPI.Card;
using DiskCardGame;
using Infiniscryption.FunAndGames;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.FunAndGames.Cards
{
    public class KnightStrike : ExtendedAbilityBehaviour
    {
        public static Ability ID { get; private set; }
        public override Ability Ability => ID;

        public override bool RespondsToGetOpposingSlots() => true;

        public override List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots)
        {
            List<CardSlot> opposingSlots = this.Card.OpponentCard ? BoardManager.Instance.PlayerSlotsCopy : BoardManager.Instance.OpponentSlotsCopy;

            if (this.Card.Slot.Index + 2 >= opposingSlots.Count)
                return new List<CardSlot>() { opposingSlots[this.Card.Slot.Index - 2] };
            else
                return new List<CardSlot>() { opposingSlots[this.Card.Slot.Index + 2] };
        }

        public override bool RemoveDefaultAttackSlot() => true;

        internal static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Knight Strike";
            info.rulebookDescription = "[creature] attacks the card slot two slots away from it";
            info.powerLevel = 0;
            info.opponentUsable = true;
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_knight_strike"));
            info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Rulebook);

            ID = AbilityManager.Add(GamesPlugin.PluginGuid, info, typeof(KnightStrike), AssetHelper.LoadTexture("ability_knight_strike")).Id;
        }
    }
}