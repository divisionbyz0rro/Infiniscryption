using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class EventManagement
    {
        public const StoryEvent ALL_ZONE_ENEMIES_KILLED = (StoryEvent)8055;
        public const StoryEvent ALL_BOSSES_KILLED = (StoryEvent)805535;
        public const StoryEvent HAS_DRAFT_TOKEN = (StoryEvent)2477;

        public static int UpgradePrice
        {
            get
            {
                return 6 + (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 4 : 0);
            }
        }

        public readonly static Tuple<int, int> CURRENCY_GAIN_RANGE = new(5, 10);

        public static int NumberOfLivesRemaining
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(InfiniscryptionP03Plugin.PluginGuid, "NumberOfLivesRemaining"); }
            set { ModdedSaveManager.RunState.SetValue(InfiniscryptionP03Plugin.PluginGuid, "NumberOfLivesRemaining", value); }
        }

        public const int ENEMIES_TO_UNLOCK_BOSS = 4;
        public static int NumberOfZoneEnemiesKilled
        {
            get { return ModdedSaveManager.RunState.GetValueAsInt(InfiniscryptionP03Plugin.PluginGuid, "ZoneEnemiesKilled"); }
            set { ModdedSaveManager.RunState.SetValue(InfiniscryptionP03Plugin.PluginGuid, "ZoneEnemiesKilled", value); }
        }

        public static List<string> CompletedZones
        {
            get
            {
                string zoneCsv = ModdedSaveManager.RunState.GetValue(InfiniscryptionP03Plugin.PluginGuid, "CompletedZones");
                if (zoneCsv == default(string))
                    return new List<string>();

                return zoneCsv.Split(',').ToList();
            }
        }

        public static void AddCompletedZone(StoryEvent storyEvent)
        {
            if (storyEvent == StoryEvent.ArchivistDefeated) AddCompletedZone("FastTravelMapNode_Undead");
            if (storyEvent == StoryEvent.CanvasDefeated) AddCompletedZone("FastTravelMapNode_Wizard");
            if (storyEvent == StoryEvent.TelegrapherDefeated) AddCompletedZone("FastTravelMapNode_Tech");
            if (storyEvent == StoryEvent.PhotographerDefeated) AddCompletedZone("FastTravelMapNode_Nature");
        }

        public static void AddCompletedZone(string id)
        {
            List<string> zones = CompletedZones;
            if (!zones.Contains(id))
                zones.Add(id);
            
            ModdedSaveManager.RunState.SetValue(InfiniscryptionP03Plugin.PluginGuid, "CompletedZones", string.Join(",", zones));
        }

        public static List<string> VisitedZones
        {
            get
            {
                string zoneCsv = ModdedSaveManager.RunState.GetValue(InfiniscryptionP03Plugin.PluginGuid, "VisitedZones");
                if (zoneCsv == default(string))
                    return new List<string>();

                return zoneCsv.Split(',').ToList();
            }
        }
        public static void AddVisitedZone(string id)
        {
            List<string> zones = VisitedZones;
            if (!zones.Contains(id))
                zones.Add(id);
            
            ModdedSaveManager.RunState.SetValue(InfiniscryptionP03Plugin.PluginGuid, "VisitedZones", string.Join(",", zones));
        }

        public static StoryEvent GetStoryEventForOpponent(Opponent.Type opponent)
        {
            if (opponent == Opponent.Type.PhotographerBoss)
                return StoryEvent.PhotographerDefeated;
            if (opponent == Opponent.Type.TelegrapherBoss)
                return StoryEvent.TelegrapherDefeated;
            if (opponent == Opponent.Type.CanvasBoss)
                return StoryEvent.CanvasDefeated;
            if (opponent == Opponent.Type.ArchivistBoss)
                return StoryEvent.ArchivistDefeated;

            return StoryEvent.WoodcarverDefeated;
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
                    //__result = true;
                    return false;
                }
                if ((int)storyEvent == (int)ALL_BOSSES_KILLED)
                {
                    __result = CompletedZones.Count >= 4;
                    return false;
                }
                if ((int)storyEvent == (int)HAS_DRAFT_TOKEN)
                {
                    __result = Part3SaveData.Data.deck.Cards.Any(card => card.name == CustomCards.DRAFT_TOKEN);
                    return false;
                }

                if (storyEvent == StoryEvent.GemsModuleFetched) // Simply going to this world 'completes' that story event for you
                {
                    __result = EventManagement.VisitedZones.Contains("FastTravelMapNode_Wizard");
                    return false;
                }

                if (storyEvent == StoryEvent.HoloTechTempleSatelliteActivated)
                {
                    __result = true;
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

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "zorro.inscryption.infiniscryption.curses" })]
        public static void SortOutStartOfRun(ref AscensionSaveData __instance)
        {
            // Figure out the number of lives
            NumberOfLivesRemaining = __instance.currentRun.maxPlayerLives;
        }

        public static void FinishAscension(bool success=true)
		{
            InfiniscryptionP03Plugin.Log.LogInfo("Starting finale sequence");
			AscensionMenuScreens.ReturningFromSuccessfulRun = success;
            AscensionStatsData.TryIncrementStat(success ? AscensionStat.Type.Victories : AscensionStat.Type.Losses);

            if (success)
            {
                foreach (AscensionChallenge c in AscensionSaveData.Data.activeChallenges)
                    if (!AscensionSaveData.Data.conqueredChallenges.Contains(c))
                        AscensionSaveData.Data.conqueredChallenges.Add(c);

                if (!string.IsNullOrEmpty(AscensionSaveData.Data.currentStarterDeck) && !AscensionSaveData.Data.conqueredStarterDecks.Contains(AscensionSaveData.Data.currentStarterDeck))
                    AscensionSaveData.Data.conqueredStarterDecks.Add(AscensionSaveData.Data.currentStarterDeck);
            }

            // Delete the ascension save; the run is over            
            ModdedSaveManager.SaveData.SetValue(InfiniscryptionP03Plugin.PluginGuid, P03AscensionSaveData.ASCENSION_SAVE_KEY, default(string)); 

            SaveManager.SaveToFile(true);

            P03AscensionSaveData.IsP03Run = false; // and force the state of p03 runs back to false

            InfiniscryptionP03Plugin.Log.LogInfo("Loading ascension scene");
            SceneLoader.Load("Ascension_Configure");
		}
    }
}