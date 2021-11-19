using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.StarterDecks;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.StarterDecks.Patchers
{
    public static partial class DeckConstructionPatches
    {
        // This class contains all of the patches that affect the user interface
        // Stuff like dialogue, event sequencing, that sort of thing.

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void MoreAccurateDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("NewRunDealtDeckDefault", "you will select a starting deck on the map");
            DialogueHelper.AddOrModifySimpleDialogEvent("NewRunGetStarterDeck", "choose the tribal leader of your starter deck");
            DialogueHelper.AddOrModifySimpleDialogEvent("NewRunBuildingStarterDeck", "your starter deck is now in place");
        }

        // This next set of hooks prefixed with RunStart_
        // hook into a few different places to adjust the opening
        // sequence of events so that you don't waste your time looking
        // at an empty deck

        public static bool InRunStart = false;

        [HarmonyPatch(typeof(RunIntroSequencer), "RunIntroSequence")]
        [HarmonyPrefix]
        public static void RunStart_SetInRunStartPrefix()
        {
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"We are in the run intro sequence");
            // Tell everything else that we are in the run start
            // But only if the tutorial run is over. Let's make sure
            // We don't screw anything up
            if (StoryEventsData.EventCompleted(StoryEvent.TutorialRunCompleted))
            {
                InRunStart = true;
            }
        }

        [HarmonyPatch(typeof(ViewManager), "SwitchToView")]
        [HarmonyPrefix]
        public static bool RunStart_StopSwitchToDeckView(View view)
        {
            // If the mod is active and we are in run start
            // and you want to switch to the deck viewer,
            // don't do anything!
            if (view == View.MapDeckReview)
            {
                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Someone wants to show the deck. Are we in the intro sequence? {InRunStart}");
                if (InRunStart)
                {
                    InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Stopping the switch to deck view");
                    return false;
                }
            }

            if (InRunStart && view == View.MapDefault)
            {
                // If we're in the run, but it's time to show the map, then we're done with the important
                // bit of the intro sequence
                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"We are no longer in the run intro sequence");
                InRunStart = false;
            }
            return true;
        }

        [HarmonyPatch(typeof(DeckReviewSequencer), "SetDeckReviewShown", new Type[] {typeof(bool), typeof(Transform), typeof(Vector3), typeof(bool)})]
        [HarmonyPrefix]
        public static bool RunStart_CannotShowDeckReview()
        {
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Someone wants to set deck review. Are we in the intro sequence? {InRunStart}");
            // If the mod is active and we are in run start
            // don't do anything!
            if (InRunStart)
            {
                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Stopping the setting of deck review.");
                return false;
            }
            return true;
        }

        public static bool GiveInitialDialogue = false;
        public static int CountOfWaits = 0;

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "CardSelectionSequence")]
        [HarmonyPrefix]
        public static void DeckSelectionSequencer_Initialize()
        {
            // And only do this after the tutorial run
            if (StoryEventsData.EventCompleted(StoryEvent.TutorialRunCompleted))
            {
                // We give this dialog only if the current number of cards
                // in the deck is 0.
                if (SaveManager.SaveFile.CurrentDeck.Cards.Count == 0)
                {
                    GiveInitialDialogue = true;
                    CountOfWaits = 0;
                }
            }
        }

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "CardSelectionSequence")]
        [HarmonyPostfix]
        public static IEnumerator DeckSelectionSequencer_Modify(IEnumerator sequenceEvent)
        {
            // Iterate through the sequence of events and find the right
            // place to inject our dialogue
            while (sequenceEvent.MoveNext())
            {

                // We're going to do this on the FOURTH instance of 'WaitForSeconds'
                if (GiveInitialDialogue && typeof(WaitForSeconds).IsInstanceOfType(sequenceEvent.Current))
                {
                    CountOfWaits += 1;

                    if (CountOfWaits == 4) // On the fourth "waitforseconds," it's time to inject our dialogue
                    {
                        yield return sequenceEvent.Current;
                        yield return (object) Singleton<TextDisplayer>.Instance.PlayDialogueEvent("NewRunGetStarterDeck", TextDisplayer.MessageAdvanceMode.Input);
                        yield return (object) new WaitForSeconds(0.2f);
                    }
                }
                else
                {
                    yield return sequenceEvent.Current;
                }
            }

            // And finally add our final dialogue
            if (GiveInitialDialogue)
            {
                yield return (object) Singleton<TextDisplayer>.Instance.PlayDialogueEvent("NewRunBuildingStarterDeck", TextDisplayer.MessageAdvanceMode.Input);
                GiveInitialDialogue = false;
            }
        }
    }
}