using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using System.Linq;
using InscryptionAPI.Card;
using Infiniscryption.Curses.Sequences;

namespace Infiniscryption.Curses.Cards
{
    public class Dynamite : AbilityBehaviour
    {
        public const string EXPLOSION_SOUND = "card_explosion";

		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        // This has all of the logic to implement the Dynamite card that is added to the Prospector boss battle
        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            DynamiteAppearance.Register();

            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Booby Trap";
            info.rulebookDescription = "If this is in your hand at the end of your turn, it explodes. If it is on the board at the end of your opponent's turn, it explodes. Either way, it explodes.";
            info.canStack = true;
            info.powerLevel = -2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };

            Dynamite.AbilityID = AbilityManager.Add(
                CursePlugin.PluginGuid,
                info,
                typeof(Dynamite),
                AssetHelper.LoadTexture("ability_dynamite")
            ).Id;

            CardManager.New(CursePlugin.CardPrefix, ProspectorBossHardOpponent.DYNAMITE, "Dynamite", 0, 2)
                .AddTraits(Trait.Terrain)
                .SetPortrait(AssetHelper.LoadTexture("dynamite_portrait"), AssetHelper.LoadTexture("dynamite_emission"))
                .AddAbilities(Dynamite.AbilityID)
                .AddAppearances(DynamiteAppearance.ID)
                .temple = CardTemple.Nature;

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Dynamite).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(DynamiteAppearance).TypeHandle);

            // Patch this class
            harmony.PatchAll(typeof(Dynamite));
        }

        // We're going to cheat the upkeep triggers a bit.
        // When on board, it only responds to the opponent's upkeep (it explodes)
        // In hand, it responds to your upkeep.
        // The 'RespondsToUpkeep' is used by the game under normal circumstances, so it only says 'yes' on opponent's upkeep

        [HarmonyPatch(typeof(TurnManager), "PlayerTurn")]
        [HarmonyPostfix]
        public static IEnumerator ExplodeAtEndOfTurn(IEnumerator sequenceEvent)
        {
            yield return sequenceEvent;

            // Check for dynamite cards in hand and do it if necessary
            if (PlayerHand.Instance != null)
            {
                List<PlayableCard> cardsToExplode = PlayerHand.Instance.CardsInHand
                                                    .Where(c => c.Info.Abilities.Any(a => a == Dynamite.AbilityID))
                                                    .ToList();

                object[] upkeepVars = new object[] { true };

                foreach (PlayableCard card in cardsToExplode)
                    yield return card.TriggerHandler.OnTrigger(Trigger.TurnEnd, upkeepVars);
            }
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd)
        {
            return playerTurnEnd;
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            if (playerTurnEnd)
            {
                // Only do this if the card is in the player's hand
                if (this.Card.InHand)
                {                    
                    // Only explode if the player hasn't already won
                    if (LifeManager.Instance.DamageUntilPlayerWin <= 0)
                        yield break;

                    // Focus on the card
                    ViewManager.Instance.SwitchToView(View.Hand);
                    PlayerHand.Instance.OnCardInspected(this.Card);
                    PlayerHand.Instance.InspectingLocked = true;

                    // Shake the card
                    this.Card.Anim.SetShaking(true);
                    yield return new WaitForSeconds(0.75f);

                    // Kill the card
                    this.Card.Anim.PlaySacrificeParticles();
                    yield return new WaitForSeconds(0.2f);

                    // Make the explosion sound
                    AudioController.Instance.PlaySound3D(EXPLOSION_SOUND, MixerGroup.CardVoiceSFX, this.Card.transform.position, 1f, 0f, null, null, null, null, false);
                    this.Card.Anim.PlayTransformAnimation();
                    yield return new WaitForSeconds(0.44f);

                    this.Card.Anim.StopAllCoroutines();
                    PlayerHand.Instance.RemoveCardFromHand(this.Card);
                    GameObject.Destroy(this.Card.gameObject);

                    yield return new WaitForSeconds(0.8f);

                    PlayerHand.Instance.InspectingLocked = false;
                    
                    // Show the damage
                    yield return LifeManager.Instance.ShowDamageSequence(2, 2, true, 0f, null, 0f);
                    yield return new WaitForSeconds(0.5f);

                    ViewManager.Instance.SwitchToView(View.Hand);
                } 
                else 
                {
                    // Focus on the card
                    ViewManager.Instance.SwitchToView(View.BoardCentered);

                    // Shake the card
                    this.Card.Anim.SetShaking(true);
                    yield return new WaitForSeconds(0.6f);

                    // Make the explosion sound
                    AudioController.Instance.PlaySound3D(EXPLOSION_SOUND, MixerGroup.CardVoiceSFX, this.Card.transform.position, 1f, 0f, null, null, new AudioParams.Randomization(true), null, false);
                    this.Card.Anim.PlayTransformAnimation();
                    yield return new WaitForSeconds(0.4f);

                    // Kill the adjacent cards
                    if (this.Card.Slot != null)
                    {
                        List<CardSlot> slots = BoardManager.Instance.PlayerSlotsCopy
                                               .AddItem(this.Card.Slot.opposingSlot)
                                               .ToList();

                        foreach (CardSlot slot in slots.Where(s => s != null && s.Card != null))
                            yield return slot.Card.TakeDamage(2, this.Card);
                    }
                    
                    // Show the damage
                    yield return this.Card.Die(true, null, true);

                    GameObject.Destroy(this.Card.gameObject, 0.5f);
                }
            }

            yield break;
        }
    }
}