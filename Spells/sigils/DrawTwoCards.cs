using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Spells.Sigils
{
	// Token: 0x02000324 RID: 804
	public class DrawTwoCards : AbilityBehaviour
	{
		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06001358 RID: 4952 RVA: 0x000438A9 File Offset: 0x00041AA9
		public override Ability Ability => _ability;
        private static Ability _ability;
        
        public static AbilityIdentifier Identifier { get; private set; }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Draw Twice",
                "Draw the top card of your main deck and side deck when this card dies."
            );
            info.canStack = true;

            Identifier = AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.drawtwocards", "Draw Twice");

            NewAbility ability = new NewAbility(
                info,
                typeof(DrawTwoCards),
                AssetHelper.LoadTexture("ability_drawtwocardsondeath"),
                Identifier
            );

            DrawTwoCards._ability = ability.ability;
        }

		// Token: 0x0600135B RID: 4955 RVA: 0x0000F57E File Offset: 0x0000D77E
		public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
		{
			return true;
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x000438AD File Offset: 0x00041AAD
		public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
		{
			yield return base.PreSuccessfulTriggerSequence();
			ViewManager.Instance.SwitchToView(View.Default);

            // Now we draw the top card from each deck
            if (CardDrawPiles.Instance is CardDrawPiles3D)
            {
                CardDrawPiles3D cardPiles = CardDrawPiles.Instance as CardDrawPiles3D;
                yield return cardPiles.DrawCardFromDeck();
                yield return cardPiles.DrawFromSidePile();
            } 
            else 
            {
                yield return CardDrawPiles.Instance.DrawCardFromDeck();
                yield return CardDrawPiles.Instance.DrawCardFromDeck();
            }

			yield return base.LearnAbility(0.5f);
			yield break;
		}
	}
}
