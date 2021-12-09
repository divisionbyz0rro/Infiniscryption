using DiskCardGame;
using APIPlugin;
using Infiniscryption.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;

namespace Infiniscryption.Curses.Sequences
{
    public class ProspectorBossHardSequencer : ProspectorBattleSequencer
    {
        // At the end of his turn, he throws dynamite at us

        public override IEnumerator PlayerPostDraw()
        {
        
            if (TurnManager.Instance.Opponent.NumLives > 1)
                yield break;

            //InfiniscryptionCursePlugin.Log.LogInfo($"Turn end");

            // We need to spawn a card in the player's hand
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            ViewManager.Instance.SwitchToView(View.Default);
            yield return TextDisplayer.Instance.PlayDialogueEvent("CatchDynamite", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Hand);

            PlayableCard dynamite = CardSpawner.SpawnPlayableCard(CardLoader.GetCardByName("Prospector_Dynamite"));
            yield return PlayerHand.Instance.AddCardToHand(dynamite);
            PlayerHand.Instance.OnCardInspected(dynamite);
            PlayerHand.Instance.InspectingLocked = true;

            dynamite.Anim.PlayHitAnimation();
            yield return new WaitForSeconds(0.6f);

            PlayerHand.Instance.InspectingLocked = false;
            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.33f);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
    }
}