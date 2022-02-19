using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Infiniscryption.SideDecks.Patchers;
using Infiniscryption.SideDecks.Sequences;
using Infiniscryption.SideDecks.UserInterface;
using InscryptionAPI.Ascension;

namespace Infiniscryption.SideDecks
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("julianperge.inscryption.cards.healthForAnts")]
    public class SideDecksPlugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.sidedecks";
		public const string PluginName = "Infiniscryption Side Decks";
		public const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            CustomCards.RegisterCustomCards(harmony);
            harmony.PatchAll(typeof(SideDeckManager));

            harmony.PatchAll(typeof(SideDeckSelectorScreen));
            AscensionScreenManager.RegisterScreen<SideDeckSelectorScreen>();

            SideDeckSelectionSequencer.Register();

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
