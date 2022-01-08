using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BossManagement
    {
        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void AddSequenceDialogue()
        {
            // I need some more control over dialogue than the simple helper gives me
            DialogueDataUtil.Data.events.Add(new DialogueEvent() {
                id = "Part3AscensionBossRareToken",
                speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, DialogueEvent.Speaker.P03 },
                mainLines = new(new List<DialogueEvent.Line>() {
                    new() { p03Face = P03AnimationController.Face.NoChange, text="Well done. I will give you a [c:bR]rare draft token[c:] for defeating that boss. Don't forget to spend it.", specialInstruction=""}
                })
            });

            DialogueDataUtil.Data.events.Add(new DialogueEvent() {
                id = "Part3AscensionBossDraftToken",
                speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, DialogueEvent.Speaker.P03 },
                mainLines = new(new List<DialogueEvent.Line>() {
                    new() { p03Face = P03AnimationController.Face.NoChange, text="This time I will only give you a [c:bR]regular draft token[c:] for defeating that boss. Don't forget to spend it.", specialInstruction=""}
                })
            });
        }

        [HarmonyPatch(typeof(HoloGameMap), "BossDefeatedSequence")]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03BossDefeatedSequence(IEnumerator sequence, StoryEvent bossDefeatedStoryEvent)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }

            EventManagement.AddCompletedZone(bossDefeatedStoryEvent);

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoBossRares))
            {
                Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN));
                ChallengeActivationUI.TryShowActivation(AscensionChallenge.NoBossRares);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossDraftToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }
            else
            {
                Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossRareToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            yield return FastTravelManagement.ReturnToHomeBase();

            yield break;
        }
    }
}