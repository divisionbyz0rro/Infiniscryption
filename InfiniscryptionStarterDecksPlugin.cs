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

        public bool Active
        {
            get
            {
                return Config.Bind("InfiniscryptionStarterDecks", "Active", true, new BepInEx.Configuration.ConfigDescription("Activates the Starter Decks plugin for Infiniscryption. If false, the player will get the default starting deck every time.")).Value;
            }
        }

        public List<string> DeckSpecs
        {
            get
            {
                List<string> retval = new List<string>();
                retval.Add(Config.Bind("InfiniscryptionStarterDecks", "Deck1", "Wolf,WolfCub,Coyote,Bullfrog", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 1 - comma separated. The first card will be the one shown to the player to select.")).Value);
                retval.Add(Config.Bind("InfiniscryptionStarterDecks", "Deck2", "Sparrow,Kingfisher,RavenEgg,Snapper", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 2 - comma separated. The first card will be the one shown to the player to select.")).Value);
                retval.Add(Config.Bind("InfiniscryptionStarterDecks", "Deck3", "Elk,ElkCub,Pronghorn,Porcupine", new BepInEx.Configuration.ConfigDescription("This defines Starter Deck 3 - comma separated. The first card will be the one shown to the player to select.")).Value);
                return retval;
            }
		}

        internal static ManualLogSource Log;

        private void Awake()
        {
            if (this.Active)
            {
                Harmony harmony = new Harmony(PluginGuid);
                harmony.PatchAll(typeof(DeckConstructionPatches));

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
