using APIPlugin;
using System;
using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;

namespace Infiniscryption.Spells.Patchers
{
    public static class StackableSigilDefectFix
    {
        private static bool _shouldPatch;
        private static bool _checkedPatch = false;
        private static bool ShouldPatch
        {
            get
            {
                if (!_checkedPatch)
                {
                    Assembly apiAssembly = Assembly.GetAssembly(typeof(APIPlugin.NewAbility));
                    Version apiVersion = apiAssembly.GetName().Version;

                    InfiniscryptionSpellsPlugin.Log.LogInfo($"I see API version {apiVersion}");

                    _shouldPatch = (apiVersion.Major < 1) ||
                                   (apiVersion.Major == 1 && (apiVersion.Minor < 12 ||
                                                             (apiVersion.Minor == 12 && apiVersion.Build <= 1)));

                    _checkedPatch = true;
                }

                return _shouldPatch;
            }
        }

        // This is designed to patch a bug in the API.
        // We'll only patch certain versions of the API.
        [HarmonyPatch(typeof(CardTriggerHandler), "AddAbility", typeof(Ability))]
        [HarmonyPrefix]
        [HarmonyBefore(new string[] { "cyantist.inscryption.api" } )]
	    public static bool APIDefectFix_CardTriggerHandler_AddAbility_Ability(Ability ability, CardTriggerHandler __instance)
        {
            if (!ShouldPatch) // Only run this patch for some versions of the API
                return true;

			if ((int)ability < 99) // Only run this patch for custom abilites
			{
				return true;
			}

            // Only run this patch if this is stackable. Otherwise, let the other code run.
            if (!(AbilitiesUtil.GetInfo(ability).canStack && !AbilitiesUtil.GetInfo(ability).passive))
                return true;

            InfiniscryptionSpellsPlugin.Log.LogDebug($"API Defect Fix for stackable ability {ability}");

            NewAbility newAbility = NewAbility.abilities.Find(x => x.ability == ability);
            Type type = newAbility.abilityBehaviour;
            AbilityBehaviour item = __instance.gameObject.GetComponent(type) as AbilityBehaviour;
            if (item == null)
                item = __instance.gameObject.AddComponent(type) as AbilityBehaviour;

            Traverse trav = Traverse.Create(__instance);
            List<Tuple<Ability, AbilityBehaviour>> triggers = trav.Field("triggeredAbilities").GetValue<List<Tuple<Ability, AbilityBehaviour>>>();
            triggers.Add(new Tuple<Ability, AbilityBehaviour>(ability, item));

			return false;
		}
    }
}