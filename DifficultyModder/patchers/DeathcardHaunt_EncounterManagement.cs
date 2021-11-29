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
using Infiniscryption.Curses.Sequences;
using Infiniscryption.Core.Helpers;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Infiniscryption.Curses.Patchers
{
    public partial class DeathcardHaunt : CurseBase
    {
        public DeathcardHaunt(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) { }

        public override string Description => "You will be haunted by those who came before you and their essences will oppose you in battle. Your haunt level will increase the more you win.";

        public override string Title => "Haunted Pasts";

        private Texture2D _iconTexture = AssetHelper.LoadTexture("deathcard_icon");
        public override Texture2D IconTexture => _iconTexture;

        public override void Reset()
        {
            // We don't need to do anything because the only meaningful variable is stored as a runstate variable.
        }

        private const string DEATHCARD_INTRO_CLIP = "wind_blowing_loop";

        public static bool RollForDeathcard()
        {
            if (HauntLevel <= 1)
                return false; // You can't get it at haunt level 1. You gotta get to 2 to start.

            float randomValue = SeededRandom.Value(SaveManager.SaveFile.GetCurrentRandomSeed());
            return (randomValue < (float)HauntLevel / (float)MAX_HAUNT_LEVEL) && CurseManager.IsActive<DeathcardHaunt>();
        }

        // This handles haunt level management at the cleanup phase of a fight.
        // It increases when you win, and resets to 0 when you lose.
        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPrefix]
        public static void ResetPlayerHauntLevelWhenBattleLost(ref TurnManager __instance)
        {
            bool playerWon = __instance.Opponent.NumLives <= 0 || __instance.Opponent.Surrendered; // Can't use __instance.PlayerWon because it hasn't been set yet
            InfiniscryptionCursePlugin.Log.LogInfo($"Battle over. Player Won = {playerWon}");
            if (playerWon)
            {
                if (_sawDeathcard)
                {
                    // Winning a battle against a deathcard decreases the haunt by 2
                    IncreaseHaunt(-2);
                } else {
                    IncreaseHaunt();
                }
            }
            else // Losing a battle resets the haunt
                ResetHaunt();

            // Always clear the audio state
            _sawDeathcard = false;
            _pausedState = null;
            _deathcardOnBoard = null;
        }

        public static double TurnAverage(IEnumerable<CardInfo> turn)
        {
            int length = 0;
            double sum = 0d;
            foreach (CardInfo card in turn)
            {
                sum += card.PowerLevel;
                length += 1;
            }
            return length == 0 ? -100 : sum / length;
        }

        // This manages adding deathcards to the encounter
        [HarmonyPatch(typeof(EncounterBuilder), "Build")]
        [HarmonyPostfix]
        public static void AddDeathcardToEncounter(ref EncounterData __result, CardBattleNodeData nodeData)
        {
            if (!CurseManager.IsActive<DeathcardHaunt>())
                return;

            // We don't do this to boss battles
            if (nodeData is BossBattleNodeData)
                return;

            InfiniscryptionCursePlugin.Log.LogInfo("Checking to see if we should add a deathcard...");

            // And let's check the haunt
            if (!RollForDeathcard())
                return;

            InfiniscryptionCursePlugin.Log.LogInfo("Adding a deathcard...");

            // Let's make sure the deathcard shows up in the first four turns of the game
            // But not on the very first turn
            List<List<CardInfo>> tp = __result.opponentTurnPlan.GetRange(1, 3);

            // Okay, time to add a deathcard!
            // First, we need to create one
            CardInfo deathcard = GetRandomDeathcard();

            // When bounty hunters are added to the turn plan, they leverage the 'energy cost' concept
            // which directly correlates to turn numbers. I.e., DM tries to add bounty hunters to the turn
            // plan in a way that makes it kinda fair - they'll show up on a turn that correlates when you could
            // have played them.

            // That's harder to figure out here. 
            // So let's just take the easy way out for now.
            // Let's look at the average power level of each card in each turn.
            // Then insert the deathcard at the turn where its power level most closely matches
            List<double> differences = tp.Select(cards => Math.Abs(deathcard.PowerLevel - TurnAverage(cards))).ToList();
            int idealTurn = Enumerable.Range(0, differences.Count).Aggregate((a, b) => (differences[a] < differences[b] ? a : b));

            // This turn has an average power level that closest matches the deathcard.
            // Now let's put it in. We'll replace the weakest card with the deathcard
            int weakestIndex = Enumerable.Range(0, tp[idealTurn].Count).Aggregate((a, b) => (tp[idealTurn][a].PowerLevel < tp[idealTurn][b].PowerLevel ? a : b));

            // Replace the weakest card in the ideal turn with the deathcard
            tp[idealTurn][weakestIndex] = deathcard;

            _deathcardOnBoard = deathcard;
            SetHauntedCardSlot(deathcard, weakestIndex);

            // And we're done! The weakest card in the ideal turn now has a deathcard insted.
            InfiniscryptionCursePlugin.Log.LogInfo($"Added a deathcard in turn {idealTurn} in slot {weakestIndex}");
        }

        [HarmonyPatch(typeof(Opponent), "CanOfferSurrender")]
        [HarmonyPostfix]
        public static void PreventSurrenderIfDeathcardInPlan(ref bool __result)
        {
            if (_deathcardOnBoard != null)
                __result = false;
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void DeathcardDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("DeathcardArrives", new string[] {
                "you feel a chill in the air",
                "the hair stands up on the back of your neck",
                "[c:bR][v:0][c:] has arrived"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("DeathcardZoom", new string[] {
                "[c:O]\"I have been looking for you\"[c:] says the apparition",
                "[c:O]\"And now you must die!\"[c:]"
            }, new string[][]{
                new string[] {
                    "the apparition has nothing to say",
                    "but you sense that it is here for you"
                },
                new string[] {
                    "[c:O]\"you made a mistake coming here\"[c:] it growls",
                    "[c:O]\"let me show you the way home\"[c:]"
                },
                new string[] {
                    "the apparition seems lonely",
                    "it wants you to come join it in the afterlife"
                }
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("DeathcardDies", new string[] {
                "the apparition fades into the wind",
                "you can't shake the feeling there are [c:O]more[c:] out there waiting"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("DeathcardWins", new string[] {
                "[c:bR][v:0][c:] shivers with delight as your candle is extinguished",
            });

            // This is also a good time to load audio
            try
            {
                AssetHelper.LoadAudioClip(DEATHCARD_INTRO_CLIP);
            } catch (Exception e)
            {
                InfiniscryptionCursePlugin.Log.LogError(e);
            }
        }

        private static CardInfo _deathcardOnBoard = null;

        private static bool _sawDeathcard = false;

        private static List<AudioHelper.AudioState> _pausedState = null;
        
        [HarmonyPatch(typeof(Opponent), "QueueCard")]
        [HarmonyPostfix]
        public static IEnumerator PlayDeathcardIntro(IEnumerator sequenceEvent, CardInfo cardInfo, CardSlot slot)
        {
            InfiniscryptionCursePlugin.Log.LogInfo("In QueueCard");

            sequenceEvent.MoveNext();
            yield return sequenceEvent.Current;
            sequenceEvent.MoveNext();

            // Now we check for our custom card
            int customSlot = GetHauntedCardSlot(cardInfo);
            if (customSlot >= 0)
            {
                _sawDeathcard = true;

                InfiniscryptionCursePlugin.Log.LogInfo("Playing animation");
                View oldView = ViewManager.Instance.CurrentView;
                ViewManager.Instance.SwitchToView(View.P03Face);

                _pausedState = AudioHelper.PauseAllLoops();
                AudioController.Instance.SetLoopAndPlay(DEATHCARD_INTRO_CLIP);

                yield return TextDisplayer.Instance.PlayDialogueEvent("DeathcardArrives", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] {
                    cardInfo.DisplayedNameLocalized
                }, null);
                yield return new WaitForSeconds(0.25f);

                // Figure out where we want the camera to be.
                ViewInfo targetPos = ViewManager.GetViewInfo(View.OpponentQueue);
                Vector3 translationOffset = (targetPos.camPosition + SLOT_OFFSETS[slot.Index]) - ViewManager.GetViewInfo(ViewManager.Instance.CurrentView).camPosition;
                Vector3 rotationOffset = targetPos.camRotation - ViewManager.GetViewInfo(ViewManager.Instance.CurrentView).camRotation;

                ViewManager.Instance.OffsetPosition(translationOffset, 0.75f);
                ViewManager.Instance.OffsetRotation(rotationOffset, 0.75f);

                yield return TextDisplayer.Instance.PlayDialogueEvent("DeathcardZoom", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

                yield return new WaitForSeconds(0.5f);

                ViewManager.Instance.SwitchToView(oldView);
            }
        }

        public static IEnumerator DeathcardOuttroSequence()
        {
            yield return TextDisplayer.Instance.PlayDialogueEvent("DeathcardDies", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            
            if (_pausedState != null)
            {
                AudioController.Instance.StopAllLoops();
                AudioHelper.ResumeAllLoops(_pausedState);
            }

            IncreaseHaunt(-1); // Killing a deathcard decreases the haunt
        }

        [HarmonyPatch(typeof(CandleHolder), "BlowOutCandleSequence")]
        [HarmonyPostfix]
        public static IEnumerator DeathcardWonSequence(IEnumerator sequenceResult)
        {
            if (_deathcardOnBoard == null || !CurseManager.IsActive<DeathcardHaunt>())
            {
                while(sequenceResult.MoveNext())
                    yield return sequenceResult.Current;
                yield break;
            }

            sequenceResult.MoveNext();
            yield return sequenceResult.Current;

            yield return TextDisplayer.Instance.PlayDialogueEvent("DeathcardWins", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] {
                _deathcardOnBoard.DisplayedNameLocalized
            }, null);

            while (sequenceResult.MoveNext())
                yield return sequenceResult.Current;
        }

        private static Vector3[] SLOT_OFFSETS = new Vector3[]
        {
            new Vector3(-2f, -1f, 1.5f),
            new Vector3(-0.5f, -1f, 1.5f),
            new Vector3(1f, -1f, 1.5f),
            new Vector3(2.4f, -1f, 1.5f)
        };
    }
}