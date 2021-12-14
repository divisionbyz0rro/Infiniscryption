using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using UnityEngine.Networking;
using System.Linq;
using Infiniscryption.Core.Components;

namespace Infiniscryption.Core.Helpers
{
    public static class CustomNodeHelper
    {
        // Some of the behavior of this is going to look really, really strange.
        // Remebmer that these helpers are shared across multiple projects, but I have to account
        // for the fact that some players may have mods installed that have different versions of the core
        // project in them. I can't have installing a new mod of mine be a breaking change for an old one.
        // So I use ILRepack to merge the core project into each project.
        // What this means is that each project has its own instance of the core project.
        //
        // When I patch the game code, I have to remember that multiple patches are happening, one for each
        // mod that uses this helper. So each of these methods has to respect the fact that it might be executing
        // outside of the context of the mod that created it.
        //
        // So everything is carefully written to always check the current assembly (like the part that converts a
        // string to a Type; I can't just look at all types currently loaded, because that type could come from
        // a different assembly, and I might not have access to the stuff I need from it).

        private static bool Initialized = false;

        private static Dictionary<string, Texture2D[]> customNodeArt = new Dictionary<string, Texture2D[]>();

        private static ManualLogSource Log;

        public static void Initialize(Harmony harmony, ManualLogSource log)
        {
            Initialized = true;
            Log = log;
            harmony.PatchAll(typeof(CustomNodeHelper));
        }

        public static GenericCustomNodeData GetNodeData<T>(string icon) where T : ICustomNodeSequence
        {
            return new GenericCustomNodeData(typeof(T), icon);
        }

        public static Texture2D[] GetCustomNodeTextures(string icon)
        {
            if (!customNodeArt.ContainsKey(icon))
            {
                Log.LogInfo($"Getting node art for {icon}");
                if (string.IsNullOrEmpty(AssetHelper.FindResourceName($"{icon}_1", "png", Assembly.GetExecutingAssembly())))
                {
                    Log.LogInfo($"Could not find node art for {icon}");
                    customNodeArt.Add(icon, null);
                }
                else
                {
                    Texture2D[] textures = new Texture2D[4];
                    for (int i = 0; i < 4; i++)
                        textures[i] = AssetHelper.LoadTexture($"{icon}_{i+1}");
                    customNodeArt.Add(icon, textures);
                }
            }

            return customNodeArt[icon];
        }

        public static Type GetCustomNodeSequencer(string sequencerGUID)
        {
            Log.LogInfo($"Getting custom node sequencer for {sequencerGUID}");
            Assembly current = Assembly.GetExecutingAssembly();
            Type type = current.GetType(sequencerGUID, false, true);
            Log.LogInfo($"Sequencer is {type} for guid {sequencerGUID} in assembly {current.FullName}");
            return type;
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool CustomNodeGenericSelect(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            // This sends the player to the upgrade shop if the triggering node is SpendExcessTeeth
            Log.LogInfo($"Arrived at node {nodeData} of type {nodeData.GetType()}");
            if (nodeData is GenericCustomNodeData genericNode)
			{
                Type customNodeType = CustomNodeHelper.GetCustomNodeSequencer(genericNode.guid);

                if (customNodeType == null)
                    return true;

                ICustomNodeSequence sequence = __instance.gameObject.GetComponent(customNodeType) as ICustomNodeSequence;
                if (sequence == null)
                {
                    sequence = __instance.gameObject.AddComponent(customNodeType) as ICustomNodeSequence;
                }

				__instance.StartCoroutine(sequence.ExecuteCustomSequence(genericNode));
				return false; // This prevents the rest of the thing from running.
			}
            return true; // This makes the rest of the thing run
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
                Texture2D[] nodeTextures = GetCustomNodeTextures(spriteCode);

                if (nodeTextures == null)
                    return;

                // Replace the sprite
                AnimatingSprite sprite = __result.GetComponentInChildren<AnimatingSprite>();

                bool loadedTexture = false;
                for (int i = 0; i < sprite.textureFrames.Count; i++)
                {
                    if (sprite.textureFrames[i].name != $"Infiniscryption_{spriteCode}_{i+1}")
                    {
                        sprite.textureFrames[i] = nodeTextures[i];
                        loadedTexture = true;
                    }
                }

                if (loadedTexture)
                    sprite.IterateFrame();
            }
        }
    }
}