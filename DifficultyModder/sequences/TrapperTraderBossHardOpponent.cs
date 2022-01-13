using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using Infiniscryption.Curses.Patchers;
using HarmonyLib;

namespace Infiniscryption.Curses.Sequences
{
    public class TrapperTraderBossHardOpponent : TrapperTraderBossOpponent
    {
        public override int StartingLives => 3;

        private static Vector3 BOSS_CARD_RECEIVE_OFFSET = new Vector3(0f, 2f, 8f);

        // The harder version lights an extra candle
        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            yield return base.IntroSequence(encounter);
            yield return HarderBosses.ShowExtraBossCandle(this, "TrapperTraderExtraCandle");
        }

        private void RemoveCovered(List<CardInfo> list, Func<CardInfo, bool> predicate, int count)
        {
            List<CardInfo> covered = list.Where(predicate).Take(count).ToList();
            foreach (CardInfo card in covered)
                list.Remove(card);
        }

        private static SpecialStatIcon[] USELESS_STAT_ICONS = new SpecialStatIcon[] {
            SpecialStatIcon.None,
            SpecialStatIcon.CardsInHand,
            SpecialStatIcon.GreenGems,
            SpecialStatIcon.Mirror
        };

        private bool CardUsableByBoss(CardInfo card)
        {
            if (!card.Sacrificable)
                return false;

            if (card.Health == 0)
                return false;

            if (card.Attack == 0 && USELESS_STAT_ICONS.Any(st => st == card.SpecialStatIcon))
                return false;

            return true;
        }

        private List<CardInfo> BuildPhaseThreeDeck(List<CardInfo> trapperDeck)
        {
            List<CardInfo> retval = new List<CardInfo>();

            // Start with two of everything
            for (int i = 0; i < 2; i++)
            {
                retval.Add(CardLoader.GetCardByName("Trap"));
                retval.Add(CardLoader.GetCardByName("Trapper_Spike_Trap"));
                retval.Add(CardLoader.GetCardByName("Trapper_Capture"));
                retval.Add(CardLoader.GetCardByName("Trapper_Bow"));
            }

            // Let's figure out which cards in the opponent's deck aren't "covered"
            List<CardInfo> uncovered = new List<CardInfo>(trapperDeck);

            // These are covered by wolf pelts
            RemoveCovered(uncovered, card => !card.HasAbility(Ability.Flying) && card.HasAbility(Ability.Brittle), 3);

            // These are covered by spike traps
            Func<CardInfo, bool> spikePred = card => !card.HasAbility(Ability.Flying) && (card.Health == 1 || (card.Attack <= 2 || card.Health == 2));
            RemoveCovered(uncovered, spikePred, 2);

            // These are covered by bows
            Func<CardInfo, bool> bowPred = card => card.Health <=2;
            RemoveCovered(uncovered, bowPred, 2);

            // These are covered by the two traps and two captures
            // Sort by health biggest first
            uncovered.Sort((a, b) => b.Health - a.Health);
            uncovered.RemoveRange(0, Math.Min(4, uncovered.Count)); // These are taken out by traps and captures

            // Now we need to cover the rest.
            // Add up to two more spike traps
            for (int i = 0; i < 2; i ++)
            {
                CardInfo spikeable = uncovered.Find(spikePred.Invoke);
                if (spikeable != null)
                {
                    retval.Add(CardLoader.GetCardByName("Trapper_Spike_Trap"));
                    uncovered.Remove(spikeable);
                }
            }

            // Cover everything killable by bows
            List<CardInfo> bowKills = uncovered.Where(card => card.Health <= 2).ToList();
            foreach (CardInfo card in bowKills)
            {
                retval.Add(CardLoader.GetCardByName("Trapper_Bow"));
                uncovered.Remove(card);
            }

            // Cover up to one thing killable by two bows
            CardInfo twoBowKills = uncovered.Find(bowPred.Invoke);
            if (twoBowKills != null)
            {
                retval.Add(CardLoader.GetCardByName("Trapper_Bow"));
                retval.Add(CardLoader.GetCardByName("Trapper_Bow"));
                uncovered.Remove(twoBowKills);
            }

            // The rest in traps
            for (int i = 0; i < uncovered.Count; i++)
                retval.Add(CardLoader.GetCardByName("Trap"));

            // If you have no fishooks, add another capture card
            if (!RunState.Run.consumables.Any(c => c.ToLowerInvariant() == "fishhook"))
                retval.Add(CardLoader.GetCardByName("Trapper_Capture"));

            // If you have no scissors, add another leaping trap
            if (!RunState.Run.consumables.Any(c => c.ToLowerInvariant() == "scissors"))
                retval.Add(CardLoader.GetCardByName("Trap"));

            // Add a trap for each card in the original list with ice cube
            int iceCubes = trapperDeck.Where(card => card.HasAbility(Ability.IceCube)).Count();
            for (int i = 0; i < iceCubes; i++)
                retval.Add(CardLoader.GetCardByName("Trap"));

            // Adding two more bows for safety
            retval.Add(CardLoader.GetCardByName("Trapper_Bow"));
            retval.Add(CardLoader.GetCardByName("Trapper_Bow"));

            // If this is the first region, add another capture
            if (RunState.CurrentRegionTier == 0)
                retval.Add(CardLoader.GetCardByName("Trapper_Capture"));

            return retval;
        }

        private IEnumerator FlyBothDecksIntoOpponent()
        {
            CardDrawPiles3D drawPiles = CardDrawPiles3D.Instance;
            yield return drawPiles.Pile.DestroyCards(BOSS_CARD_RECEIVE_OFFSET, 30f, 0.5f);
            drawPiles.Deck.ClearCards();
            yield return drawPiles.SidePile.DestroyCards(BOSS_CARD_RECEIVE_OFFSET, 30f, 0.5f);
            drawPiles.SideDeck.ClearCards();
            //GameObject.Destroy(drawPiles.Deck);
            //GameObject.Destroy(drawPiles.SideDeck);
        }

        private IEnumerator GeneratePhaseThreeDeck(List<CardInfo> currentDeck)
        {
            // Here, we're going to manually do the generation of a deck while skipping the side deck
            List<CardInfo> phaseThreeDeck = BuildPhaseThreeDeck(currentDeck);

            CardDrawPiles3D drawPiles = CardDrawPiles3D.Instance;
            Traverse pileTraverse = Traverse.Create(drawPiles);
            drawPiles.Deck.Initialize(phaseThreeDeck, SaveManager.SaveFile.GetCurrentRandomSeed());
            //pileTraverse.Property("Deck").SetValue(SpawnDeckObject(phaseThreeDeck));
            //pileTraverse.Property("SideDeck").SetValue(SpawnDeckObject(new List<CardInfo>())); // The side deck is empty!
            yield return drawPiles.Pile.SpawnCards(phaseThreeDeck.Count, 0.5f);
            pileTraverse.Method("AssignHintActionToPiles").GetValue();

            // And draw a specific opening hand
            drawPiles.Pile.Draw();
            yield return drawPiles.DrawCardFromDeck(drawPiles.Deck.GetCardByName("Trap"));
            drawPiles.Pile.Draw();
            yield return drawPiles.DrawCardFromDeck(drawPiles.Deck.GetCardByName("Trapper_Spike_Trap"));
            drawPiles.Pile.Draw();
            yield return drawPiles.DrawCardFromDeck(drawPiles.Deck.GetCardByName("Trapper_Bow"));
            drawPiles.Pile.Draw();
            yield return drawPiles.DrawCardFromDeck(drawPiles.Deck.GetCardByName("Trapper_Bow"));

            yield break;
        }

        private CardInfo CloneWithModsForTrapperDeck(CardInfo info)
        {
            CardInfo retval = info.Clone() as CardInfo;
            foreach (CardModificationInfo mod in info.Mods)
                retval.Mods.Add(mod.Clone() as CardModificationInfo);

            CardModificationInfo clearUnusable = new CardModificationInfo();
            clearUnusable.negateAbilities = new List<Ability>();
            foreach (Ability ability in retval.Abilities.Where(ab => !AbilitiesUtil.GetInfo(ab).opponentUsable))
                clearUnusable.negateAbilities.Add(ability);
                
            retval.Mods.Add(clearUnusable);

            return retval;
        }

        private List<List<CardInfo>> BuildTurnPlan(List<CardInfo> currentDeck)
        {
            List<List<CardInfo>> turnPlan = new List<List<CardInfo>>();

            int idx = 0;
            while (idx < currentDeck.Count)
            {
                List<CardInfo> newTurn = new List<CardInfo>();
                newTurn.Add(CloneWithModsForTrapperDeck(currentDeck[idx]));
                idx += 1;
                if (idx < currentDeck.Count)
                {
                    newTurn.Add(CloneWithModsForTrapperDeck(currentDeck[idx]));
                    idx += 1;
                }

                turnPlan.Add(newTurn);

                if (turnPlan.Count > 5)
                    turnPlan.Add(new List<CardInfo>()); // Starting here, we start putting gaps in between
                                                        // Because the resources are wearing quite thin
            }

            return turnPlan;
        }


        // The harder version has an extra phase
        public override IEnumerator StartNewPhaseSequence()
        {
            if (this.NumLives >= 2)
            {
                yield return base.StartNewPhaseSequence();
                yield break;
            }

            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

            // Tell the player what we're going to do
            ViewManager.Instance.SwitchToView(View.Default);
            yield return TextDisplayer.Instance.PlayDialogueEvent("TrapperTraderPhaseThree", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            // Clear the board
            yield return this.ClearQueue();
            yield return this.ClearBoard();
            foreach (CardSlot cardSlot in Singleton<BoardManager>.Instance.PlayerSlotsCopy)
            {
                if (cardSlot.Card != null)
                {
                    cardSlot.Card.ExitBoard(0.4f, Vector3.zero);
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // Fly the deck and sidedeck into the boss
            yield return PlayerHand.Instance.CleanUp();
            yield return FlyBothDecksIntoOpponent();

            // Sort out the valid cards in the player's current deck
            List<CardInfo> currentDeck = SaveManager.SaveFile.CurrentDeck.Cards;

            currentDeck.Sort((a, b) => a.PowerLevel - b.PowerLevel);

            // Filter out the chaff
            currentDeck = currentDeck.Where(CardUsableByBoss).ToList();

            // Build a new deck
            // Draw the player's opening hand
            // Two traps, two bows
            PlayerHand.Instance.Initialize();
            yield return GeneratePhaseThreeDeck(currentDeck);

            // Build the turn plan
            this.Blueprint = null;
            this.ReplaceAndAppendTurnPlan(BuildTurnPlan(currentDeck));
            yield return this.QueueNewCards();

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
    }
}