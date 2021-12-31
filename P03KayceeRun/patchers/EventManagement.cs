using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public class EventManagement
    {
        public static StoryEvent ALL_ZONE_ENEMIES_KILLED = (StoryEvent)8055;

        public const int ENEMIES_TO_UNLOCK_BOSS = 4;
        public static int NumberOfZoneEnemiesKilled
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(InfiniscryptionP03Plugin.PluginGuid, "ZoneEnemiesKilled"); }
            set { ModdedSaveManager.RunState.SetValue(InfiniscryptionP03Plugin.PluginGuid, "ZoneEnemiesKilled", value); }
        }

        [HarmonyPatch(typeof(StoryEventsData), "EventCompleted")]
        [HarmonyPrefix]
        public static bool P03AscensionStoryData(ref bool __result, StoryEvent storyEvent)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
            {
                if ((int)storyEvent == (int)ALL_ZONE_ENEMIES_KILLED)
                {
                    __result = NumberOfZoneEnemiesKilled >= ENEMIES_TO_UNLOCK_BOSS;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPrefix]
        public static void TrackVictories(ref TurnManager __instance)
        {
            if (__instance.PlayerWon)
                NumberOfZoneEnemiesKilled += 1;
        }
    }
}