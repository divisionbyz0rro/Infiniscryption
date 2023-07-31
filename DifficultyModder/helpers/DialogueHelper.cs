using System.Collections.Generic;
using System;
using InscryptionAPI.Dialogue;
using System.Linq;

namespace Infiniscryption.Curses.Helpers
{
    internal static class DialogueHelper
    {
        private static Tuple<List<InscryptionAPI.Dialogue.CustomLine>, List<List<InscryptionAPI.Dialogue.CustomLine>>> LinesHelper(params string[] lines)
        {
            List<InscryptionAPI.Dialogue.CustomLine> main = new() { new () { text = lines [0] } };
            List<List<InscryptionAPI.Dialogue.CustomLine>> extra = lines.Skip(1).Select(l => new List<InscryptionAPI.Dialogue.CustomLine>() { new () { text = l }}).ToList();
            return new(main, extra);
        }

        internal static void GenerateVeryLargeDialogue(string dialogueId, DialogueEvent.Speaker speaker, params string[][] lines)
        {
            List<InscryptionAPI.Dialogue.CustomLine> main = lines[0].Select(l => new InscryptionAPI.Dialogue.CustomLine() { text = l }).ToList();
            List<List<InscryptionAPI.Dialogue.CustomLine>> extra = lines.Skip(1).Select(ls => ls.Select(l => new InscryptionAPI.Dialogue.CustomLine() { text = l}).ToList()).ToList();
            DialogueManager.Add(CursePlugin.PluginGuid, DialogueManager.GenerateEvent(
                CursePlugin.PluginGuid, dialogueId, main, extra, defaultSpeaker: speaker
            ));
        }

        internal static void GenerateVeryLargeDialogue(string dialogueId, params string[][] lines)
        {
            GenerateVeryLargeDialogue(dialogueId, DialogueEvent.Speaker.Leshy, lines);
        }

        internal static void GenerateLargeDialogue(string dialogueId, DialogueEvent.Speaker speaker, params string[] lines)
        {
            List<InscryptionAPI.Dialogue.CustomLine> main = lines.Select(l => new InscryptionAPI.Dialogue.CustomLine() { text = l }).ToList();
            DialogueManager.Add(CursePlugin.PluginGuid, DialogueManager.GenerateEvent(
                CursePlugin.PluginGuid, dialogueId, main, defaultSpeaker: speaker
            ));
        }

        internal static void GenerateLargeDialogue(string dialogueId, params string[] lines)
        {
            GenerateLargeDialogue(dialogueId, DialogueEvent.Speaker.Leshy, lines);
        }

        internal static void GenerateDialogue(string dialogueId, DialogueEvent.Speaker speaker, params string[] lines)
        {
            var linesData = LinesHelper(lines);
            DialogueManager.Add(CursePlugin.PluginGuid, DialogueManager.GenerateEvent(
                CursePlugin.PluginGuid, dialogueId, linesData.Item1, linesData.Item2, defaultSpeaker: speaker
            ));
        }

        internal static void GenerateDialogue(string dialogueId, params string[] lines)
        {
            GenerateDialogue(dialogueId, DialogueEvent.Speaker.Leshy, lines);
        }
    }
}