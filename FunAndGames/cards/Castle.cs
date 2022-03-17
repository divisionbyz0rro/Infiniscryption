using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.FunAndGames.Cards
{
    public class Castle : ActivatedAbilityBehaviour
    {
        public static Ability ID { get; private set; }
        public override Ability Ability => ID;

        private bool CanCastle = true;

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            // This logic lets Leshy use a card with Castle

            if (playerUpkeep)
            {
                CanCastle = true;
                yield break;
            }

            if (!this.Card.OpponentCard)
                yield break;

            if (this.Card.slot == null)
                yield break;

            // Figure out which slot is going to take the most damage
            Dictionary<CardSlot, int> slotDamages = BoardManager.Instance.OpponentSlotsCopy.ToDictionary(s => s, s => 0);

            foreach (PlayableCard playerCard in BoardManager.Instance.PlayerSlotsCopy.Where(s => s.Card != null).Select(s => s.Card))
                foreach (CardSlot opposingSlot in playerCard.GetOpposingSlots())
                    slotDamages[opposingSlot] += playerCard.Attack;

            CardSlot bestSlot = this.Card.Slot;
            int mostDamage = 0;

            foreach (var kvp in slotDamages)
            {
                if (kvp.Value > mostDamage)
                {
                    bestSlot = kvp.Key;
                    mostDamage = kvp.Value;
                }
            }

            if (bestSlot != this.Card.Slot)
                yield return CastleSequence(bestSlot);

            yield break;
        }

        private IEnumerator CastleSequence(CardSlot destination)
        {
            // Code borrowed from StrafeSwap
            PlayableCard swappedCard = destination.Card;

            if (swappedCard != null) // Code borrowed from StrafeSwap
            {
				float midPointX = (swappedCard.Slot.transform.position.x + this.Card.Slot.transform.position.x) / 2f;
				float y = swappedCard.transform.position.y + 0.35f;
				float z = swappedCard.transform.position.z;
				Tween.Position(swappedCard.transform, new Vector3(midPointX, y, z), 0.3f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            }

            CardSlot originalSlot = base.Card.Slot;
			yield return BoardManager.Instance.AssignCardToSlot(this.Card, destination, 0.1f, null, true);
            yield return new WaitForSeconds(0.25f);
			if (swappedCard != null && !swappedCard.Dead)
            {
				yield return BoardManager.Instance.AssignCardToSlot(swappedCard, originalSlot, 0.1f, null, true);
                yield return new WaitForSeconds(0.15f);
            }
        }

        public override IEnumerator Activate()
        {
            if (this.Card == null || this.Card.OpponentCard)
                yield break;

            if (!CanCastle)
                yield break;

            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.25f);

            yield return BoardManager.Instance.ChooseSlot(BoardManager.Instance.playerSlots, true);

            if (BoardManager.Instance.cancelledPlacementWithInput)
                yield break;

            CardSlot destination = BoardManager.Instance.LastSelectedSlot;

            yield return CastleSequence(destination);

            CanCastle = false;   
        }
    }
}