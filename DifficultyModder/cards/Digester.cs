using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Infiniscryption.Curses.Sequences;

namespace Infiniscryption.Curses.Cards
{
    public class Digester : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility ID { get; private set; }

        public const string GULP_SOUND = "gulp";

        public override int Priority => int.MinValue; // We have to fire last

        private CardInfo StomachContents
        {
            // We store the stomach contents in the card's ice cube parameters
            // This is just a helper to make it easier to get
            get
            {
                if (this.PlayableCard.Info.iceCubeParams == null || this.PlayableCard.HasAbility(Ability.IceCube))
                    return null;

                return this.PlayableCard.Info.iceCubeParams.creatureWithin;
            }
            set
            {
                if (this.PlayableCard.HasAbility(Ability.IceCube))
                    return;

                if (value == null)
                {
                    this.PlayableCard.Info.iceCubeParams = null;
                }
                else
                {
                    this.PlayableCard.Info.iceCubeParams = new IceCubeParams() { creatureWithin = value };
                }
            }
        }

        // This has all of the logic to implement the Shark card that is added to the Angler
        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            MegaSharkAppearance.Register();
            ID = SpecialTriggeredAbilityManager.Add(CursePlugin.PluginGuid, "Swallow Whole", typeof(Digester)).Id;

            CardManager.New(CursePlugin.CardPrefix, AnglerBossHardOpponent.MEGA_SHARK, "Mega Shark", 3, 5)
                .AddTraits(Trait.Uncuttable)
                .SetPortrait(MegaSharkAppearance.SHARK_OPEN_PORTRAIT, MegaSharkAppearance.SHARK_OPEN_EMISSION)
                .AddAbilities(Ability.Reach, Ability.WhackAMole)
                .AddAppearances(MegaSharkAppearance.ID)
                .AddSpecialAbilities(Digester.ID);

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Digester).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Bitten).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(BittenCardAppearance).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(MegaSharkAppearance).TypeHandle);

            // Add another emission
            TextureHelper.RegisterEmissionForSprite(MegaSharkAppearance.SHARK_CLOSED_PORTRAIT_SPRITE, MegaSharkAppearance.SHARK_CLOSED_EMISSION_SPRITE);

            // Patch this class
            harmony.PatchAll(typeof(Digester));
        }

        // This patch makes the card have the right background
        [HarmonyPatch(typeof(Card), "ApplyAppearanceBehaviours")]
        [HarmonyPostfix]
        public static void SpellBackground(ref Card __instance)
        {
            if (__instance.Info.name == "Angler_Shark")
            {
                __instance.gameObject.AddComponent<MegaSharkAppearance>().ApplyAppearance();
            }
        }

        private void UpdateAppearances()
        {
            foreach (CardAppearanceBehaviour behavior in this.PlayableCard.GetComponents<CardAppearanceBehaviour>())
                behavior.ApplyAppearance();
        }

        private IEnumerator TransitionAwayFromDigestingState()
        {
            // This happens when the card switches to a digesting state
            // For right now, let's just add temporary mods to make the attack go down to 0.
            // In the future, we'll look at the evolve parameters and evolve into something else
            ViewManager.Instance.SwitchToView(View.Board);

            List<CardModificationInfo> modsToRemove = this.PlayableCard.TemporaryMods.Where(mod => mod.attackAdjustment == -1000).ToList();
            foreach(CardModificationInfo mod in modsToRemove)
                this.PlayableCard.TemporaryMods.Remove(mod);

            UpdateAppearances();

            this.PlayableCard.Anim.SetShaking(true);

            yield return TextDisplayer.Instance.PlayDialogueEvent("HungryAgain", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            this.PlayableCard.Anim.SetShaking(false);
        }

        private IEnumerator EatCard(PlayableCard target, bool attackerWasMeal)
        {
            // We need to eat this thing...
            // Start by pausing all animations on the target.
            if (attackerWasMeal)
            {
                this.PlayableCard.Anim.PlayTransformAnimation();
                yield return new WaitForSeconds(0.2f);
            }
            AudioController.Instance.PlaySound3D(GULP_SOUND, MixerGroup.CardVoiceSFX, this.Card.transform.position, 1f, 0f, null, null, null, null, false);
            yield return new WaitForSeconds(0.1f);

            // Time to add the card to our stomach!
            this.StomachContents = target.Info;

            // And kill the target
            target.Anim.StopAllCoroutines();
            target.UnassignFromSlot();
            GameObject.Destroy(target.gameObject, 0.1f); // We destroy it instead of killing it. This prevents any of its triggers from firing.

            this.PlayableCard.TemporaryMods.Add(new CardModificationInfo(-1000, 0)); // Force the card's attack to 0
            UpdateAppearances();
            this.PlayableCard.Anim.SetShaking(true);

            yield return TextDisplayer.Instance.PlayDialogueEvent("CardIsSleep", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            this.PlayableCard.Anim.SetShaking(false);

            yield break;
        }

        private void AddModToCard(CardInfo card, CardModificationInfo mod)
        {
            if (RunState.Run.playerDeck.Cards.Contains(card))
                RunState.Run.playerDeck.ModifyCard(card, mod);
            else
                card.Mods.Add(mod);
        }

        private IEnumerator Digest()
        {
            if (this.StomachContents == null)
                yield break; // Don't do anything if we have no stomach contents

            ViewManager.Instance.SwitchToView(View.Board);

            // First, play the transformation effect
            this.PlayableCard.Anim.PlayTransformAnimation();
            yield return new WaitForSeconds(0.3f);

            // Let'se see if this damages or kills us
            if (this.StomachContents.traits.Any(tr => tr == Trait.KillsSurvivors))
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("CardAtePoison", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

                // This kills us!
                yield return ExpelCard();
                yield return this.PlayableCard.Die(true, playSound:true);
                yield break;
            }

            // Sharp cards do damage to us
            int internalDamage = this.StomachContents.Abilities.Where(
                ab => ab == Ability.Sharp
            ).Count();
            if (internalDamage > 0)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("CardAteSharp", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

                // This causes damage!
                bool quitAfterTakeDamage = false;
                if (internalDamage > this.PlayableCard.Health)
                {
                    yield return ExpelCard();
                    quitAfterTakeDamage = true;
                }

                yield return this.PlayableCard.TakeDamage(internalDamage, this.PlayableCard);
                
                if (quitAfterTakeDamage)
                {
                    yield break;
                }
            }

            // If the card does not have the bite mark, add the bite mark.
            // Otherwise, add -1/-1 to the card
            // But we only add the bite mark to cards in the main deck
            bool isMainDeckCard = RunState.Run.playerDeck.Cards.Any(c => c.name == this.StomachContents.name);
            if (!this.StomachContents.HasAbility(Bitten.AbilityID) && isMainDeckCard)
            {
                CardModificationInfo info = new CardModificationInfo(Bitten.AbilityID);
                AddModToCard(this.StomachContents, info);
            } else {
                AddModToCard(this.StomachContents, new CardModificationInfo(-1, -1));
            }

            if (this.StomachContents.Health > 0)
            {
                if (internalDamage == 0)
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("DigestingCard", 
                        TextDisplayer.MessageAdvanceMode.Input, 
                        TextDisplayer.EventIntersectMode.Wait, 
                        new string[] { this.StomachContents.DisplayedNameLocalized, this.StomachContents.Health.ToString() }, 
                        null
                    );
                }
            }
            else
            {
                // The card is dead...
                if (this.PlayableCard.OpponentCard)
                {
                    // It could be a side deck card, in which case, who cares.
                    if (SaveManager.SaveFile.CurrentDeck.Cards.Contains(this.StomachContents))
                    {
                        // Remove it from the player's deck. Oh so sorry.
                        SaveManager.SaveFile.CurrentDeck.RemoveCard(this.StomachContents);

                        yield return TextDisplayer.Instance.PlayDialogueEvent("DigestedCardDeadForever", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] { this.StomachContents.DisplayedNameLocalized }, null);
                    }
                }

                this.StomachContents = null;
                yield return TransitionAwayFromDigestingState();
            }
        }

        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            // We only do something on upkeep if we have ice cube parameters.
            // This whole thing is a huge freaking hack; we're storing the digested card as an ice cube
            // And just hoping the card doesn't somehow also gain the ice cube ability
            if (this.PlayableCard.Info.iceCubeParams == null || this.PlayableCard.Info.iceCubeParams.creatureWithin == null)
                return false;

            return (this.PlayableCard.OpponentCard && !playerUpkeep) || (!this.PlayableCard.OpponentCard && playerUpkeep);
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return Digest();
            CursePlugin.Log.LogInfo("Upkeep complete");
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            return true;
        }

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            // We fired last, so we can assume this is the final state (i.e., WhackAMole has already happened)
            
            // There must be a defending card and attacking card
            if (slot.Card == null || attacker == null)
                yield break;

            PlayableCard defender = slot.Card;

            // One of the attacker or defender must be us
            if (attacker != this.PlayableCard && slot.Card != this.PlayableCard)
                yield break;

            // We must have room in our bellies
            if (this.StomachContents != null)
                yield break;

            // Figure out who's getting eaten
            PlayableCard meal = attacker == this.PlayableCard ? defender : attacker;

            // Okay, if the attacker has flying and the defender doesn't have flying or reach, nothing happens
            if (attacker.HasAbility(Ability.Flying) && !(defender.HasAbility(Ability.Reach) || defender.HasAbility(Ability.Flying)))
                yield break;

            // However, this ability does not care about submerge
            // So we'll leave it at that.

            // Okay, so we really want to interrupt the attacking animation, not prevent it
            // But there aren't really any hooks for that.
            // So instead we pretend, and we'll play the slot attack animation ourselves

            // We know that this card is being attacked.
            // So let's play the attack animation but stop it:
            bool hitHappened = false;
            attacker.Anim.PlayAttackAnimation(false, slot, delegate() {
                attacker.Anim.SetAnimationPaused(true);
                hitHappened = true;
            });
            yield return new WaitUntil(() => hitHappened);

            // Now we can play everything we wanted.
            yield return EatCard(meal, meal == attacker);

            if (attacker != null && attacker.OnBoard)
                attacker.Anim.SetAnimationPaused(false);
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
        {
            return this.StomachContents != null;
        }

        private IEnumerator ExpelCard()
        {
            // Double check that the stomach contents are still there
            CardInfo digestedCard = this.StomachContents;

            if (digestedCard == null)
                yield break;

            // When this card dies, we need to vomit up the creature that died.
            // How that happens depends upon the type of card this is
            if (this.PlayableCard.OpponentCard)
            {
                // For an opponent card, we put it in the player's hand (with damage intact)
                ViewManager.Instance.SwitchToView(View.Hand);

                PlayableCard spawnedCard = CardSpawner.SpawnPlayableCard(digestedCard);
                yield return PlayerHand.Instance.AddCardToHand(spawnedCard, Vector3.zero, 0f);
                PlayerHand.Instance.OnCardInspected(spawnedCard);
                PlayerHand.Instance.InspectingLocked = true;

                spawnedCard.Anim.PlayHitAnimation();
                yield return new WaitForSeconds(0.6f);

                PlayerHand.Instance.InspectingLocked = false;
                ViewManager.Instance.SwitchToView(View.Default);
            }
            else
            {
                // We spit it back onto the board 
                // TODO: Implement this
            }

            this.StomachContents = null;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return ExpelCard();
        }

        public override void OnCleanUp()
        {
            base.OnCleanUp();

            // This fires when the card is cleaned up / removed from the board
            this.StomachContents = null;
        }
    }
}
