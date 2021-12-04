using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using Infiniscryption.Spells;

namespace Infiniscryption.Spells.Sigils
{
	// Token: 0x02000324 RID: 804
	public class DestroyAllCardsOnDeath : AbilityBehaviour
	{
		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06001358 RID: 4952 RVA: 0x000438A9 File Offset: 0x00041AA9
		public override Ability Ability => _ability;
        private static Ability _ability;
        
        public static AbilityIdentifier Identifier { get; private set; }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Cataclysm",
                "Destroys every other creature on board when this card dies."
            );

            Identifier = AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.cataclysm", "Cataclysm");

            NewAbility ability = new NewAbility(
                info,
                typeof(DestroyAllCardsOnDeath),
                AssetHelper.LoadTexture("ability_nuke"),
                Identifier
            );

            DestroyAllCardsOnDeath._ability = ability.ability;
        }

		// Token: 0x0600135B RID: 4955 RVA: 0x0000F57E File Offset: 0x0000D77E
		public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
		{
			return true;
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x000438AD File Offset: 0x00041AAD
		public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
		{
            InfiniscryptionSpellsPlugin.Log.LogInfo($"On cataclysm death");
			yield return base.PreSuccessfulTriggerSequence();
			ViewManager.Instance.SwitchToView(View.Board);

            // Kill EVERYTHING
            foreach (var slot in BoardManager.Instance.OpponentSlotsCopy)
            {
                if (slot.Card != null)
                {
                    InfiniscryptionSpellsPlugin.Log.LogInfo($"Killing {slot.Card.name}");
                    yield return slot.Card.Die(true, null, true);
                }
            }

            foreach (var slot in BoardManager.Instance.PlayerSlotsCopy)
            {
                if (slot.Card != null)
                {
                    InfiniscryptionSpellsPlugin.Log.LogInfo($"Killing {slot.Card.name}");
                    yield return slot.Card.Die(true, null, true);
                }
            }

			yield return base.LearnAbility(0.5f);

            ViewManager.Instance.SwitchToView(View.Default);

			yield break;
		}
	}
}
