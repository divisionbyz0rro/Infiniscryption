using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using Infiniscryption.FunAndGames;
using UnityEngine;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.FunAndGames.Cards
{
    public class SquirrelPower : VariableStatBehaviour
    {
        public static SpecialStatIcon ID { get; private set; }
        public override SpecialStatIcon IconType => ID;

        public override int[] GetStatValues()
        {
            List<CardSlot> slots = this.PlayableCard.OpponentCard ? BoardManager.Instance.OpponentSlotsCopy : BoardManager.Instance.PlayerSlotsCopy;
            int power = slots.Where(s => s.Card != null && s.Card.Info.IsOfTribe(Tribe.Squirrel)).Count();
            return new int [] { power, 0 };
        }

        public static void Register() 
        {
            StatIconInfo info = StatIconManager.New(GamesPlugin.PluginGuid, "Squirrel Power", "Squirrel Power", typeof(SquirrelPower));
            ID = info.iconType;
            info.appliesToAttack = true;
            info.appliesToHealth = false;
            info.SetIcon(AssetHelper.LoadTexture("staticon"));
        }
    }
}