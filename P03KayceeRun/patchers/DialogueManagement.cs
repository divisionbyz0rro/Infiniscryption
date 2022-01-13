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
        private static void AddDialogue(string id, List<string> lines, List<string> faces)
        {
            P03Plugin.Log.LogInfo($"Creating dialogue {id}, {string.Join(",", lines)}");

            if (string.IsNullOrEmpty(id))
                return;

            DialogueDataUtil.Data.events.Add(new DialogueEvent() {
                id = id,
                speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, DialogueEvent.Speaker.P03 },
                mainLines = new(faces.Zip(lines, (face, line) => new DialogueEvent.Line() {
                    text = line,
                    specialInstruction = "",
                    p03Face = (P03AnimationController.Face)Enum.Parse(typeof(P03AnimationController.Face), (String.IsNullOrEmpty(face) ? "NoChange" : face))
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
            List<string> dialogueFaces = new();
            foreach(string line in lines.Skip(1))
            {
                List<string> cols = SplitColumn(line);
                
                if (string.IsNullOrEmpty(cols[0]))
                {
                    dialogueLines.Add(cols[2]);
                    dialogueFaces.Add(cols[1]);
                    continue;
                }

                AddDialogue(dialogueId, dialogueLines, dialogueFaces);

                dialogueId = cols[0];
                dialogueLines = new() { cols[2] };
                dialogueFaces = new() { cols[1] };
            }

            AddDialogue(dialogueId, dialogueLines, dialogueFaces);
        }
    }
}