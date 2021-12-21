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
            yield return PlayerHand.Instance.AddCardToHand(dynamite, Vector3.zero, 0f);
            PlayerHand.Instance.OnCardInspected(dynamite);
            PlayerHand.Instance.InspectingLocked = true;

            dynamite.Anim.PlayHitAnimation();
            yield return new WaitForSeconds(0.6f);

            PlayerHand.Instance.InspectingLocked = false;
            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.33f);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }

        private List<CardSlot> EmptyLanes()
        {
            return BoardManager.Instance.OpponentSlotsCopy.Where(s => s.Card == null && s.opposingSlot.Card == null).ToList();
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd)
        {
            // If there is an empty lane, queue a wolf for it.
            // The goal here is to ensure that the player doesn't just cheese the battle
            // by putting all the dynamite on board every time.
            return !playerTurnEnd && EmptyLanes().Count > 0 && TurnManager.Instance.Opponent.NumLives == 1;
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            List<CardSlot> wolfSlots = EmptyLanes();
            if (playerTurnEnd || wolfSlots.Count == 0 || TurnManager.Instance.Opponent.NumLives > 1)
                yield break;

            foreach (CardSlot slot in wolfSlots)
            {
                yield return TurnManager.Instance.Opponent.QueueCard(CardLoader.GetCardByName("Wolf"), slot);
                if (DialogueEventsData.GetEventRepeatCount("ProspectorWolfSpawn") == 0)
                    yield return TextDisplayer.Instance.PlayDialogueEvent("ProspectorWolfSpawn", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            yield break;
        }
    }
}