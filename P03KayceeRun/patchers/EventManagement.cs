using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public class EventManagement
    {
        public static StoryEvent ALL_ZONE_ENEMIES_KILLED = (StoryEvent)8055;
        public static StoryEvent ALL_BOSSES_KILLED = (StoryEvent)805535;
        public static StoryEvent HAS_DRAFT_TOKEN = (StoryEvent)2477;

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
                if ((int)storyEvent == (int)ALL_BOSSES_KILLED)
                {
                    __result = false; // Force to false for right now
                    return false;
                }
                if ((int)storyEvent == (int)HAS_DRAFT_TOKEN)
                {
                    __result = Part3SaveData.Data.deck.Cards.Any(card => card.name == CustomCards.DRAFT_TOKEN);
                    return false;
                }

                if (storyEvent == StoryEvent.GemsModuleFetched) // Simply going to this world 'completes' that story event for you
                {
                    __result = P03AscensionSaveData.VisitedZones.Contains("FastTravelMapNode_Wizard");
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPrefix]
        public static void TrackVictories(ref TurnManager __instance)
        {
            if (__instance.Opponent.NumLives <= 0 || __instance.Opponent.Surrendered)
                NumberOfZoneEnemiesKilled = NumberOfZoneEnemiesKilled + 1;
        }
    }
}