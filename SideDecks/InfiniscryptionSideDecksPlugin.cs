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
    [BepInDependency("julianperge.inscryption.specialAbilities.healthForAnts")]
    [BepInDependency("extraVoid.inscryption.LifeCost")]
    public class SideDecksPlugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.sidedecks";
        public const string PluginName = "Infiniscryption Side Decks";
        public const string PluginVersion = "1.0";
        public const string CardPrefix = "ZSDD";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            CustomCards.RegisterCustomCards(harmony);
            harmony.PatchAll(typeof(SideDeckManager));

            // For now, let's take away the side deck selector screen
            // And try to use the TVF Labs Screen
            //harmony.PatchAll(typeof(SideDeckSelectorScreen));
            //AscensionScreenManager.RegisterScreen<SideDeckSelectorScreen>();

            SideDeckSelectionSequencer.Register();

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
