using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using Infiniscryption.Curses.Helpers;
using Infiniscryption.Core.Helpers;
using APIPlugin;
using System.Linq;

namespace Infiniscryption.Curses.Cards
{
    public class Dynamite : AbilityBehaviour
    {
        public const string EXPLOSION_SOUND = "card_explosion";

        private static Ability _ability;
        public override Ability Ability => _ability;

        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.dynamite", "Booby Trap");
            }
        }

        // This has all of the logic to implement the Dynamite card that is added to the Prospector boss battle
        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Booby Trap",
                "If this is in your hand at the beginning of your turn, it explodes. If it is on the board at the beginning of your opponent's turn, it explodes. Either way, it explodes."
            );

            NewAbility ability = new NewAbility(
                info,
                typeof(Dynamite),
                AssetHelper.LoadTexture("ability_dynamite"),
                Identifier
            );

            Dynamite._ability = ability.ability;

            NewCard.Add(
                "Prospector_Dynamite",
                "Dynamite",
                0, 2,
                new List<CardMetaCategory>(),
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Ouch!",
                traits: new List<Trait>() { Trait.Terrain },
                defaultTex: AssetHelper.LoadTexture("dynamite_portrait"),
                emissionTex: AssetHelper.LoadTexture("dynamite_emission"),
                abilityIdsParam: new List<AbilityIdentifier>() { Dynamite.Identifier }
            );

            // Patch this class
            harmony.PatchAll(typeof(Dynamite));
        }

        // This patch makes the card have the right background
        [HarmonyPatch(typeof(Card), "ApplyAppearanceBehaviours")]
        [HarmonyPostfix]
        public static void SpellBackground(ref Card __instance)
        {
            if (__instance.Info.Abilities.Contains(Dynamite._ability))
            {
                __instance.gameObject.AddComponent<DynamiteAppearance>().ApplyAppearance();
            }
        }

        // We're going to cheat the upkeep triggers a bit.
        // When on board, it only responds to the opponent's upkeep (it explodes)
        // In hand, it responds to your upkeep.
        // The 'RespondsToUpkeep' is used by the game under normal circumstances, so it only says 'yes' on opponent's upkeep

        [HarmonyPatch(typeof(TurnManager), "DoUpkeepPhase")]
        [HarmonyPostfix]
        public static IEnumerator ExplodeOnUpkeep(IEnumerator sequenceEvent, bool playerUpkeep)
        {
            while (sequenceEvent.MoveNext())
                yield return sequenceEvent.Current;

            // Check for dynamite cards in hand and do it if necessary
            if (PlayerHand.Instance != null && playerUpkeep)
            {
                List<PlayableCard> cardsToExplode = PlayerHand.Instance.CardsInHand
                                                    .Where(c => c.Info.Abilities.Any(a => (int)a == (int)Dynamite._ability))
                                                    .ToList();

                object[] upkeepVars = new object[] { playerUpkeep };

                foreach (PlayableCard card in cardsToExplode)
                    yield return card.TriggerHandler.OnTrigger(Trigger.Upkeep, upkeepVars);
            }
        }

        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return (playerUpkeep && this.Card.InHand) || (!playerUpkeep && this.Card.OnBoard);
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (playerUpkeep)
            {
                // Only do this if the card is in the player's hand
                if (this.Card.InHand)
                {                    
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
            }
            else
            {
                // Only do this if the card is on board still
                if (this.Card.OnBoard)
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