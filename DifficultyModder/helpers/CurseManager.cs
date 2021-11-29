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
using UnityEngine.UI;
using Infiniscryption.Core.Helpers;
using System.IO;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using Infiniscryption.Curses.Sequences;

namespace Infiniscryption.Curses.Helpers
{
    public static class CurseManager
    {
        private static Dictionary<string, CurseBase> CurseMods = new Dictionary<string, CurseBase>();

        public enum BindsTo
        {
            Configuration = 0,
            RunSetting = 1
        }

        public static void Register<T>(Harmony harmony, ConfigFile config, BindsTo behavior = BindsTo.Configuration) where T : CurseBase
        {
            CurseBase.SetActiveDelegate setActive;
            CurseBase.GetActiveDelegate getActive;

            if (!config.Bind("InfiniscryptionCurses", typeof(T).Name, true, new BepInEx.Configuration.ConfigDescription("Should this difficulty mod be active?")).Value)
            {
                return; // Don't bind or store - config said it was inactive
            }

            if (behavior == BindsTo.RunSetting)
            {
                setActive = delegate(bool active) { SaveGameHelper.SetValue($"Curse.{typeof(T).Name}", active.ToString()); }; 
                getActive = delegate() { return SaveGameHelper.GetBool($"Curse.{typeof(T).Name}"); };
            }
            else
            {
                setActive = delegate(bool active) {  };
                getActive = delegate() { return true; }; // Globally on - not run-by-run
            }

            T instance = (T)Activator.CreateInstance(typeof(T), new object[] { typeof(T).Name, getActive, setActive} );
            harmony.PatchAll(typeof(T));
            CurseMods.Add(instance.ID, instance);
        }

        public static CurseBase GetInstance<T>() where T : CurseBase
        {
            if (CurseMods.ContainsKey(typeof(T).Name))
                return CurseMods[typeof(T).Name];

            return null;
        }

        public static bool IsActive<T>() where T : CurseBase
        {
            CurseBase mod = GetInstance<T>();
            return (mod == null) ? false : mod.Active;
        }

        internal static void SetActive<T>(bool value) where T : CurseBase
        {
            CurseBase mod = GetInstance<T>();
            if (mod != null)
                mod.Active = value;
        }

        public static List<CurseBase> GetAllCurses() => CurseMods.Values.ToList();

        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        public static void ResetAll()
        {
            foreach (CurseBase mod in CurseMods.Values)
            {
                mod.Reset();
            }
        }

        [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData")]
        [HarmonyPostfix]
        public static void AddCursesToRulebook(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
        {
            PageRangeInfo boonPages = __instance.pageRanges.Find(pri => pri.type == PageRangeType.Boons);
            string sectionText = Localization.Translate("APPENDIX XII, SUBSECTION X - CURSES {0}");

            int curseCount = 1;
            foreach (var key in CurseMods.Keys)
            {
                // Construct a page, add it to the boons list
                RuleBookPageInfo newPage = new RuleBookPageInfo();
                newPage.pagePrefab = boonPages.rangePrefab;
                newPage.headerText = string.Format(sectionText, curseCount);
                newPage.pageId = key;
                newPage.boon = BoonData.Type.None;

                __result.Add(newPage);

                //lastBoon += 1;
                curseCount += 1;
            }
        }

        private static FieldInfo pageInfoPageId = AccessTools.Field(typeof(RuleBookPageInfo), "pageId");

        [HarmonyPatch(typeof(PageContentLoader), "LoadPage")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PageTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // With this patch in place, we can properly patch boonpage.fillpage
            // It makes PageContentLoader.LoadPage pass both the boon enum and the boon page id
            // to the boon page renderer.
            // This matters because our custom boons have a boon enum value of 'None'

            bool inBoonPageBlock = false;
            bool hasLoadedArraySize = false;
            bool injected = false;
            foreach (var instruction in instructions)
            {
                // Okay, we need to find the part where the loadpage is trying to set up
                // boon pages
                // We need it to also send the page id as part of the call

                // We're looking for an 'isinst' of boonpage
                if (!inBoonPageBlock && instruction.Is(OpCodes.Isinst, typeof(BoonPage)))
                {
                    // Okay we're in 
                    inBoonPageBlock = true;
                }

                // Now we're waiting for the ldloc 
                if (inBoonPageBlock && !hasLoadedArraySize && instruction.opcode == OpCodes.Ldc_I4_1)
                {
                    // We need to modify the code instruction.
                    // The length of the object array will be 2, not one
                    instruction.opcode = OpCodes.Ldc_I4_2;
                    hasLoadedArraySize = true;
                }

                // Now we're waiting to inject our new set of instructions
                if (inBoonPageBlock && hasLoadedArraySize && !injected &&
                    instruction.opcode == OpCodes.Stelem_Ref)
                {
                    // Here we go!
                    // First, go ahead and send back this instruction
                    yield return instruction;

                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1); // Pretty sure this is the index into the array
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // Load argument 1
                    yield return new CodeInstruction(OpCodes.Ldfld, pageInfoPageId); // Get the pageid field
                    yield return new CodeInstruction(OpCodes.Stelem_Ref);

                    injected = true;
                    continue;
                }

                yield return instruction;
            }
        }

        [HarmonyPatch(typeof(BoonPage), "FillPage")]
        [HarmonyPrefix]
        public static bool FillPageWithCurses(string headerText, ref BoonPage __instance, params object[] otherArgs)
		{
            if (CurseMods.ContainsKey(otherArgs[1].ToString()))
            {
                CurseBase mod = CurseMods[otherArgs[1].ToString()];

                Traverse traverse = Traverse.Create(__instance);

                if (traverse.Field("headerTextMesh") != null)
                {
                    (traverse.Field("headerTextMesh").GetValue<TextMeshPro>()).text = headerText;
                }
                (traverse.Field("iconRenderer").GetValue<Renderer>()).material.mainTexture = mod.IconTexture;
                (traverse.Field("iconRenderer2").GetValue<Renderer>()).material.mainTexture = mod.IconTexture;
                (traverse.Field("nameTextMesh").GetValue<TextMeshPro>()).text = Localization.Translate($"Curse Of {mod.Title}");
                (traverse.Field("descriptionTextMesh").GetValue<TextMeshPro>()).text = DialogueParser.GetUnformattedMessage(Localization.Translate(mod.Description), "[", "]");
                return false;
            }
            return true;
		}

        public static bool HasSeenCurseSelectBefore
        {
            // Tracks if the user has seen the curse selection screen before
            get { return SaveGameHelper.GetBool("HasSeenCurses"); }
            set { SaveGameHelper.SetValue("HasSeenCurses", value.ToString()); }
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void CurseDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("CurseIntroIntro", new string []
            {
                "your journey begins as you enter a dark and foreboding forest",
                "suddenly, an old woman appears before you"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("SummonCurses", new string []
            {
                "she opens her hand and summons forth [c:bR]curse cards[c:]"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("WhatAreCurses", new string[] {
                "curses increase the [c:bR]difficulty[c:] of your run",
                "do not make this decision lightly"
            });
            DialogueHelper.AddOrModifySimpleDialogEvent("HowToSelect", "each [c:bR]curse card[c:] you leave face up will be active until your run concludes");
            DialogueHelper.AddOrModifySimpleDialogEvent("CursesSelect", "the woman silently picks up her cards and walks alway", TextDisplayer.LetterAnimation.WavyJitter, Emotion.Laughter);
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool SendToHallOfCurses(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            // This sends the player to the upgrade shop if the triggering node is SpendExcessTeeth
            if (nodeData is CurseNodeData)
			{
                if (__instance.gameObject.GetComponent<CurseNodeSequencer>() == null)
                {
                    InfiniscryptionCursePlugin.Log.LogInfo($"Attaching hall of curses sequencer to parent");
                    __instance.gameObject.AddComponent<CurseNodeSequencer>();
                }

                InfiniscryptionCursePlugin.Log.LogInfo($"Starting the hall of curses");
				__instance.StartCoroutine(__instance.gameObject.GetComponent<CurseNodeSequencer>().PlayCurseShop());
				return false; // This prevents the rest of the thing from running.
			}
            return true; // This makes the rest of the thing run
        }

        [HarmonyPatch(typeof(PaperGameMap), "TryInitializeMapData")]
        [HarmonyPrefix]
        [HarmonyAfter(new string[] { "porta.inscryption.traderstart", "zorro.inscryption.infiniscryption.starterdecks" })]
        [HarmonyBefore(new string[] { "cyantist.inscryption.extendedmap" })]
        public static void StartWithCurseSelection(ref PaperGameMap __instance)
        {
            // This patch ensures that the first node of the map is always
            // a tribe selection node. It also sets up the cards that you can
            // select from to start your deck

            // Be a good citizen - if you haven't completed the tutorial, this should have no effect:
            if (StoryEventsData.EventCompleted(StoryEvent.TutorialRunCompleted))
            {
                InfiniscryptionCursePlugin.Log.LogInfo($"Testing to add curse node");
                if (RunState.Run.map == null) // Only do this when the map is empty
                {
                    InfiniscryptionCursePlugin.Log.LogInfo($"Map is null - adding curse node");
                    // Let's start by seeing if we have predefined nodes already
                    // It's unfortunately private
                    Traverse paperMapTraverse = Traverse.Create(__instance);
                    PredefinedNodes predefinedNodes = paperMapTraverse.Method("get_PredefinedNodes").GetValue<PredefinedNodes>();
                    if (predefinedNodes != null)
                    {
                        InfiniscryptionCursePlugin.Log.LogInfo($"Inserting the curse node at start");
                        predefinedNodes.nodeRows.Insert(1, new List<NodeData>() { new CurseNodeData() });
                    } else {
                        InfiniscryptionCursePlugin.Log.LogInfo($"Adding the curse node to start");
                        PredefinedNodes nodes = ScriptableObject.CreateInstance<PredefinedNodes>();
                        nodes.nodeRows.Add(new List<NodeData>() { new NodeData() });
                        nodes.nodeRows.Add(new List<NodeData>() { new CurseNodeData() });
                        __instance.PredefinedNodes = nodes;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapDataReader), "GetPrefabPath")]
        [HarmonyPostfix]
        public static void TrimPrefabPath(ref string __result)
        {
            // So, for some reason, the map data reader doesn't just
            // straight up read the property of the map node.
            // It passes through here first.
            // That's convenient! We will trim our extra instructions off here
            // Then we'll read that information off later
            if (__result.Contains('@'))
                __result = __result.Substring(0, __result.IndexOf('@')); // Get rid of everything after the @
        }

        [HarmonyPatch(typeof(MapDataReader), "SpawnAndPlaceElement")]
        [HarmonyPostfix]
        public static void TransformMapNode(ref GameObject __result, MapElementData data)
        {
            // First, let's see if we need to do anything
            if (data.PrefabPath.Contains('@'))
            {   
                string spriteCode = data.PrefabPath.Substring(data.PrefabPath.IndexOf('@') + 1);

                // Replace the sprite
                AnimatingSprite sprite = __result.GetComponentInChildren<AnimatingSprite>();

                for (int i = 0; i < sprite.textureFrames.Count; i++)
                    sprite.textureFrames[i] = AssetHelper.LoadTexture($"{spriteCode}_{i+1}");

                sprite.IterateFrame();
            }
        }
    }
}