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
using Infiniscryption.Core.Helpers;
using Infiniscryption.Curses.Cards;
using Infiniscryption.Curses.Sequences;
using System.Linq;
using InscryptionAPI.Ascension;

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
                AssetHelper.LoadTexture("challenge_boss_revenge"),
                AssetHelper.LoadTexture("ascensionicon_activated_bossrevenge")
            ).challengeType;

            harmony.PatchAll(typeof(HarderBosses));

            // Custom cards
            Dynamite.RegisterCardAndAbilities(harmony);
            Digester.RegisterCardAndAbilities(harmony);
            Bitten.RegisterCardAndAbilities(harmony);
            Bow.RegisterCardAndAbilities(harmony);
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

            DialogueHelper.AddOrModifySimpleDialogEvent("AnglerExtraCandle", new string[] {
                "You want more fish?"
            }, new string[][] {
                new string[] {
                    "We get extra fish."
                },
                new string[] {
                    "Many fish this time."
                }
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("TrapperTraderExtraCandle", new string[] {
                "I'm afraid our encounter today will take a little more time"
            }, new string[][] {
                new string[] {
                    "It looks like you and I have extra business to attend to"
                }
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("CardIsSleep", new string[] {
                "He swalled it whole..."
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("CardAtePoison", new string[] {
                "Eating that was a bad idea..."
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("CardAteSharp", new string[] {
                "That's a little uncomfortable..."
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("DigestedCardDeadForever", new string[] {
                "Your [c:bR][v:0][c:] has been fully digested. You will never see it again."
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("DigestingCard", new string[] {
                "Your [c:bR][v:0][c:] is being slowly digested. It has [c:bR][v:1][c:] health left."
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("TrapperTraderPhaseThree", new string[] {
                "I have yet another trade to propose",
                "This time, let us trade our whole decks"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("HungryAgain", new string[] {
                "He's hungry again"
            }, new string[][] {
                new string[] {
                    "Time for more morsels"
                },
                new string[] {
                    "Back on the hunt"
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

            DialogueHelper.AddOrModifySimpleDialogEvent("AnglerPhaseThree", new string[] {
                "Fish not big enough. Get bigger fish.",
            }, new string[][] {
                new string[] {
                    "Little fish bad. Big fish better."
                },
                new string[] {
                    "Need better fish."
                }
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("ProspectorWolfSpawn", new string[] {
                "That fella looks mighty curious about that [c:bR]empty lane[c:]",
            });

            // This is also a good time to load audio
            try
            {
                AssetHelper.LoadAudioClip(Dynamite.EXPLOSION_SOUND, group: "SFX");
                AssetHelper.LoadAudioClip(Digester.GULP_SOUND, group: "SFX");
            } catch (Exception e)
            {
                CursePlugin.Log.LogError(e);
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