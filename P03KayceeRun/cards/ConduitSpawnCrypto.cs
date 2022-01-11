using System;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;

namespace Infiniscryption.P03KayceeRun.Cards
{
	public class ConduitSpawnCrypto : ConduitSpawn
	{
        private static Ability _ability;
		public override Ability Ability
		{
			get
			{
				return _ability;
			}
		}

        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.conduitspawncrypto", "Crypto Miner");
            }
        }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Mine Cryptocurrency",
                "When part of a conduit, [creature] will generate cryptocurrency."
            );
            info.canStack = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            NewAbility ability = new NewAbility(
                info,
                typeof(ConduitSpawnCrypto),
                AssetHelper.LoadTexture("ability_minecrypto"),
                Identifier
            );

            ConduitSpawnCrypto._ability = ability.ability;
        }

		// Token: 0x0600142B RID: 5163 RVA: 0x000546BC File Offset: 0x000528BC
		protected override string GetSpawnCardId()
		{
			return CustomCards.GOLLYCOIN;
		}
	}
}
