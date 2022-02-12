using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Infiniscryption.PackManagement.Patchers;
using Infiniscryption.PackManagement.UserInterface;
using InscryptionAPI.Ascension;

namespace Infiniscryption.PackManagement
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class PackPlugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.packmanager";
		public const string PluginName = "Infiniscryption Pack Manager";
		public const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();

            JSONLoader.LoadFromJSON();
            CreatePacks.CreatePacksForOtherMods();
            AscensionScreenManager.RegisterScreen<PackSelectorScreen>();

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
