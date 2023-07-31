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
using InscryptionAPI.Dialogue;
using InscryptionAPI.Helpers;
using Infiniscryption.Curses.Cards;
using Infiniscryption.Curses.Sequences;
using System.Linq;
using InscryptionAPI.Ascension;
using Infiniscryption.Curses.Helpers;

namespace Infiniscryption.Curses.Patchers
{
    public static class HarderBosses
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Boss Revenge",
                "Each boss has an additional phase",
                10,
                TextureHelper.GetImageAsTexture("challenge_boss_revenge.png", typeof(HarderBosses).Assembly),
                TextureHelper.GetImageAsTexture("ascensionicon_activated_bossrevenge.png", typeof(HarderBosses).Assembly)
            ).Challenge.challengeType;

            harmony.PatchAll(typeof(HarderBosses));

            // Custom cards
            Dynamite.RegisterCardAndAbilities(harmony);
            Digester.RegisterCardAndAbilities(harmony);
            Bitten.RegisterCardAndAbilities(harmony);
            Bow.RegisterCardAndAbilities(harmony);

            // Dialogue
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.GenerateDialogue(
                "ProspectorExtraCandle",
                "Let's play longer this time!", 
                "A lil' more fun never hurt nobody!", 
                "I got a little somethin' special for ya!", 
                "Heeeeeeee-haaw! Let's keep the fun goin'!"
            );

            DialogueHelper.GenerateDialogue(
                "AnglerExtraCandle",
                "You want more fish?",
                "We get extra fish.",
                "Many fish this time."
            );

            DialogueHelper.GenerateDialogue(
                "TrapperTraderExtraCandle",
                "I'm afraid our encounter today will take a little more time",
                "It looks like you and I have extra business to attend to"
            );

            DialogueHelper.GenerateDialogue("CardIsSleep", "He swalled it whole..");
            DialogueHelper.GenerateDialogue("CardAtePoison", "Eating that was a bad idea..");
            DialogueHelper.GenerateDialogue("CardAteSharp", "That's a little uncomfortable..");
            DialogueHelper.GenerateDialogue("DigestedCardDeadForever", "Your [c:bR][v:0][c:] has been fully digested. You will never see it again.");
            DialogueHelper.GenerateDialogue("DigestingCard", "Your [c:bR][v:0][c:] is being slowly digested. It has [c:bR][v:1][c:] health left.");

            DialogueHelper.GenerateLargeDialogue(
                "TrapperTraderPhaseThree",
                "I have yet another trade to propose",
                "This time, let us trade our whole decks"
            );

            DialogueHelper.GenerateDialogue(
                "HungryAgain",
                "He's hungry again",
                "Time for more morsels",
                "Back on the hunt"
            );

            DialogueHelper.GenerateDialogue(
                "CatchDynamite",
                "Think fast!",
                "Catch!",
                "Heads up!"
            );

            DialogueHelper.GenerateLargeDialogue(
                "ProspectorPhaseThree",
                "You're a dad-gum pain in my backside!",
                "Let's see you get past this!"
            );

            DialogueHelper.GenerateDialogue(
                "AnglerPhaseThree",
                "Fish not big enough. Get bigger fish.",
                "Little fish bad. Big fish better.",
                "Need better fish."
            );

            DialogueHelper.GenerateDialogue(
                "ProspectorWolfSpawn",
                "That fella looks mighty curious about that [c:bR]empty lane[c:]"
            );
        }

        // Helper for opponents
        internal static IEnumerator ShowExtraBossCandle(Part1BossOpponent opponent, string dialogueEvent)
        {
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            ChallengeActivationUI.TryShowActivation(ID);
            ViewManager.Instance.SwitchToView(View.BossSkull);
            yield return new WaitForSeconds(0.25f);
            yield return TextDisplayer.Instance.PlayDialogueEvent(dialogueEvent, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.4f);
            opponent.gameObject.GetComponentInChildren<BossSkull>().EnterHand();
            yield return new WaitForSeconds(3.5f);
            ViewManager.Instance.SwitchToView(View.Default);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
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
            if (AscensionSaveData.Data.ChallengeIsActive(ID))
            {
                if (specialBattleId == BossBattleSequencer.GetSequencerIdForBoss(Opponent.Type.ProspectorBoss))
                {
                    AddBossSequencer<ProspectorBossHardSequencer>(__instance);
                    return false;
                }

                if (specialBattleId == BossBattleSequencer.GetSequencerIdForBoss(Opponent.Type.AnglerBoss))
                {
                    AddBossSequencer<AnglerBossHardSequencer>(__instance);
                    return false;
                }

                if (specialBattleId == BossBattleSequencer.GetSequencerIdForBoss(Opponent.Type.TrapperTraderBoss))
                {
                    AddBossSequencer<TrapperTraderBossHardSequencer>(__instance);
                    return false;
                }
            }

            return true;
        }

        private static Opponent.Type[] SUPPORTED_OPPONENTS = new Opponent.Type[] { 
            Opponent.Type.ProspectorBoss,
            Opponent.Type.AnglerBoss,
            Opponent.Type.TrapperTraderBoss
        };

        [HarmonyPatch(typeof(Opponent), "SpawnOpponent")]
        [HarmonyPrefix]
        public static bool ReplaceOpponent(EncounterData encounterData, ref Opponent __result)
        {
            if (!AscensionSaveData.Data.ChallengeIsActive(ID))
                return true;

            if (!SUPPORTED_OPPONENTS.Contains(encounterData.opponentType))
                return true;

            GameObject gameObject = new GameObject();
			gameObject.name = "Opponent";
			Opponent.Type opponentType = encounterData.opponentType;
			Opponent opponent;
			switch (opponentType)
			{
			    case Opponent.Type.ProspectorBoss:
				    opponent = gameObject.AddComponent<ProspectorBossHardOpponent>();
				    break;

                case Opponent.Type.AnglerBoss:
				    opponent = gameObject.AddComponent<AnglerBossHardOpponent>();
				    break;

                case Opponent.Type.TrapperTraderBoss:
				    opponent = gameObject.AddComponent<TrapperTraderBossHardOpponent>();
				    break;
			
			    default:
                    throw new InvalidOperationException("Somehow got into a patch for hard opponents with an unsupported opponent");
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