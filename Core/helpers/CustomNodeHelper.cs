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

namespace Infiniscryption.Core.Helpers
{
    public static class CustomNodeHelper
    {
        private static bool Initialized = false;

        public static void Initialize(Harmony harmony)
        {
            Initialized = true;
            harmony.PatchAll(typeof(CustomNodeHelper));
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

                bool loadedTexture = false;
                for (int i = 0; i < sprite.textureFrames.Count; i++)
                {
                    if (sprite.textureFrames[i].name != $"Infiniscryption_{spriteCode}_{i+1}")
                    {
                        sprite.textureFrames[i] = AssetHelper.LoadTexture($"{spriteCode}_{i+1}");
                        loadedTexture = true;
                    }
                }

                if (loadedTexture)
                    sprite.IterateFrame();
            }
        }
    }
}