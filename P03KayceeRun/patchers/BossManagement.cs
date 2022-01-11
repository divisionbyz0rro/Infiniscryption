using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Sequences;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BossManagement
    {
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
            }

            return true;
        }

        private static Opponent.Type[] SUPPORTED_OPPONENTS = new Opponent.Type[] { 
            Opponent.Type.TelegrapherBoss
        };

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
			switch (opponentType)
			{
			    case Opponent.Type.TelegrapherBoss:
				    opponent = gameObject.AddComponent<TelegrapherAscensionOpponent>();
				    break;
			
			    default:
                    throw new InvalidOperationException("Somehow got into a patch for ascension opponents that's not supported");
			}
			string text = encounterData.aiId;
			if (string.IsNullOrEmpty(text))
			{
				text = "AI";
			}
			opponent.AI = (Activator.CreateInstance(CustomType.GetType("DiskCardGame", text)) as AI);
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