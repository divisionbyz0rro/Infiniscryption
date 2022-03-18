using InscryptionAPI.Card;
using DiskCardGame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.FunAndGames.Cards
{
    public class SquirrelFriend : ExtendedAbilityBehaviour
    {
        public static Ability ID { get; private set; }
        public override Ability Ability => ID;

        public override bool ProvidesPassiveAttackBuff => true;
        public override bool ProvidesPassiveHealthBuff => true;

        public override int[] GetPassiveAttackBuffs()
        {
            List<CardSlot> slots = this.Card.OpponentCard ? BoardManager.Instance.opponentSlots : BoardManager.Instance.playerSlots;
            return slots.Select(s => s.Card != null && s.Card.Info.IsOfTribe(Tribe.Squirrel) ? 1 : 0).ToArray();
        }

        public override int[] GetPassiveHealthBuffs()
        {
            List<CardSlot> slots = this.Card.OpponentCard ? BoardManager.Instance.opponentSlots : BoardManager.Instance.playerSlots;
            return slots.Select(s => s.Card != null && s.Card.Info.IsOfTribe(Tribe.Squirrel) ? 2 : 0).ToArray();
        }

        internal static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Squirrel Friend";
            info.rulebookDescription = "[creature] provides +1 attack and +2 health to all friendly squirrels.";
            info.powerLevel = 4;
            info.opponentUsable = false;
            info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Rulebook);

            ID = AbilityManager.Add(GamesPlugin.PluginGuid, info, typeof(SquirrelFriend), AssetHelper.LoadTexture("ability_squirrel_friend")).Id;
        }
    }
}