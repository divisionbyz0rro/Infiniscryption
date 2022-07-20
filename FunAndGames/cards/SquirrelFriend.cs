using InscryptionAPI.Card;
using DiskCardGame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Triggers;

namespace Infiniscryption.FunAndGames.Cards
{
    public class SquirrelFriend : ExtendedAbilityBehaviour
    {
        public static Ability ID { get; private set; }
        public override Ability Ability => ID;

        public override int GetPassiveAttackBuff(PlayableCard target)
        {
            return target.OpponentCard == this.Card.OpponentCard && target.Info.IsOfTribe(Tribe.Squirrel) ? 1 : 0;
        }

        public override int GetPassiveHealthBuff(PlayableCard target)
        {
            return GetPassiveAttackBuff(target) * 2;
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