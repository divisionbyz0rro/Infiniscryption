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
using Infiniscryption.StarterDecks.Helpers;
using Infiniscryption.Core.Helpers;
using TMPro;
using UnityEngine.UI;

namespace Infiniscryption.StarterDecks.Patchers
{
    public static partial class MetaCurrencyPatches
    {
        // This class has all the logic for gaining teeth.
        // You gain teeth only when you die. All your leftover teeth,
        // plus some from your mouth.

        public static bool PlayerHasSeenTeethExtraction
        {
            get { return SaveGameHelper.GetBool("SeenTeethExtraction"); }
            set { SaveGameHelper.SetValue("SeenTeethExtraction", value.ToString()); }
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void LeshyDeathDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("LeshyTakeYourTeeth", new string[] {"you have so many teeth left in your skull", "it would be a shame to let them go to waste" });
            DialogueHelper.AddOrModifySimpleDialogEvent("TeethSavedForNextRun", new string[] {"the next to come through here will make use of these" });
        }

        [HarmonyPatch(typeof(Part1GameFlowManager), "KillPlayerSequence")]
        [HarmonyPostfix]
        public static IEnumerator KillPlayerSequence_Modify(IEnumerator sequenceEvent)
        {
            // Go ahead and update the excess teeth
            if (RunState.Run.playerLives <= 0)
            {
                ExcessTeeth += RunState.Run.currency + InfiniscryptionStarterDecksPlugin.CostPerLevel;
            }

            while (sequenceEvent.MoveNext())
            {
                if (RunState.Run.playerLives <= 0)
                {

                    // Here, we're waiting for one specific combination of factors
                    if (ViewManager.Instance.CurrentView == View.Default &&
                        //PlayerHand.Instance.gameObject.activeSelf &&
                        sequenceEvent.Current is WaitForSeconds &&
                        (sequenceEvent.Current as WaitForSeconds).m_Seconds == 2.75f)
                    {
                        // Phew!
                        // We're at the part where Leshy has just reached out to us, as if to grab us.

                        // Let the timer go - it's the time it takes for his arms to reach out.
                        yield return sequenceEvent.Current; 

                        // Let's take those lovely teeth!
                        if (!PlayerHasSeenTeethExtraction)
                        {
                            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("LeshyTakeYourTeeth", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                            PlayerHasSeenTeethExtraction = true;
                        }

                        // This all comes from the pliers
                        Singleton<UIManager>.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0.5f, 0.75f);
                        yield return new WaitForSeconds(0.5f);

                        Singleton<FirstPersonController>.Instance.AnimController.PlayOneShotAnimation("PliersAnimation", null);
                        AudioController.Instance.PlaySound2D("whoosh2", MixerGroup.None, 1f, 0.4f, null, null, null, null, false);
                        yield return new WaitForSeconds(0.35f);
                        AudioController.Instance.PlaySound2D("consumable_pliers_use", MixerGroup.None, 1f, 0f, null, null, null, null, false);
                        yield return new WaitForSeconds(0.75f);
                        AudioController.Instance.FadeBGMMixerParam("BGMLowpassFreq", 50f, 0.1f);
                        Singleton<UIManager>.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0f, 0.025f);
                        Singleton<CameraEffects>.Instance.Shake(0.1f, 0.5f);
                        Singleton<UIManager>.Instance.Effects.GetEffect<ScreenColorEffect>().SetColor(GameColors.Instance.red);
                        Singleton<UIManager>.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(1f, 50f);
                        Singleton<CameraEffects>.Instance.TweenBlur(4f, 0.03f);
                        yield return new WaitForSeconds(0.03f);
                        Singleton<UIManager>.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(0f, 0.2f);
                        Singleton<CameraEffects>.Instance.TweenBlur(0f, 4f);
                        yield return new WaitForSeconds(1f);
                    } else {
                        yield return sequenceEvent.Current;
                    }
                } else {
                    yield return sequenceEvent.Current;
                }
            }
        }
    }
}