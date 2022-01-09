using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using APIPlugin;
using InscryptionAPI.Guid;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public static class DialogueManagement
    {
        private static void UpdateExistingCard(string name, string textureKey, string pixelTextureKey, string regionCode, string decalTextureKey)
        {
            if (string.IsNullOrEmpty(name))
                return;

            CustomCard customCard = new CustomCard(name);
            CardInfo card = null;

            if (!string.IsNullOrEmpty(textureKey))
                customCard.tex = AssetHelper.LoadTexture(textureKey);

            if (!string.IsNullOrEmpty(pixelTextureKey))
                customCard.pixelTex = AssetHelper.LoadTexture(pixelTextureKey);

            if (!string.IsNullOrEmpty(regionCode))
            {
                card = card ?? CardLoader.GetCardByName(name);
                List<CardMetaCategory> cats = card.metaCategories;
                cats.Add((CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(InfiniscryptionP03Plugin.PluginGuid, regionCode));
                customCard.metaCategories = cats;
            }

            if (!string.IsNullOrEmpty(decalTextureKey))
                customCard.decals = new () { AssetHelper.LoadTexture(decalTextureKey) };
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Load the custom cards from the CSV database
            string database = AssetHelper.GetResourceString("card_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines.Skip(1))
            {
                string[] cols = line.Split(new char[] { ',' } , StringSplitOptions.None);
                //InfiniscryptionP03Plugin.Log.LogInfo($"I see line {string.Join(";", cols)}");
                UpdateExistingCard(cols[0], cols[1], cols[2], cols[3], cols[4]);
            }
        }

        private static void AddDialogue(string id, List<string> faces, List<string> lines)
        {
            if (string.IsNullOrEmpty(id))
                return;

            DialogueDataUtil.Data.events.Add(new DialogueEvent() {
                id = id,
                speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, DialogueEvent.Speaker.P03 },
                mainLines = new(faces.Zip(lines, (face, line) => new DialogueEvent.Line() {
                    text = line,
                    specialInstruction = "",
                    p03Face = (P03AnimationController.Face)Enum.Parse(typeof(P03AnimationController.Face), face)
                }).ToList())
            });
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
                string[] cols = line.Split(new char[] { ',' } , StringSplitOptions.None);
                
                if (string.IsNullOrEmpty(cols[0]))
                {
                    dialogueLines.Add(cols[2]);
                    dialogueFaces.Add(cols[1]);
                    continue;
                }

                AddDialogue(dialogueId, dialogueLines, dialogueFaces);

                dialogueId = cols[0];
                dialogueLines.Add(cols[2]);
                dialogueFaces.Add(cols[1]);
            }

            AddDialogue(dialogueId, dialogueLines, dialogueFaces);
        }
    }
}