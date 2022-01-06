using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Saves;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
	[HarmonyPatch]
	public class AscensionFinaleSequencer : HoloAreaSpecialSequencer
	{
        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void AddSequenceDialogue()
        {
            // I need some more control over dialogue than the simple helper gives me
            DialogueDataUtil.Data.events.Add(new DialogueEvent() {
                id = "Part3AscensionFinale",
                speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.P03 },
                mainLines = new(new List<DialogueEvent.Line>() {
                    new() { p03Face = P03AnimationController.Face.Bored, text="Well that was neat. I see you've made it all the way to the end", specialInstruction=""},
                    new() { p03Face = P03AnimationController.Face.Bored, text="I bet you think there's going to be some sort of final boss fight", specialInstruction="" },
                    new() { p03Face = P03AnimationController.Face.Angry, text="But that stupid [c:bR]modder[c:] hasn't gotten around to it yet!", specialInstruction="" },
                    new() { p03Face = P03AnimationController.Face.Angry, text="So instead, I'm just going to have to do this...", specialInstruction="" },
                })
            });
        }
		
		public override IEnumerator PreEnteredSequence()
		{
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = true;
            ViewManager.Instance.SwitchToView(View.P03Face, false, true);
            yield return new WaitForSeconds(0.5f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionFinale", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.25f);
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            EventManagement.FinishAscension(true);
			yield break;
		}
	}
}
