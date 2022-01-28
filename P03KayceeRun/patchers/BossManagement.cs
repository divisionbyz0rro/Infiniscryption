using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Sequences;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BossManagement
    {
        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.IntroSequence))]
        [HarmonyPostfix]
        public static IEnumerator ReduceLivesOnBossNode(IEnumerator sequence)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }

            bool hasShownLivesDrop = false;
            while (sequence.MoveNext())
            {
                if (sequence.Current is WaitForSeconds)
                {
                    yield return sequence.Current;
                    sequence.MoveNext();

                    if (EventManagement.NumberOfLivesRemaining > 1 && !hasShownLivesDrop)
                    {
                        int livesToDrop = EventManagement.NumberOfLivesRemaining - 1;
                        yield return P03LivesFace.ShowChangeLives(-livesToDrop, true);
                        EventManagement.NumberOfLivesRemaining = 1;

                        if (!StoryEventsData.EventCompleted(EventManagement.ONLY_ONE_BOSS_LIFE))
                        {
                            yield return TextDisplayer.Instance.PlayDialogueEvent("P03OnlyOneBossLife", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                            StoryEventsData.SetEventCompleted(EventManagement.ONLY_ONE_BOSS_LIFE);
                        }
                    }
                    hasShownLivesDrop = true;
                }
                yield return sequence.Current;
            }
            yield break;
        }

        [HarmonyPatch(typeof(CanvasBossOpponent), nameof(CanvasBossOpponent.IntroSequence))]
        [HarmonyPostfix]
        public static IEnumerator CanvasResetLives(IEnumerator sequence)
        {
            yield return ReduceLivesOnBossNode(sequence);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.BossDefeatedSequence))]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03ResetLives(IEnumerator sequence)
        {
            if (SaveFile.IsAscension)
            {
                // Reset lives to maximum
                if (EventManagement.NumberOfLivesRemaining < AscensionSaveData.Data.currentRun.maxPlayerLives)
                {
                    int livesToAdd = AscensionSaveData.Data.currentRun.maxPlayerLives - EventManagement.NumberOfLivesRemaining;
                    yield return P03LivesFace.ShowChangeLives(livesToAdd, true);
                    yield return new WaitForSeconds(0.5f);
                    EventManagement.NumberOfLivesRemaining = AscensionSaveData.Data.currentRun.maxPlayerLives;
                }
            }

            yield return sequence;
            yield break;
        }

        [HarmonyPatch(typeof(HoloGameMap), "BossDefeatedSequence")]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03BossDefeatedSequence(IEnumerator sequence, StoryEvent bossDefeatedStoryEvent)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }

            EventManagement.AddCompletedZone(bossDefeatedStoryEvent);

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoBossRares))
            {
                Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN));
                ChallengeActivationUI.TryShowActivation(AscensionChallenge.NoBossRares);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossDraftToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }
            else
            {
                Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossRareToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            yield return FastTravelManagement.ReturnToHomeBase();

            yield break;
        }

        private static void AddBossSequencer<T>(TurnManager manager) where T : SpecialBattleSequencer
        {
            GameObject.Destroy(manager.SpecialSequencer);
            SpecialBattleSequencer sequencer = manager.gameObject.AddComponent<T>();
            Traverse trav = Traverse.Create(manager);
            trav.Property("SpecialSequencer").SetValue(sequencer);
        }

        [HarmonyPatch(typeof(TurnManager), "UpdateSpecialSequencer")]
        [HarmonyPrefix]
        public static bool ReplaceSequencers(string specialBattleId, ref TurnManager __instance)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
            {
                if (specialBattleId == BossBattleSequencer.GetSequencerIdForBoss(Opponent.Type.TelegrapherBoss))
                {
                    AddBossSequencer<TelegrapherAscensionSequencer>(__instance);
                    return false;
                }

                if (specialBattleId == P03FinalBossSequencer.SequenceID)
                {
                    AddBossSequencer<P03FinalBossSequencer>(__instance);
                    return false;
                }

                if (specialBattleId == BossBattleSequencer.GetSequencerIdForBoss(Opponent.Type.CanvasBoss))
                {
                    AddBossSequencer<CanvasAscensionSequencer>(__instance);
                    return false;
                }
            }

            return true;
        }

        private static Opponent.Type[] SUPPORTED_OPPONENTS = new Opponent.Type[] { 
            Opponent.Type.TelegrapherBoss,
            P03AscensionOpponent.ID,
            Opponent.Type.ArchivistBoss
        };

        [HarmonyPatch(typeof(BossBattleSequencer), nameof(BossBattleSequencer.GetSequencerIdForBoss))]
        [HarmonyPrefix]
        public static bool GetP03ID(ref string __result, Opponent.Type bossType)
        {
            if (bossType == P03AscensionOpponent.ID)
            {
                __result = P03FinalBossSequencer.SequenceID;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Opponent), "SpawnOpponent")]
        [HarmonyPrefix]
        public static bool ReplaceOpponent(EncounterData encounterData, ref Opponent __result)
        {
            if (!(SaveFile.IsAscension && P03AscensionSaveData.IsP03Run))
                return true;

            if (!SUPPORTED_OPPONENTS.Contains(encounterData.opponentType))
                return true;

            GameObject gameObject = new GameObject();
			gameObject.name = "Opponent";
			Opponent.Type opponentType = encounterData.opponentType;
			Opponent opponent;

			if (opponentType == Opponent.Type.TelegrapherBoss)
                opponent = gameObject.AddComponent<TelegrapherAscensionOpponent>();
			else if(opponentType == P03AscensionOpponent.ID)
                opponent = gameObject.AddComponent<P03AscensionOpponent>();
            else if (opponentType == Opponent.Type.ArchivistBoss)
                opponent = gameObject.AddComponent<ArchivistAscensionOpponent>();
            else
                throw new InvalidOperationException("Somehow got into a patch for ascension opponents that's not supported");

			string text = encounterData.aiId;
			if (string.IsNullOrEmpty(text))
			{
				text = "AI";
			}
			opponent.AI = opponentType == P03AscensionOpponent.ID ? new P03FinalBossOpponentAI() : (Activator.CreateInstance(CustomType.GetType("DiskCardGame", text)) as AI);
			opponent.NumLives = opponent.StartingLives;
			opponent.OpponentType = opponentType;
			opponent.TurnPlan = opponent.ModifyTurnPlan(encounterData.opponentTurnPlan);
			opponent.Blueprint = encounterData.Blueprint;
			opponent.Difficulty = encounterData.Difficulty;
			opponent.ExtraTurnsToSurrender = SeededRandom.Range(0, 3, SaveManager.SaveFile.GetCurrentRandomSeed());
			__result = opponent;
            return false;
        }
    }
}