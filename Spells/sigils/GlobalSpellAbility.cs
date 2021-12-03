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

namespace Infiniscryption.Spells.Sigils
{
    public class GlobalSpellAbility :  VariableStatBehaviour
    {
        // Why is this a stat behavior when these cards have no stats?
        // Simple. I want to cover over the health and attack icons.
        // I want these cards to have 0 health and 0 attack at all times in all zones.
        // This is the best way to do that.

        // I'm following the pattern of HealthForAnts

        private static SpecialStatIcon _icon;
        protected override SpecialStatIcon IconType => _icon;

        public static NewSpecialAbility Instance;
        public static void Register(Harmony harmony)
        {
            if (Instance == null)
            {
                StatIconInfo info = ScriptableObject.CreateInstance<StatIconInfo>();
                info.appliesToAttack = true;
                info.appliesToHealth = true;
                info.rulebookName = "Spell (Global)";
                info.rulebookDescription = "This card has an immediate effect when played and does not resolve on board.";
                info.iconGraphic = AssetHelper.LoadTexture("global_spell_stat_icon");

                SpecialAbilityIdentifier spellId = SpecialAbilityIdentifier.GetID(
                    "zorro.infiniscryption.sigils.globalspell",
                    info.rulebookName
                );

                Instance = new NewSpecialAbility(typeof(GlobalSpellAbility), spellId, info);
                _icon = Instance.statIconInfo.iconType;

                harmony.PatchAll(typeof(GlobalSpellAbility)); // Take care of all the other fancy stuff we have to do
            }
        }

        protected override int[] GetStatValues()
        {
            return new int[] { 0, 0 };
        }

        // First: we don't need room on board
        [HarmonyPatch(typeof(BoardManager), "SacrificesCreateRoomForCard")]
        [HarmonyPrefix]
        public static bool SpellsDoNotNeedSpace(PlayableCard card, ref bool __result)
        {
            if (card != null && card.gameObject.GetComponent<GlobalSpellAbility>() != null)
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
            if (card != null && card.gameObject.GetComponent<GlobalSpellAbility>() == null)
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

                // Now we take care of actually playing the card
                if (PlayerHand.Instance.CardsInHand.Contains(card))
                {
                    if (card.Info.BonesCost > 0)
                        yield return ResourcesManager.Instance.SpendBones(card.Info.BonesCost);
                
                    if (card.EnergyCost > 0)
                        yield return ResourcesManager.Instance.SpendEnergy(card.EnergyCost);
                
                    PlayerHand.Instance.RemoveCardFromHand(card);
                    card.Dead = true;
                    card.Anim.PlayDeathAnimation(false);

                    // Let's allow you to respond to play from hand or to dies
				    if (card.TriggerHandler.RespondsToTrigger(Trigger.PlayFromHand, Array.Empty<object>()))
					    yield return card.TriggerHandler.OnTrigger(Trigger.PlayFromHand, Array.Empty<object>());

                    object[] diedArgs = new object[] { true, null };
                    if (card.TriggerHandler.RespondsToTrigger(Trigger.Die, diedArgs))
					    yield return card.TriggerHandler.OnTrigger(Trigger.Die, diedArgs);

                    // Get rid of the card the hard way
                    yield return new WaitUntil(() => Singleton<GlobalTriggerHandler>.Instance.StackSize == 0);
                    GameObject.Destroy(card.gameObject);

                    if (Singleton<TurnManager>.Instance.IsPlayerTurn)
                    {
                        Traverse boardTraverse = Traverse.Create(BoardManager.Instance);
                        boardTraverse.Field("playerCardsPlayedThisRound").GetValue<List<CardInfo>>().Add(card.Info);
                    }
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

