using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.VanillaStackable
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class InfiniscryptionVanillaStackablePlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.vanillastackablesigils";
		private const string PluginName = "Infiniscryption Vanilla Stackable Abilities";
		private const string PluginVersion = "1.0";

        public static Ability[] VANILLA_STACKABLES = new Ability[]
        {
            Ability.BeesOnHit,
            Ability.DrawAnt,
            Ability.DrawCopy,
            Ability.DrawCopyOnDeath,
            Ability.DrawRabbits,
            Ability.DrawRandomCardOnDeath,
            Ability.DrawVesselOnHit,
            Ability.GainBattery,
            Ability.Loot,
            Ability.QuadrupleBones,
            Ability.RandomConsumable,
            Ability.Sentry,
            Ability.Sharp,
            Ability.Tutor,
            Ability.BuffNeighbours,
            Ability.BuffEnemy,
            Ability.BuffGems,
            Ability.DebuffEnemy,
            Ability.ConduitBuffAttack,
            Ability.BuffGems
        };

        internal static ManualLogSource Log;

        private bool AddCards
        {
            get
            {
                return Config.Bind("InfiniscryptionVanillaStackable", "DebugMode", false, new BepInEx.Configuration.ConfigDescription("If true, this will add debug cards to the pool and will start the game with a bunch of them.")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;

            foreach (Ability ability in VANILLA_STACKABLES)
                AbilityManager.BaseGameAbilities.AbilityByID(ability).Info.canStack = true;
        }
    }
}
