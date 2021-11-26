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
    public class DeathcardHaunt : CurseBase
    {
        public DeathcardHaunt(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) { }

        public override string Description => "You will be haunted by those who have passed on before you. Their deathcards will appear in battles and try to kill you. Your haunt level will increase the more you win.";

        public override string Title => "The Deathly Haunt";

        private Texture2D _iconTexture = AssetHelper.LoadTexture("deathcard_icon");
        public override Texture2D IconTexture => _iconTexture;

        public override void Reset()
        {
            // We don't need to do anything because the only meaningful variable is stored as a runstate variable.
        }

        private static int HauntLevel
        {
            get 
            { 
                return RunStateHelper.GetInt("Curse.BaseHauntLevel")
                + (RunState.Run.survivorsDead ? 4 : 0);
            }
        }

        public static void ResetHaunt(int value=0)
        {
            RunStateHelper.SetValue("Curse.BaseHauntLevel", value.ToString());
        }

        public static void IncreaseHaunt(int by=1)
        {
            RunStateHelper.SetValue("Curse.BaseHauntLevel", (RunStateHelper.GetInt("Curse.BaseHauntLevel") + by).ToString());
        }

        public static bool RollForDeathcard()
        {
            float randomValue = SeededRandom.Value(SaveManager.SaveFile.GetCurrentRandomSeed());
            //return randomValue < (float)HauntLevel / 11f;       
            return true;
        }

        // This handles haunt level management at the cleanup phase of a fight.
        // It increases when you win, and resets to 0 when you lose.
        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        public static void ResetPlayerHauntLevelWhenBattleLost(ref TurnManager __instance)
        {
            if (__instance.PlayerWon)
                IncreaseHaunt();
            else
                ResetHaunt();
        }

        public class IntWrapper 
        {
            public int Value { get; set; }
        }

        private static ConditionalWeakTable<CardInfo, IntWrapper> deathcardAnimationPlayTable = new ConditionalWeakTable<CardInfo, IntWrapper>();
        private static void MarkAsHauntedCard(CardInfo card, int slot)
        {
            deathcardAnimationPlayTable.Add(card, new IntWrapper { Value = slot });
        }

        private static int GetHauntedCardSlot(CardInfo card)
        {
            IntWrapper dummy;
            if (deathcardAnimationPlayTable.TryGetValue(card, out dummy))
                return dummy.Value;
            else
                return -1;
        }

        public static CardInfo GetRandomDeathcard()
        {
            // Build the base card
            int seed = SaveManager.SaveFile.randomSeed;
            List<CardModificationInfo> modList = SaveManager.SaveFile.GetChoosableDeathcardMods();
            CardModificationInfo mod = modList[SeededRandom.Range(0, modList.Count, seed)];
            CardInfo deathcard = CardLoader.CreateDeathCard(mod);

            return deathcard;
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
            // We don't do this to boss battles
            if (nodeData is BossBattleNodeData)
                return;

            InfiniscryptionCursePlugin.Log.LogInfo("Checking to see if we should add a deathcard...");

            // And let's check the haunt
            if (!RollForDeathcard())
                return;

            InfiniscryptionCursePlugin.Log.LogInfo("Adding a deathcard...");

            List<List<CardInfo>> tp = __result.opponentTurnPlan;

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
            MarkAsHauntedCard(deathcard, weakestIndex);

            // And we're done! The weakest card in the ideal turn now has a deathcard insted.
            InfiniscryptionCursePlugin.Log.LogInfo($"Added a deathcard in turn {idealTurn} in slot {weakestIndex}");
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
                "[c:O]\"I have been looking for you\"[c:] said the apparition",
                "[c:O]\"And now you must die!\"[c:]"
            });
        }

        private static PlayableCard _lastCreatedCard;

        [HarmonyPatch(typeof(Opponent), "CreateCard")]
        [HarmonyPostfix]
        public static void SaveLastCreatedCard(ref PlayableCard __result)
        {
            _lastCreatedCard = __result;
        }
        
        [HarmonyPatch(typeof(Opponent), "QueueCard")]
        [HarmonyPostfix]
        public static IEnumerator PlayDeathcardIntro(IEnumerator sequenceEvent, CardInfo cardInfo)
        {
            InfiniscryptionCursePlugin.Log.LogInfo("In QueueCard");

            sequenceEvent.MoveNext();
            yield return sequenceEvent.Current;
            sequenceEvent.MoveNext();

            // Now we check for our custom card
            int customSlot = GetHauntedCardSlot(cardInfo);
            if (customSlot >= 0)
            {
                InfiniscryptionCursePlugin.Log.LogInfo("Playing animation");
                View oldView = ViewManager.Instance.CurrentView;
                ViewManager.Instance.SwitchToView(View.P03Face);
                yield return TextDisplayer.Instance.PlayDialogueEvent("DeathcardArrives", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] {
                    cardInfo.DisplayedNameLocalized
                }, null);
                yield return new WaitForSeconds(0.25f);

                // Figure out where we want the camera to be.
                ViewInfo targetPos = ViewManager.GetViewInfo(View.OpponentQueue);
                Vector3 translationOffset = (targetPos.camPosition + SLOT_OFFSETS[customSlot]) - ViewManager.GetViewInfo(ViewManager.Instance.CurrentView).camPosition;
                Vector3 rotationOffset = targetPos.camRotation - ViewManager.GetViewInfo(ViewManager.Instance.CurrentView).camRotation;

                ViewManager.Instance.OffsetPosition(translationOffset, 0.16f);
                ViewManager.Instance.OffsetRotation(rotationOffset, 0.16f);

                yield return TextDisplayer.Instance.PlayDialogueEvent("DeathcardZoom", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

                yield return new WaitForSeconds(0.5f);
                ViewManager.Instance.SwitchToView(oldView);
            }
        }

        private static Vector3[] SLOT_OFFSETS = new Vector3[]
        {
            new Vector3(-1.5f, -1f, 1.5f),
            new Vector3(-0.5f, -1f, 1.5f),
            new Vector3(0.5f, -1f, 1.5f),
            new Vector3(1.5f, -1f, 1.5f)
        };
    }
}