using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Infiniscryption.Helpers
{
    public static class DialogueHelper
    {
        // Helper functions for dialogue

        public static void AddOrModifySimpleDialogEvent(string eventId, string line, string template = "NewRunDealtDeckDefault")
        {
            string[] lines = { line };
            AddOrModifySimpleDialogEvent(eventId, lines, null, template);
        }

        private static void SyncLineCollection(List<DialogueEvent.Line> curLines, string[] newLines)
        {
            // Delete unnecessary lines
            while (curLines.Count > newLines.Length)
                curLines.RemoveAt(curLines.Count - 1);

            // Modify all existing lines of dialogue in place
            for (int i = 0; i < curLines.Count; i++)
                curLines[i].text = newLines[i];

            // Clone the first line, modify it, and add to the end for any additional lines
            for (int i = curLines.Count; i < newLines.Length; i++)
            {
                DialogueEvent.Line newLine = CloneLine(curLines[0]);
                newLine.text = newLines[i];
                curLines.Add(newLine);
            }
        }

        public static void AddOrModifySimpleDialogEvent(string eventId, string[] lines, string[][] repeatLines=null, string template = "NewRunDealtDeckDefault")
        {
            // Get the event from the database
            bool addEvent = false;
            DialogueEvent dialogue = DialogueDataUtil.Data.GetEvent(eventId);

            if (dialogue == null) // This event doesn't exist, which means we need to create it
            {
                addEvent = true;
                dialogue = CloneDialogueEvent(DialogueDataUtil.Data.GetEvent(template), eventId);

                // Remove excess lines
                while (dialogue.mainLines.lines.Count > lines.Length)
                {
                    dialogue.mainLines.lines.RemoveAt(lines.Length);
                }
            } 

            // Sync the main lines
            SyncLineCollection(dialogue.mainLines.lines, lines);

            // Sync the repeat lines
            if (repeatLines == null)
            {
                dialogue.repeatLines.Clear();
            } else {

                // Delete unnecessary
                while (dialogue.repeatLines.Count > repeatLines.Length)
                    dialogue.repeatLines.RemoveAt(dialogue.repeatLines.Count - 1);

                // Modify all existing lines of dialogue in place
                for (int i = 0; i < dialogue.repeatLines.Count; i++)
                    SyncLineCollection(dialogue.repeatLines[i].lines, repeatLines[i]);
            }

            if (addEvent)
                DialogueDataUtil.Data.events.Add(dialogue);
        }

        public static DialogueEvent.Line CloneLine(DialogueEvent.Line line)
        {
            return new DialogueEvent.Line {
                p03Face = line.p03Face,
                emotion = line.emotion,
                letterAnimation = line.letterAnimation,
                speakerIndex = line.speakerIndex,
                text = line.text,
                specialInstruction = line.specialInstruction,
                storyCondition = line.storyCondition,
                storyConditionMustBeMet = line.storyConditionMustBeMet
            };
        }

        public static DialogueEvent CloneDialogueEvent(DialogueEvent dialogueEvent, string newId, bool includeRepeat=false)
        {
            DialogueEvent clonedEvent = new DialogueEvent {
                id = newId,
                groupId = dialogueEvent.groupId,
                mainLines = new DialogueEvent.LineSet(),
                speakers = new List<DialogueEvent.Speaker>(),
                repeatLines = new List<DialogueEvent.LineSet>()
            };

            foreach (var line in dialogueEvent.mainLines.lines)
            {
                clonedEvent.mainLines.lines.Add(CloneLine(line));
            }

            if (includeRepeat)
            {
                foreach (var lineSet in dialogueEvent.repeatLines)
                {
                    DialogueEvent.LineSet newSet = new DialogueEvent.LineSet();
                    foreach (var line in lineSet.lines)
                    {
                        newSet.lines.Add(CloneLine(line));
                    }
                    clonedEvent.repeatLines.Add(newSet);
                }
            }

            foreach (var speaker in dialogueEvent.speakers)
            {
                clonedEvent.speakers.Add(speaker);
            }

            return clonedEvent;
        }
    }
}