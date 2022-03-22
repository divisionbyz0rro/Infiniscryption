using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BuildACardPatcher
    {
        public static readonly Ability[] AscensionAbilities = new Ability[] {
            Ability.Strafe,
            Ability.CellBuffSelf,
            Ability.CellTriStrike,
            Ability.ConduitNull,
            Ability.DeathShield,
            Ability.ExplodeOnDeath,
            Ability.LatchBrittle,
            Ability.LatchDeathShield,
            Ability.RandomAbility,
            Ability.ConduitFactory,
            Ability.ConduitSpawnGems,
            Ability.DrawVesselOnHit
        };

        [HarmonyPatch(typeof(BuildACardInfo), nameof(BuildACardInfo.GetValidAbilities))]
        [HarmonyPostfix]
        public static void NoRecursionForAscension(ref List<Ability> __result)
        {
            if (SaveFile.IsAscension)
            {
                __result.Remove(Ability.DrawCopyOnDeath);
                __result.Remove(Ability.GainBattery);
                foreach(Ability ab in AscensionAbilities)
                    if (!__result.Contains(ab))
                        __result.Add(ab);

                __result = __result.Distinct().Randomize().Take(8).ToList();
            }        
        }
    }
}