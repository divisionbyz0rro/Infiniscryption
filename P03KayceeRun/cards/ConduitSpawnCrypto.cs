using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
	public class ConduitSpawnCrypto : ConduitSpawn
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Mine Cryptocurrency";
            info.rulebookDescription = "When part of a conduit, [creature] will generate cryptocurrency.";
            info.canStack = true;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ConduitSpawnCrypto.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitSpawnCrypto),
                AssetHelper.LoadTexture("ability_minecrypto")
            ).Id;
        }

		public override string GetSpawnCardId()
		{
			return CustomCards.GOLLYCOIN;
		}
	}
}
