using System.Collections.Generic;
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
            Ability.GainGemTriple,
            Ability.LatchBrittle,
            Ability.LatchDeathShield,
            Ability.RandomAbility,
            Ability.Tutor
        };

        [HarmonyPatch(typeof(BuildACardInfo), nameof(BuildACardInfo.GetValidAbilities))]
        [HarmonyPostfix]
        public static void NoRecursionForAscension(ref List<Ability> __result)
        {
            if (SaveFile.IsAscension)
            {
                __result.Remove(Ability.DrawCopyOnDeath);
                foreach(Ability ab in AscensionAbilities)
                    if (!__result.Contains(ab))
                        __result.Add(ab);
            }        
        }
    }
}