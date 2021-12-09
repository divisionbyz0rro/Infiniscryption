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
using Infiniscryption.Curses.Cards;
using Infiniscryption.Curses.Sequences;
using System.Linq;

namespace Infiniscryption.Curses.Patchers
{
    public class HarderBosses : CurseBase
    {
        public override string Description => $"Each of the bosses has another phase that you must fight through.";
        public override string Title => "Revenge";

        Texture2D _iconTexture = AssetHelper.LoadTexture("clover_icon");
        public override Texture2D IconTexture => _iconTexture;

        public HarderBosses(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) {}

        public override void Reset()
        {
            // We don't have to do anything during a run.
            // So this stays empty
        }

        public static void RegisterCustomCards(Harmony harmony)
        {
            // Prospector custom cards
            Dynamite.RegisterCardAndAbilities(harmony);
        }

        // Helper for opponents
        internal static IEnumerator ShowExtraBossCandle(Part1BossOpponent opponent, string dialogueEvent)
        {
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            ViewManager.Instance.SwitchToView(View.BossSkull);
            yield return new WaitForSeconds(0.25f);
            yield return TextDisplayer.Instance.PlayDialogueEvent(dialogueEvent, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.4f);
            opponent.gameObject.GetComponentInChildren<BossSkull>().EnterHand();
            yield return new WaitForSeconds(3.5f);
            ViewManager.Instance.SwitchToView(View.Default);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void BossDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("ProspectorExtraCandle", new string[] {
                "Let's play longer this time!"
            }, new string[][] {
                new string[] {
                    "A lil' more fun never hurt nobody!"
                },
                new string[] {
                    "I got a little somethin' special for ya!"
                },
                new string[] {
                    "Heeeeeeee-haaw! Let's keep the fun goin'!"
                }
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("CatchDynamite", new string[] {
                "Think fast!"
            }, new string[][] {
                new string[] {
                    "Catch!"
                },
                new string[] {
                    "Heads up!"
                }
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("ProspectorPhaseThree", new string[] {
                "You're a dad-gum pain in my backside!",
                "Let's see you get past this!"
            }, new string[][] {
                new string[] {
                    "HEE-HEE-HEE-HAAAAWWW!",
                    "I'm gonna sit a spell over here"
                },
                new string[] {
                    "How in tarnation are you doing this to me?"
                }
            });

            // This is also a good time to load audio
            try
            {
                AssetHelper.LoadAudioClip(Dynamite.EXPLOSION_SOUND, group: "SFX");
            } catch (Exception e)
            {
                InfiniscryptionCursePlugin.Log.LogError(e);
            }
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
            if (CurseManager.IsActive<HarderBosses>())
            {
                if (specialBattleId == BossBattleSequencer.GetSequencerIdForBoss(Opponent.Type.ProspectorBoss))
                    AddBossSequencer<ProspectorBossHardSequencer>(__instance);

                return false;
            }

            return true;
        }

        private static Opponent.Type[] SUPPORTED_OPPONENTS = new Opponent.Type[] { Opponent.Type.ProspectorBoss };

        [HarmonyPatch(typeof(Opponent), "SpawnOpponent")]
        [HarmonyPrefix]
        public static bool ReplaceOpponent(EncounterData encounterData, ref Opponent __result)
        {
            if (!CurseManager.IsActive<HarderBosses>())
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