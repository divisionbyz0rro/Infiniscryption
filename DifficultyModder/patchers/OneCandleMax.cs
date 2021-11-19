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
using Infiniscryption.Core.Helpers;
using UnityEngine.UI;
using Infiniscryption.DifficultyMod.Sequences;

namespace Infiniscryption.DifficultyMod.Patchers
{
    public static class OneCandleMax
    {
        static OneCandleMax()
        {
            ToggleableDifficultyManager.Register(ResetState);
        }

        // This is a toggleable difficulty setting that forces the maximum number
        // of candles to one, regardless of how many you've unlocked
        public static bool Active 
        {
            get { return SaveGameHelper.GetBool("Difficulty.OneCandle"); }
            set { SaveGameHelper.SetValue("Difficulty.OneCandle", value.ToString()); }
        }

        // Resets the game state based on this value
        // This gets called when the user steps away from the UI before the run starts
        public static void ResetState()
        {
            RunState.Run.maxPlayerLives = Active ? 1 : StoryEventsData.EventCompleted(StoryEvent.CandleArmFound) ? 3 : 2;
            RunState.Run.playerLives = RunState.Run.maxPlayerLives;
        }

        [HarmonyPatch(typeof(Part1BossOpponent), "BossDefeatedSequence")]
        [HarmonyPostfix]
        public static IEnumerator BossCandleRenewSequence(IEnumerator sequenceEvent)
        {
            // If not active, just return everything as-is
            // We aren't doing anything special here.
            if (!Active)
            {
                while (sequenceEvent.MoveNext())
                    yield return sequenceEvent.Current;
            }
            else
            {
                while (sequenceEvent.MoveNext())
                {
                    // We're looking for the delay right before the part where the boss
                    // gives you back more lives
                    if (sequenceEvent.Current is WaitForSeconds &&
                        (sequenceEvent.Current as WaitForSeconds).m_Seconds == 0.8f)
                    {
                        // We just aren't going to do any of this
                        sequenceEvent.MoveNext(); // Now it's dialogue
                        sequenceEvent.MoveNext(); // Now it's another wait for seconds
                        sequenceEvent.MoveNext(); // Now it's the replenish flames seequence
                    }
                    else
                    {
                        yield return sequenceEvent.Current;
                    }
                }
            }
        }
    }
}