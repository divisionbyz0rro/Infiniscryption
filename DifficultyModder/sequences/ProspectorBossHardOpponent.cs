using DiskCardGame;
using APIPlugin;
using Infiniscryption.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using Infiniscryption.Curses.Patchers;

namespace Infiniscryption.Curses.Sequences
{
    public class ProspectorBossHardOpponent : ProspectorBossOpponent
    {
        public override int StartingLives => 3;

        // The harder version lights an extra candle
        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            yield return base.IntroSequence(encounter);
            yield return HarderBosses.ShowExtraBossCandle(this, "ProspectorExtraCandle");
        }

        // The harder version has an extra phase
        protected override IEnumerator StartNewPhaseSequence()
        {
            if (this.NumLives >= 2)
            {
                yield return base.StartNewPhaseSequence();
                yield break;
            }

            // Here we do the extra phase
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

            // This phase will spawn boulders and then put booby trap dynamite in your hand every turn.
            ViewManager.Instance.SwitchToView(View.BossCloseup);
            yield return TextDisplayer.Instance.PlayDialogueEvent("ProspectorPhaseThree", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Board);

            // Clear out the queue and the board
            yield return this.ClearQueue();
            yield return this.ClearBoard();

            // Get rid of all gold on the board
            foreach (CardSlot slot in BoardManager.Instance.PlayerSlotsCopy)
                if (slot.Card != null && slot.Card.Info.name == "GoldNugget")
                    slot.Card.ExitBoard(0.4f, Vector3.zero);

            // We aren't going to use an encounter blueprint for this
            this.Blueprint = null;
            this.ReplaceAndAppendTurnPlan(new List<List<CardInfo>>()); // There are no cards in the plan!

            foreach (CardSlot slot in BoardManager.Instance.OpponentSlotsCopy)
            {
                CardInfo bigBoulder = CardLoader.GetCardByName("Boulder");
                bigBoulder.Mods.Add(new CardModificationInfo(Ability.Reach));
                yield return BoardManager.Instance.CreateCardInSlot(bigBoulder, slot);
                yield return new WaitForSeconds(0.15f);
            }

            yield return new WaitForSeconds(0.35f);

            ViewManager.Instance.SwitchToView(View.Default);

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
    }
}