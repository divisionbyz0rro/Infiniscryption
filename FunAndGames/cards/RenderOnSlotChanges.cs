using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.FunAndGames.Cards
{
    public class RenderOnSlotChanges : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility ID { get; private set; }

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            return true;
        }

        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            this.Card.RenderCard();
            yield break;
        }

        public override bool RespondsToResolveOnBoard()
        {
            return true;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            this.Card.RenderCard();
            yield break;
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            return true;
        }

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            this.Card.RenderCard();
            yield break;
        }

        internal static void Register()
        {
            ID = SpecialTriggeredAbilityManager.Add(GamesPlugin.PluginGuid, "RenderOnSlotChanges", typeof(RenderOnSlotChanges)).Id;
        }
    }
}