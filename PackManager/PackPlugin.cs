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

        internal static PackPlugin Instance;

        internal bool ToggleEncounters
        {
            get
            {
                return Config.Bind("EncounterManagement", "ToggleEncounters", true, new BepInEx.Configuration.ConfigDescription("If true, toggling off a card pack will also remove all encounters from the encounter pool that use cards in that pack.")).Value;
            }
        }

        internal bool RemoveDefaultEncounters
        {
            get
            {
                return Config.Bind("EncounterManagement", "RemoveDefaultEncounters", false, new BepInEx.Configuration.ConfigDescription("If true, toggling off the 'default' card pack will remove default encounters from the pool.")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;
            Instance = this;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();

            JSONLoader.LoadFromJSON();
            CreatePacks.CreatePacksForOtherMods();
            AscensionScreenManager.RegisterScreen<PackSelectorScreen>();

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        private void Start()
        {
            PackManager.ForceSyncOfAllPacks();
        }
    }
}
