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
using Infiniscryption.Curses.Sequences;
using Infiniscryption.Curses.Helpers;

namespace Infiniscryption.Curses.Patchers
{
    public class OneCandleMax : CurseBase
    {
        public override string Description => "Limits you to only a single candle flame for the duration of your run";
        public override string Title => "The Lone Candle";
        
        Texture2D _iconTexture = AssetHelper.LoadTexture("candle_icon");
        public override Texture2D IconTexture => _iconTexture;

        public OneCandleMax(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) {}


        // Resets the game state based on this value
        // This gets called when the user steps away from the UI before the run starts
        public override void Reset()
        {
            RunState.Run.maxPlayerLives = Active ? 1 : StoryEventsData.EventCompleted(StoryEvent.CandleArmFound) ? 3 : 2;
            RunState.Run.playerLives = RunState.Run.maxPlayerLives;

            if (CandleHolder.Instance != null)
                CandleHolder.Instance.UpdateArmsAndFlames();
        }

        [HarmonyPatch(typeof(CandleHolder), "ReplenishFlamesSequence")]
        [HarmonyPostfix]
        public static IEnumerator PreventReplenishFlamesSequence(IEnumerator sequenceEvent)
        {
            if (!CurseManager.IsActive<OneCandleMax>())
            {
                while (sequenceEvent.MoveNext())
                    yield return sequenceEvent.Current;
            } else {
                // Well guess what! There's no need to do anything. You're either dead
                // in which case we are never replenishing flames, or you've survived
                // in which case we are never replenishing flames.
                yield break;
            }
        }

        [HarmonyPatch(typeof(Part1BossOpponent), "BossDefeatedSequence")]
        [HarmonyPostfix]
        public static IEnumerator BossCandleRenewSequence(IEnumerator sequenceEvent)
        {
            // If not active, just return everything as-is
            // We aren't doing anything special here.
            if (!CurseManager.IsActive<OneCandleMax>())
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