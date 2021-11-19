using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Helpers;
using Infiniscryption.Patchers;

namespace Infiniscryption
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInIncompatibility("porta.inscryption.traderstart")] // Both plugins replace the starting deck
    public class InfiniscryptionStarterDecksPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.starterdecks";
		private const string PluginName = "Infiniscryption Starter Decks";
		private const string PluginVersion = "1.0";

        private bool Active
        {
            get
            {
                return Config.Bind("InfiniscryptionStarterDecks", "Active", true, new BepInEx.Configuration.ConfigDescription("Activates the Starter Decks plugin for Infiniscryption. If false, the player will get the default starting deck every time.")).Value;
            }
        }

        private string[] _deckSpecsConfig
        {
            get
            {
                return new string[] {
                    Config.Bind("InfiniscryptionStarterDecks", "Deck1", "Wolf,WolfCub,Coyote,Bullfrog", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 1 - comma separated. The first card will be the one shown to the player to select.")).Value,
                    Config.Bind("InfiniscryptionStarterDecks", "Deck2", "Sparrow,Bat,RavenEgg,Snapper", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 2 - comma separated. The first card will be the one shown to the player to select.")).Value,
                    Config.Bind("InfiniscryptionStarterDecks", "Deck3", "Elk,ElkCub,Pronghorn,Porcupine", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 3 - comma separated. The first card will be the one shown to the player to select.")).Value
                };
            }
		}
        public static string[] DeckSpecs;

        private string[] _deckEvolutionsConfig
        {
            get
            {
                return new string[] {
                    Config.Bind("InfiniscryptionStarterDecks", "Deck1Evolution", "1=Wolf_Talking,3+2H&+WhackAMoleS,2=Alpha&+-1O,0+TailOnHitS,3+SharpS,2+-1O&+1H,0+DrawRabbitS,1+FlyingS", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 1 - comma separated. The first card will be the one shown to the player to select.")).Value,
                    Config.Bind("InfiniscryptionStarterDecks", "Deck2Evolution", "3=Stoat_Talking&+1H&+QuadrupleBonesS,1+-1O,0+BeesOnHitS,2+SharpS,3+DeathtouchS,0+SplitStrikeS,2+PreventAttackS,1+UnkillableS", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 2 - comma separated. The first card will be the one shown to the player to select.")).Value,
                    Config.Bind("InfiniscryptionStarterDecks", "Deck3Evolution", "3=Stinkbug_Talking&+1H,0+1A,2+SharpS,1+SubmergeS,3+SharpS&+RandomAbilityS,1+DrawAnt,0+1A&+2H&+StrafePushS,2+1A", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 3 - comma separated. The first card will be the one shown to the player to select.")).Value
                };
            }
        }
        public static string[] DeckEvolutions;

        private int _costPerLevelConfig
        {
            get { return Config.Bind("InfiniscryptionStarterDecks", "UpgradeCostPerLevel", 4, new BepInEx.Configuration.ConfigDescription("The amount of excess teeth you have to spend per level of upgrade.")).Value; }
        }
        public static int CostPerLevel;

        internal static ManualLogSource Log;

        private void Awake()
        {
            if (this.Active)
            {
                DeckEvolutions = _deckEvolutionsConfig;
                DeckSpecs = _deckSpecsConfig;
                CostPerLevel = _costPerLevelConfig;

                Harmony harmony = new Harmony(PluginGuid);
                harmony.PatchAll(typeof(DeckConstructionPatches));
                harmony.PatchAll(typeof(MetaCurrencyPatches));
                harmony.PatchAll(typeof(SaveGameHelper));

                InfiniscryptionStarterDecksPlugin.Log = base.Logger;

                // And we're loaded
                Logger.LogInfo($"Plugin {PluginName} is loaded!");
            }
            else
            {
                Logger.LogInfo($"Plugin {PluginName} is loaded, but DEACTIVATED due to configuration!");
            }     
        }
    }
}
