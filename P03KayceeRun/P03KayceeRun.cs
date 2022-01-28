using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;

namespace Infiniscryption.P03KayceeRun
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("MADH.inscryption.ActivatedAbilityFix")]
    [BepInDependency("irie.inscryption.vap")]
    public class P03Plugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";
		public const string PluginName = "Infiniscryption P03 in Kaycee's Mod";
		public const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
            CustomCards.RegisterCustomCards(harmony);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
