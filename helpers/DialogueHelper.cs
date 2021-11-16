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
            AddOrModifySimpleDialogEvent(eventId, lines, template);
        }

        public static void AddOrModifySimpleDialogEvent(string eventId, string[] lines, string template = "NewRunDealtDeckDefault")
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

            // Modify all existing lines of dialogue in place
            for (int i = 0; i < dialogue.mainLines.lines.Count; i++)
                dialogue.mainLines.lines[i].text = lines[i];

            // Clone the first line, modify it, and add to the end for any additional lines
            for (int i = dialogue.mainLines.lines.Count; i < lines.Length; i++)
            {
                DialogueEvent.Line newLine = CloneLine(dialogue.mainLines.lines[0]);
                newLine.text = lines[i];
                dialogue.mainLines.lines.Add(newLine);
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

        public static DialogueEvent CloneDialogueEvent(DialogueEvent dialogueEvent, string newId)
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

            foreach (var lineSet in dialogueEvent.repeatLines)
            {
                DialogueEvent.LineSet newSet = new DialogueEvent.LineSet();
                foreach (var line in lineSet.lines)
                {
                    newSet.lines.Add(CloneLine(line));
                }
                clonedEvent.repeatLines.Add(newSet);
            }

            foreach (var speaker in dialogueEvent.speakers)
            {
                clonedEvent.speakers.Add(speaker);
            }

            return clonedEvent;
        }
    }
}