using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class DialogueManagement
    {
        private static Emotion FaceEmotion(this P03AnimationController.Face face)
        {
            if (face == P03AnimationController.Face.Angry)
                return Emotion.Anger;
            if (face == P03AnimationController.Face.Thinking)
                return Emotion.Curious;
            if (face == P03AnimationController.Face.Happy)
                return Emotion.Laughter;
            return Emotion.Neutral;
        }

        private static P03AnimationController.Face ParseFace(this string face)
        {
            return (P03AnimationController.Face)Enum.Parse(typeof(P03AnimationController.Face), (String.IsNullOrEmpty(face) ? "NoChange" : face));
        }

        private static void AddDialogue(string id, List<string> lines, List<string> faces, List<string> dialogueWavies)
        {
            //P03Plugin.Log.LogInfo($"Creating dialogue {id}, {string.Join(",", lines)}");

            bool leshy = faces.Any(s => s.ToLowerInvariant() == "leshy");

            DialogueEvent.Speaker speaker = DialogueEvent.Speaker.P03;
            if (leshy)
                speaker = DialogueEvent.Speaker.Leshy;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("telegrapher")))
                speaker = DialogueEvent.Speaker.P03Telegrapher;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("archivist")))
                speaker = DialogueEvent.Speaker.P03Archivist;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("photographer")))
                speaker = DialogueEvent.Speaker.P03Photographer;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("canvas")))
                speaker = DialogueEvent.Speaker.P03Canvas;

            if (string.IsNullOrEmpty(id))
                return;

            DialogueDataUtil.Data.events.Add(new DialogueEvent() {
                id = id,
                speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, speaker },
                mainLines = new(faces.Zip(lines, (face, line) => new DialogueEvent.Line() {
                    text = line,
                    specialInstruction = "",
                    p03Face = leshy ? (P03AnimationController.Face)0 : ParseFace(face),
                    speakerIndex = 1,
                    emotion = leshy ? Emotion.Neutral : ParseFace(face).FaceEmotion()
                })
                .Zip(dialogueWavies, delegate(DialogueEvent.Line line, string wavy) {
                    if (!string.IsNullOrEmpty(wavy) && wavy.ToLowerInvariant() == "y")
                        line.letterAnimation = TextDisplayer.LetterAnimation.WavyJitter;
                    return line;
                }).ToList())
            });
        }

        private static List<string> SplitColumn(string col, char sep = ',', char quote = '"')
        {
            bool isQuoted = false;
            List<string> retval = new();
            string cur = string.Empty;
            foreach (char c in col)
            {
                if (c == sep && !isQuoted)
                {
                    retval.Add(cur);
                    cur = string.Empty;
                    continue;
                }

                if (c == quote && cur == string.Empty)
                {
                    isQuoted = true;
                    continue;
                }

                if (c == quote && cur != string.Empty && isQuoted)
                {
                    isQuoted = false;
                    continue;
                }

                cur += c;
            }
            retval.Add(cur);
            return retval;
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void AddSequenceDialogue()
        {
            string database = AssetHelper.GetResourceString("dialogue_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string dialogueId = string.Empty;
            List<string> dialogueLines = new();
            List<string> dialogueWavies = new ();
            List<string> dialogueFaces = new();
            foreach(string line in lines.Skip(1))
            {
                List<string> cols = SplitColumn(line);
                
                if (string.IsNullOrEmpty(cols[0]))
                {
                    dialogueLines.Add(cols[3]);
                    dialogueWavies.Add(cols[2]);
                    dialogueFaces.Add(cols[1]);
                    continue;
                }

                AddDialogue(dialogueId, dialogueLines, dialogueFaces, dialogueWavies);

                dialogueId = cols[0];
                dialogueLines = new() { cols[3] };
                dialogueWavies.Add(cols[2]);
                dialogueFaces = new() { cols[1] };
            }

            AddDialogue(dialogueId, dialogueLines, dialogueFaces, dialogueWavies);
        }
    }
}