using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using System.Linq;
using Infiniscryption.Spells.Sigils;
using InscryptionAPI.Card;

namespace Infiniscryption.Spells.Patchers
{
    public static class SpellBehavior
    {
        public class SpellBackgroundAppearance : CardAppearanceBehaviour
        {
            public static CardAppearanceBehaviour.Appearance ID = CardAppearanceBehaviourManager.Add(InfiniscryptionSpellsPlugin.OriginalPluginGuid, "SpellBackground", typeof(SpellBackgroundAppearance)).Id;
            private static Texture _emptySpell = AssetHelper.LoadTexture("card_empty_spell");
            public override void ApplyAppearance()
            {
                base.Card.RenderInfo.baseTextureOverride = _emptySpell;
            }
        }

        public class RareSpellBackgroundAppearance : CardAppearanceBehaviour
        {
            public static CardAppearanceBehaviour.Appearance ID = CardAppearanceBehaviourManager.Add(InfiniscryptionSpellsPlugin.OriginalPluginGuid, "RareSpellBackground", typeof(RareSpellBackgroundAppearance)).Id;
            private static Texture _emptySpell = AssetHelper.LoadTexture("card_empty_spell_rare");
            public override void ApplyAppearance()
            {
                base.Card.RenderInfo.baseTextureOverride = _emptySpell;
            }
        }

        public static bool IsGlobalSpell(this CardInfo card)
        {
            return card.SpecialAbilities.Any(ab => ab == GlobalSpellAbility.ID);
        }

        public static bool IsTargetedSpell(this CardInfo card)
        {
            return card.SpecialAbilities.Any(ab => ab == TargetedSpellAbility.ID);
        }

        public static List<CardSlot> GetAffectedSlots(this CardSlot slot, PlayableCard card)
        {
            if (card.HasAbility(Ability.AllStrike))
            {
                if (slot.IsPlayerSlot)
                    return BoardManager.Instance.PlayerSlotsCopy;
                else
                    return BoardManager.Instance.OpponentSlotsCopy;
            }

            List<CardSlot> retval = new List<CardSlot>();

            if (card.HasAbility(Ability.SplitStrike) || card.HasAbility(Ability.TriStrike))
            {
                CardSlot leftSlot = BoardManager.Instance.GetAdjacent(slot, true);
                CardSlot rightSlot = BoardManager.Instance.GetAdjacent(slot, true);

                if (leftSlot != null)
                    retval.Add(leftSlot);

                if (rightSlot != null)
                    retval.Add(rightSlot);

                if (card.HasAbility(Ability.TriStrike))
                    retval.Add(slot);
            }
            else
            {
                retval.Add(slot);
            }

            return retval;
        }

        public static bool IsValidTarget(this CardSlot slot, PlayableCard card, bool singleSlotOverride=false)
        {
            if (singleSlotOverride)
            {
                if (slot.IsPlayerSlot && card.TriggerHandler.RespondsToTrigger(Trigger.ResolveOnBoard, Array.Empty<object>()))
                    return true;

                object[] attackTrigger = new object[] { slot, card };
                if (card.TriggerHandler.RespondsToTrigger(Trigger.SlotTargetedForAttack, attackTrigger))
                    return true;

                return false;
            }
            else
            {
                // We need to test all possible slots
                foreach (CardSlot subSlot in slot.GetAffectedSlots(card))
                    if (subSlot.IsValidTarget(card, true))
                        return true;

                return false;
            }
        }

        public static bool HasValidTarget(this PlayableCard card)
        {
            List<CardSlot> allSlots = BoardManager.Instance.OpponentSlotsCopy.Concat(BoardManager.Instance.PlayerSlotsCopy).ToList();
            object[] attackTrigger = new object[] { null, card };
            foreach (CardSlot slot in allSlots)
            {
                if (slot.IsValidTarget(card))
                    return true; // There is at least one slot that responds to this trigger, so leave the result as-is
            }

            // If we got this far without finding a slot that the card responds to, then...no good
            return false;
        }

        public static bool IsSpell(this CardInfo card)
        {
            return card.SpecialAbilities.Any(ab => ab == GlobalSpellAbility.ID || ab == TargetedSpellAbility.ID);
        }

        // First: we don't need room on board
        [HarmonyPatch(typeof(BoardManager), "SacrificesCreateRoomForCard")]
        [HarmonyPrefix]
        public static bool SpellsDoNotNeedSpace(PlayableCard card, ref bool __result)
        {
            if (card != null && card.Info.IsSpell())
            {
                __result = true;
                return false;
            }
            return true;
        }

        // Next, there has to be at least one slot (for targeted spells)
        // that the spell targets
        [HarmonyPatch(typeof(PlayableCard), "CanPlay")]
        [HarmonyPostfix]
        public static void TargetSpellsMustHaveValidTarget(ref bool __result, ref PlayableCard __instance)
        {
            if (!__result)
                return; // If the result is already false, no need to do any more work

            if (__instance.Info.IsTargetedSpell() && !__instance.HasValidTarget())
            {
                __result = false;
            }
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void SpellHints()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("NoValidTargets", new string []
            {
                "there are no valid targets for that spell"
            });
        }

        public static HintsHandler.Hint TargetSpellsNeedTargetHint = new HintsHandler.Hint("NoValidTargets", 2);

        [HarmonyPatch(typeof(HintsHandler), "OnNonplayableCardClicked")]
        [HarmonyPrefix]
        public static bool TargetSpellsNeedATarget(PlayableCard card)
        {
            if (card.Info.IsTargetedSpell() && !card.HasValidTarget())
            {
                TargetSpellsNeedTargetHint.TryPlayDialogue(null);
                return false;
            }
            return true;
        }

        // Next: we don't resolve normally
        // It's way easier to copy-paste this and only keep the stuff we need
        [HarmonyPatch(typeof(PlayerHand), "SelectSlotForCard")]
        [HarmonyPostfix]
        public static IEnumerator SpellsResolveDifferently(IEnumerator sequenceResult, PlayableCard card)
        {
            // If this isn't a global spell ability, behave like normal
            if (card != null && !card.Info.IsSpell())
            {
                while (sequenceResult.MoveNext())
                    yield return sequenceResult.Current;

                yield break;
            }

            // The rest of this comes from the original code in PlayerHand.SelectSlotForCard
            PlayerHand.Instance.CardsInHand.ForEach(delegate(PlayableCard x)
			{
				x.SetEnabled(false);
			});

            // If we're currently choosing a slot for something else, wait on that to finish first
			yield return new WaitWhile(() => PlayerHand.Instance.ChoosingSlot);

            // This is a bit of a hack to deal with a protected virtual method:
            // Just do the same thing under the same conditions
            if (PlayerHand.Instance is PlayerHand3D)
                (card.Anim as CardAnimationController3D).SetRendererYPos(0f);

			if (RuleBookController.Instance != null)
				RuleBookController.Instance.SetShown(false, true);

			BoardManager.Instance.CancelledSacrifice = false;

			PlayerHand.Instance.choosingSlotCard = card;

			if (card != null && card.Anim != null)
				card.Anim.SetSelectedToPlay(true);
			
			BoardManager.Instance.ShowCardNearBoard(card, true);
			if (TurnManager.Instance.SpecialSequencer != null)
				yield return TurnManager.Instance.SpecialSequencer.CardSelectedFromHand(card);
			
			bool cardWasPlayed = false;
			bool requiresSacrifices = card.Info.BloodCost > 0;
			if (requiresSacrifices)
			{
				List<CardSlot> validSlots = BoardManager.Instance.PlayerSlotsCopy.FindAll((CardSlot x) => x.Card != null);
				yield return BoardManager.Instance.ChooseSacrificesForCard(validSlots, card);
			}

            // All card slots
            List<CardSlot> allSlots = BoardManager.Instance.OpponentSlotsCopy.Concat(BoardManager.Instance.PlayerSlotsCopy).ToList();

			if (!BoardManager.Instance.CancelledSacrifice)
			{
                cardWasPlayed = true;
                card.Anim.SetSelectedToPlay(false);

                if (card.Info.IsTargetedSpell())
                {                   
                    // I'm not allowing the user to cancel.
                    // I can't. The sacrifice method already sacrificed the cards. They're already gone.
                    // I can't back out anymore.
                    // So...you better hope you really wanted to do this.
                    IEnumerator chooseSlotEnumerator = BoardManager.Instance.ChooseSlot(allSlots, false);
                    chooseSlotEnumerator.MoveNext();
                    
                    // Before we yield the wait, we set everything selectable
                    // If the card responds to targeting that slot!
                    foreach (CardSlot slot in allSlots)
                    {
                        if (slot.IsValidTarget(card))
                        {
                            slot.SetEnabled(true);
                            slot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);
                            slot.Chooseable = true;
                        } 
                        else
                        {
                            slot.SetEnabled(false);
				            slot.ShowState(HighlightedInteractable.State.NonInteractable, false, 0.15f);
                            slot.Chooseable = false;
                        }
                    }

                    yield return chooseSlotEnumerator.Current;

                    // Now we go through the rest
                    while (chooseSlotEnumerator.MoveNext())
                        yield return chooseSlotEnumerator.Current;

                    // Now we know what the target slot was!
                }

                // Now we take care of actually playing the card
                if (PlayerHand.Instance.CardsInHand.Contains(card))
                {
                    if (card.Info.BonesCost > 0)
                        yield return ResourcesManager.Instance.SpendBones(card.Info.BonesCost);
                
                    if (card.EnergyCost > 0)
                        yield return ResourcesManager.Instance.SpendEnergy(card.EnergyCost);
                
                    PlayerHand.Instance.RemoveCardFromHand(card);

                    // Let's allow you to respond to play from hand or to dies
				    if (card.TriggerHandler.RespondsToTrigger(Trigger.PlayFromHand, Array.Empty<object>()))
					    yield return card.TriggerHandler.OnTrigger(Trigger.PlayFromHand, Array.Empty<object>());

                    // Let's temporarily pretend to resolve this on the board
                    if (card.TriggerHandler.RespondsToTrigger(Trigger.ResolveOnBoard, Array.Empty<object>()))
                    {
                        List<CardSlot> resolveSlots;
                        if (card.Info.IsTargetedSpell())    
                            resolveSlots = BoardManager.Instance.LastSelectedSlot.GetAffectedSlots(card);
                        else
                            resolveSlots = new List<CardSlot>() { null }; // For global spells, just resolve once, globally
                        
                        foreach (CardSlot slot in resolveSlots)
                        {
                            card.Slot = slot;

                            IEnumerator resolveTrigger = card.TriggerHandler.OnTrigger(Trigger.ResolveOnBoard, Array.Empty<object>());
                            for (bool active = true; active; )
                            {
                                // Catch exceptions only on executing/resuming the iterator function
                                try
                                {
                                    active = resolveTrigger.MoveNext();
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log("IteratorFunction() threw exception: " + ex);
                                }
                            
                                // Yielding and other loop logic is moved outside of the try-catch
                                if (active)
                                    yield return resolveTrigger.Current;
                            }

                            card.Slot = null;
                        }
                    }

                    // If this is targeted, fire the targets
                    if (card.Info.IsTargetedSpell())
                    {
                        foreach (CardSlot targetSlot in BoardManager.Instance.LastSelectedSlot.GetAffectedSlots(card))
                        {
                            object[] targetArgs = new object[] { targetSlot, card };
                            yield return card.TriggerHandler.OnTrigger(Trigger.SlotTargetedForAttack, targetArgs);
                        }
                    }

                    card.Dead = true;
                    card.Anim.PlayDeathAnimation(false);

                    object[] diedArgs = new object[] { true, null };
                    if (card.TriggerHandler.RespondsToTrigger(Trigger.Die, diedArgs))
					    yield return card.TriggerHandler.OnTrigger(Trigger.Die, diedArgs);

                    yield return new WaitUntil(() => GlobalTriggerHandler.Instance.StackSize == 0);

                    if (TurnManager.Instance.IsPlayerTurn)
                        BoardManager.Instance.playerCardsPlayedThisRound.Add(card.Info);

                    InteractionCursor.Instance.ClearForcedCursorType();
                    yield return new WaitForSeconds(0.6f);
                    GameObject.Destroy(card.gameObject, 0.5f);
                    ViewManager.Instance.SwitchToView(View.Default);
                }
			}
			if (!cardWasPlayed)
				BoardManager.Instance.ShowCardNearBoard(card, false);
			
            PlayerHand.Instance.choosingSlotCard = null;

			if (card != null && card.Anim != null)
				card.Anim.SetSelectedToPlay(false);

			PlayerHand.Instance.CardsInHand.ForEach(delegate(PlayableCard x)
			{
				x.SetEnabled(true);
			});

            // Enable every slot
            foreach (CardSlot slot in allSlots)
            {
                slot.SetEnabled(true);
                slot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);
                slot.Chooseable = false;
            }

			yield break;
        }
    }
}