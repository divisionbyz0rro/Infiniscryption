using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using InscryptionAPI.Helpers;
using System.Linq;
using InscryptionAPI.Card;

namespace Infiniscryption.Terrain
{
    public class Passthrough : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        // This has all of the logic to implement the Dynamite card that is added to the Prospector boss battle
        static Passthrough()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Pass Through";
            info.rulebookDescription = "If a Terrain card has this ability, it will block attacks from the front but allow attacks to pass through from behind.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = false;
            info.passive = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.GrimoraRulebook, AbilityMetaCategory.MagnificusRulebook };

            AbilityID = AbilityManager.Add(
                TerrainPlugin.PluginGuid,
                info,
                typeof(Passthrough),
                TextureHelper.GetImageAsTexture("ability_passthrough.png", typeof(Passthrough).Assembly)
            ).Id;
        }
    }
}