using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Encounters;
using InscryptionAPI.Regions;

namespace Infiniscryption.PackManagement
{
    public static class Extensions
    {
        public static bool IsBaseGameEncounter(this EncounterBlueprintData data)
        {
            return EncounterManager.BaseGameEncounters.Any(bge => bge.name.Equals(data.name));
        }

        public static bool IsBaseGameRegion(this RegionData data)
        {
            return RegionManager.BaseGameRegions.Any(bge => bge.name.Equals(data.name));
        }

        private static readonly Dictionary<string, string> ModIds = new();

        private static string GetModIdFromAssembly(Assembly assembly)
        {
            if (ModIds.ContainsKey(assembly.FullName))
                return ModIds[assembly.FullName];

            foreach (var t in assembly.GetTypes())
            {
                foreach (var d in t.GetCustomAttributes<BepInPlugin>())
                {
                    if (d.GUID == InscryptionAPIPlugin.ModGUID)
                        continue;

                    if (d.GUID == PackPlugin.PluginGuid)
                        continue;

                    ModIds.Add(assembly.FullName, d.GUID);
                    return d.GUID;
                }
            }

            ModIds.Add(assembly.FullName, default);
            return default;
        }

        internal static string GetModIdFromCallstack(Assembly callingAssembly)
        {
            string cacheVal = GetModIdFromAssembly(callingAssembly);
            if (!string.IsNullOrEmpty(cacheVal))
                return cacheVal;

            StackTrace trace = new();
            foreach (var frame in trace.GetFrames())
            {
                string newVal = GetModIdFromAssembly(frame.GetMethod().DeclaringType.Assembly);
                if (!string.IsNullOrEmpty(newVal))
                    return newVal;
            }

            return default;
        }
    }
}