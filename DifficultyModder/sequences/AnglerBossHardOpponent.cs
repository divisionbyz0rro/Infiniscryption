using DiskCardGame;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using Infiniscryption.Curses.Patchers;

namespace Infiniscryption.Curses.Sequences
{
    public class AnglerBossHardOpponent : AnglerBossOpponent
    {
        public static readonly string MEGA_SHARK = $"{CursePlugin.CardPrefix}_Angler_Shark";

        public override int StartingLives => 3;

        // The harder version lights an extra candle
        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            yield return base.IntroSequence(encounter);
            yield return HarderBosses.ShowExtraBossCandle(this, "AnglerExtraCandle");
        }

        // The harder version has an extra phase
        public override IEnumerator StartNewPhaseSequence()
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
            yield return TextDisplayer.Instance.PlayDialogueEvent("AnglerPhaseThree", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Board);

            // Clear out the queue and the board
            yield return this.ClearQueue();
            yield return this.ClearBoard();

            // We aren't going to use an encounter blueprint for this
            this.Blueprint = null;
            this.ReplaceAndAppendTurnPlan(new List<List<CardInfo>>()); // There are no cards in the plan!

            List<CardSlot> slots = BoardManager.Instance.OpponentSlotsCopy;
            int numberOfSharks = 2;
            if (SaveFile.IsAscension && AscensionSaveData.Data.currentRun.regionTier == 0)
                numberOfSharks = 1;
            for (int i = 0; i < numberOfSharks; i++)
            {
                int slotNum = i == 0 ? 1 : i == 1 ? 3 : i == 2 ? 2 : 0;
                CardInfo shark = CardLoader.GetCardByName(MEGA_SHARK);

                if (SaveFile.IsAscension && AscensionSaveData.Data.currentRun.regionTier == 2)
                    shark.mods.Add(new CardModificationInfo(1, 1));

                yield return BoardManager.Instance.CreateCardInSlot(shark, slots[slotNum]);
                yield return new WaitForSeconds(0.15f);
            }

            yield return new WaitForSeconds(0.75f);

            ViewManager.Instance.SwitchToView(View.Default);

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
    }
}