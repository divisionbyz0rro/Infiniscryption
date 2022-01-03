using HarmonyLib;
using DiskCardGame;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class FastTravelManagement
    {
        [HarmonyPatch(typeof(HoloMapWaypointNode), "OnEnable")]
        [HarmonyPostfix]
        public static void AlwaysDisableHintUI(ref HoloMapWaypointNode __instance)
        {
            if (SaveFile.IsAscension)
                Traverse.Create(__instance).Field("fastTravelHint").GetValue<GameObject>().SetActive(false);
        }

        private static readonly Dictionary<string, int> fastTravelNodes = new()
        {
            { "FastTravelMapNode_Wizard", RunBasedHoloMap.MAGIC },
            { "FastTravelMapNode_Undead", RunBasedHoloMap.UNDEAD },
            { "FastTravelMapNode_Nature", RunBasedHoloMap.NATURE },
            { "FastTravelMapNode_Tech", RunBasedHoloMap.TECH }
        };
        
        [HarmonyPatch(typeof(FastTravelNode), "OnFastTravelActive")]
        [HarmonyPostfix]
        public static void ActivateFastTravelUsingAscensionLogic(ref FastTravelNode __instance)
        {
            if (SaveFile.IsAscension)
                __instance.gameObject.SetActive(fastTravelNodes.Keys.Contains(__instance.gameObject.name) && !P03AscensionSaveData.CompletedZones.Contains(__instance.gameObject.name));
        }

        [HarmonyPatch(typeof(FastTravelNode), "OnCursorSelectEnd")]
        [HarmonyPrefix]
        public static bool FastTravelInAscensionMode(ref FastTravelNode __instance)
        {
            // In ascension mode, fast travel is different
            // We will NOT fast travel to the world owned by the fast travel node
            // Instead, we will dynamically create a world based on that node
            if (SaveFile.IsAscension)
            {
                P03AscensionSaveData.AddVisitedZone(__instance.gameObject.name);

                Traverse nodeTraverse = Traverse.Create(__instance);
                InfiniscryptionP03Plugin.Log.LogInfo($"SetHoveringEffectsShown");
                nodeTraverse.Method("SetHoveringEffectsShown", new Type[] { typeof(bool) }).GetValue(false);
                InfiniscryptionP03Plugin.Log.LogInfo($"OnSelected");
                nodeTraverse.Method("OnSelected").GetValue();
                HoloGameMap.Instance.ToggleFastTravelActive(false, false);
                HoloMapAreaManager.Instance.CurrentArea.OnAreaActive();
                HoloMapAreaManager.Instance.CurrentArea.OnAreaEnabled();

                string worldId = RunBasedHoloMap.GetAscensionWorldID(fastTravelNodes[__instance.gameObject.name]);
                Tuple<int, int> pos = RunBasedHoloMap.GetStartingSpace(fastTravelNodes[__instance.gameObject.name]);
                Part3SaveData.WorldPosition worldPosition = new(worldId, pos.Item1, pos.Item2);

                HoloMapAreaManager.Instance.StartCoroutine(HoloMapAreaManager.Instance.DroneFlyToArea(worldPosition, false));
                Part3SaveData.Data.checkpointPos = worldPosition;

                EventManagement.NumberOfZoneEnemiesKilled = 0;

                return false;
            }

            return true;
        }

        private static bool isDroneFlying = false;
        [HarmonyPatch(typeof(HoloMapAreaManager), "DroneFlyToArea")]
        [HarmonyPrefix]
        public static void SetDroneFlying()
        {
            isDroneFlying = true;
        }

        [HarmonyPatch(typeof(HoloMapAreaManager), "DroneFlyToArea")]
        [HarmonyPostfix]
        public static void SetDroneNotFlying()
        {
            isDroneFlying = false;
        }

        [HarmonyPatch(typeof(HoloGameMap), "UpdateColors")]
        [HarmonyPrefix]
        public static bool ManuallySetMapColorsIfDroneFlying(ref HoloGameMap __instance)
        {
            if (SaveFile.IsAscension && isDroneFlying)
            {
                HoloMapArea currentArea = HoloMapAreaManager.Instance.CurrentArea;
                Traverse mapTrav = new Traverse(__instance);
                mapTrav.Method("SetSceneColors", new Type[] { typeof(Color), typeof(Color)}).GetValue(currentArea.MainColor, currentArea.LightColor);
                mapTrav.Method("SetSceneryColors", new Type[] { typeof(Color), typeof(Color)}).GetValue(GameColors.Instance.blue, GameColors.Instance.gold);
                mapTrav.Method("SetNodeColors", new Type[] { typeof(Color), typeof(Color)}).GetValue(GameColors.Instance.darkBlue, GameColors.Instance.brightBlue);
                return false;
            }
            return true;
        }
    }
}