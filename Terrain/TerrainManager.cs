using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Pixelplacement;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Ascension;
using InscryptionAPI.Nodes;
using InscryptionAPI.Guid;

namespace Infiniscryption.Terrain
{
    [HarmonyPatch]
    public class TerrainManager : Singleton<TerrainManager>
    {
        #region Card Markers

        public static readonly Trait ADVANCED_TERRAIN = GuidManager.GetEnumValue<Trait>(TerrainPlugin.PluginGuid, "AdvancedTerrain");

        #endregion

        private List<CardSlot> terrainSlots = new ();

        /// <summary>
        /// A copy of all of the terrain slots.
        /// </summary>
        public List<CardSlot> TerrainSlotsCopy => new(terrainSlots);

        [HarmonyPatch(typeof(BoardManager3D), nameof(BoardManager3D.Initialize))]
        [HarmonyPostfix]
        private static void InitializeTerrainSlot(ref BoardManager3D __instance)
        {
            // // Make sure the old slots are gone - just in case
            // foreach (var slot in TerrainManager.Instance.terrainSlots)
            //     if (slot != null)
            //         GameObject.DestroyImmediate(slot.gameObject);

            // TerrainManager.Instance.terrainSlots.Clear();
            
            // Copy the player slots into the terrain slots
            
        }
    }
}