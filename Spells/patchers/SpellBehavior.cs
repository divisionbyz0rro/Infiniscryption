using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using APIPlugin;
using System.Linq;
using Infiniscryption.Spells.Sigils;

namespace Infiniscryption.Spells.Patchers
{
    public static class SpellBehavior
    {
        public class SpellBackgroundAppearance : CardAppearanceBehaviour
        {
            private static Texture _emptySpell = AssetHelper.LoadTexture("card_empty_spell");
            public override void ApplyAppearance()
            {
                base.Card.RenderInfo.baseTextureOverride = _emptySpell;
            }
        }

        public class RareSpellBackgroundAppearance : CardAppearanceBehaviour
        {
            private static Texture _emptySpell = AssetHelper.LoadTexture("card_empty_spell_rare");
            public override void ApplyAppearance()
            {
                base.Card.RenderInfo.baseTextureOverride = _emptySpell;
            }
        }

        public static bool IsGlobalSpell(this CardInfo card)
        {
            return card.SpecialAbilities.Any(ab => (int)ab == (int)GlobalSpellAbility.Instance.id.id);
        }

        public static bool IsTargetedSpell(this CardInfo card)
        {
            return card.SpecialAbilities.Any(ab => (int)ab == (int)TargetedSpellAbility.Instance.id.id);
        }

        public static bool IsSpell(this CardInfo card)
        {
            return card.SpecialAbilities.Any(ab => (int)ab == (int)GlobalSpellAbility.Instance.id.id ||
                                                   (int)ab == (int)TargetedSpellAbility.Instance.id.id);
        }

        // This patch makes the card back have nostats despite having a staticon
        [HarmonyPatch(typeof(Card), "ApplyAppearanceBehaviours")]
        [HarmonyPostfix]
        public static void SpellBackground(ref Card __instance)
        {
            if (__instance.Info.IsSpell())
            {
                if (__instance.Info.metaCategories.Any(cat => cat == CardMetaCategory.Rare))
                    __instance.gameObject.AddComponent<RareSpellBackgroundAppearance>().ApplyAppearance();
                else
                    __instance.gameObject.AddComponent<SpellBackgroundAppearance>().ApplyAppearance();
            }
        }

        // This patch takes care of making sure that the staticon appears
        private static void PatchGlobals()
        {
            Traverse trav = Traverse.Create<ScriptableObjectLoader<CardInfo>>();
            List<CardInfo> allCards = trav.Field("allData").GetValue<List<CardInfo>>();
            foreach (CardInfo card in allCards)
            {
                if (card.IsGlobalSpell())
                {
                    // This has the global spell ability.
                    // Let's check its icon info
                    Traverse cardTrav = Traverse.Create(card);
                    cardTrav.Field("specialStatIcon").SetValue(GlobalSpellAbility._icon);
                } else if (card.IsTargetedSpell())
                {
                    // This has the global spell ability.
                    // Let's check its icon info
                    Traverse cardTrav = Traverse.Create(card);
                    cardTrav.Field("specialStatIcon").SetValue(TargetedSpellAbility._icon);
                }
            }
        }

        [HarmonyPatch(typeof(LoadingScreenManager), "LoadGameData")]
        [HarmonyPostfix]
        public static void AttachStatIconsToGlobalSpells()
        {
            PatchGlobals();
        }

        [HarmonyPatch(typeof(ChapterSelectMenu), "OnChapterConfirmed")]
        [HarmonyPostfix]
        public static void AttachStatIconsToGlobalSpellsOnChapter()
        {
            PatchGlobals();
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
            // For public stuff, 'this' becomes PlayerHand.Instance
            // For private stuff, use a Traverse
            Traverse playerHandTraverse = Traverse.Create(PlayerHand.Instance);

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

			playerHandTraverse.Field("choosingSlotCard").SetValue(card);

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
			if (!BoardManager.Instance.CancelledSacrifice)
			{
                cardWasPlayed = true;
                card.Anim.SetSelectedToPlay(false);

                if (card.Info.IsTargetedSpell())
                {
                    // This card needs a slot
                    // Let's go find a slot
                    // We'll use the existing choose slot function
                    List<CardSlot> allSlots = BoardManager.Instance.OpponentSlotsCopy.Concat(BoardManager.Instance.PlayerSlotsCopy).ToList();
                    
                    // I'm not allowing the user to cancel.
                    // I can't. The sacrifice method already sacrificed the cards. They're already gone.
                    // I can't back out anymore.
                    // So...you better hope you really wanted to do this.
                    IEnumerator chooseSlotEnumerator = BoardManager.Instance.ChooseSlot(allSlots, false);
                    chooseSlotEnumerator.MoveNext();
                    
                    // Before we yield the wait, we set everything selectable
                    foreach (CardSlot slot in allSlots)
                    {
                        slot.SetEnabled(true);
                        slot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);
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
                        if (card.Info.IsTargetedSpell())    
                            card.Slot = BoardManager.Instance.LastSelectedSlot;

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

                    // If this is targeted, fire the targets
                    if (card.Info.IsTargetedSpell())
                    {
                        object[] targetArgs = new object[] { BoardManager.Instance.LastSelectedSlot, card };
                        if (card.TriggerHandler.RespondsToTrigger(Trigger.SlotTargetedForAttack, targetArgs))
                            yield return card.TriggerHandler.OnTrigger(Trigger.SlotTargetedForAttack, targetArgs);
                    }

                    card.Dead = true;
                    card.Anim.PlayDeathAnimation(false);

                    object[] diedArgs = new object[] { true, null };
                    if (card.TriggerHandler.RespondsToTrigger(Trigger.Die, diedArgs))
					    yield return card.TriggerHandler.OnTrigger(Trigger.Die, diedArgs);

                    // Get rid of the card the hard way
                    yield return new WaitUntil(() => Singleton<GlobalTriggerHandler>.Instance.StackSize == 0);

                    if (Singleton<TurnManager>.Instance.IsPlayerTurn)
                    {
                        Traverse boardTraverse = Traverse.Create(BoardManager.Instance);
                        boardTraverse.Field("playerCardsPlayedThisRound").GetValue<List<CardInfo>>().Add(card.Info);
                    }

                    InteractionCursor.Instance.ClearForcedCursorType();
                    yield return new WaitForSeconds(0.6f);
                    GameObject.Destroy(card.gameObject, 0.5f);
                    ViewManager.Instance.SwitchToView(View.Default);
                }
			}
			if (!cardWasPlayed)
				BoardManager.Instance.ShowCardNearBoard(card, false);
			
            playerHandTraverse.Field("choosingSlotCard").SetValue(card);

			if (card != null && card.Anim != null)
				card.Anim.SetSelectedToPlay(false);

			PlayerHand.Instance.CardsInHand.ForEach(delegate(PlayableCard x)
			{
				x.SetEnabled(true);
			});

			yield break;
        }
    }
}