using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class TelegrapherAscensionOpponent : TelegrapherBossOpponent
    {
        public override IEnumerator StartNewPhaseSequence()
        {
            // Do nothing if this is not the ascension boss fight
            if (!SaveFile.IsAscension)
            {
                yield return base.StartNewPhaseSequence();
                yield break;
            }

            // Okay, we are no longer playing the original boss behavior.
            // We are solely 

            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

            // This phase will spawn boulders and then put booby trap dynamite in your hand every turn.
            ViewManager.Instance.SwitchToView(View.P03Face);
            yield return TextDisplayer.Instance.PlayDialogueEvent("TelegrapherBlockchainIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Board);
            VideoCameraRig.Instance.VOPlayer.PlayVoiceOver("Shit.", "VO_shit");

            // Clear out the queue and the board
            yield return this.ClearQueue();
            yield return this.ClearBoard();

            // No more blueprints
            this.Blueprint = null;
            this.TurnPlan = new();

            yield return BoardManager.Instance.CreateCardInSlot( CardLoader.GetCardByName(CustomCards.BLOCKCHAIN), BoardManager.Instance.OpponentSlotsCopy[0]);
            yield return new WaitForSeconds(0.15f);
            yield return BoardManager.Instance.CreateCardInSlot( CardLoader.GetCardByName(CustomCards.BLOCKCHAIN), BoardManager.Instance.OpponentSlotsCopy[4]);
            yield return new WaitForSeconds(0.15f);

            yield return new WaitForSeconds(0.75f);
            ViewManager.Instance.SwitchToView(View.P03Face);
            yield return TextDisplayer.Instance.PlayDialogueEvent("TelegrapherCryptoSpawn", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Board);

            for (int i = 1; i < 4; i++)
            {
                yield return BoardManager.Instance.CreateCardInSlot( CardLoader.GetCardByName(CustomCards.GOLLYCOIN), BoardManager.Instance.OpponentSlotsCopy[i]);
                yield return new WaitForSeconds(0.15f);
            }
            yield return new WaitForSeconds(0.75f);
            
            ViewManager.Instance.SwitchToView(View.Default);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }

        public override IEnumerator QueueNewCards(bool doTween = true, bool changeView = true)
        {
            if (this.NumLives == this.StartingLives)
            {
                yield return base.QueueNewCards(doTween, changeView);
                yield break;
            }

            // We do nothing in phase two; the sequencer handles that
            yield break;
        }
    }
}