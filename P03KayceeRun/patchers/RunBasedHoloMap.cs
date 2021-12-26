using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class RunBasedHoloMap
    {
        // The holographic map is absolutely bonkers
        // Each screen that you see on the map is called an 'area'
        // Think of it like a region on the paper game map.
        // Each area has nodes on it. Those nodes do things.
        // What's crazy is that the arrows on the edge of the map that you think of as just UI elements,
        // those are actually map nodes. The arrow itself contains all the data of the encounter and behavior.
        // The encounter data, etc, is not stored in the map area - it's stored on the arrow of the adjacent map area.

        private static readonly Dictionary<string, HoloMapWorldData> worldDataCache = new Dictionary<string, HoloMapWorldData>();

        private static GameObject defaultPrefab;
        private static GameObject neutralHoloPrefab;

        private static Dictionary<Opponent.Type, GameObject> bossPrefabs = new();
        private static Dictionary<int, GameObject> arrowPrefabs = new();
        
        public const int NEUTRAL = 0;
        public const int TECH = 1;
        public const int UNDEAD = 2;
        public const int NATURE = 3;
        public const int MAGIC = 4;

        private static readonly Dictionary<int, RegionGeneratorData> REGION_DATA = new();

        private static Dictionary<string, GameObject> objectLookups = new();

        public static readonly int EMPTY = -1;
        public static readonly int BLANK = 0;
        public static readonly int NORTH = 1;
        public static readonly int EAST = 2;
        public static readonly int SOUTH = 4;
        public static readonly int WEST = 8;
        public static readonly int BOSS = 16;
        public static readonly int ENEMY = 32;
        public static readonly int BUFF = 64;
        public static readonly int COUNTDOWN = 128;
        public static readonly int ALL_DIRECTIONS = NORTH | EAST | SOUTH | WEST;
        private static readonly Dictionary<int, string> DIR_LOOKUP = new() {{SOUTH, "S"}, {WEST, "W"}, {NORTH, "N"}, {EAST, "E"}};

        private static GameObject GetGameObject(string singleMapKey)
        {
            string holoMapKey = singleMapKey.Split('/')[0];
            string findPath = singleMapKey.Replace($"{holoMapKey}/", "");
            return GetGameObject(holoMapKey, findPath);
        }

        private static GameObject GetGameObject(string holomap, string findPath)
        {
            string key = $"{holomap}/{findPath}";
            if (objectLookups.ContainsKey(key))
                return objectLookups[key];

            InfiniscryptionP03Plugin.Log.LogInfo($"Getting {holomap} / {findPath} ");
            GameObject resource = Resources.Load<GameObject>($"prefabs/map/holomapareas/HoloMapArea_{holomap}");
            GameObject retval = resource.transform.Find(findPath).gameObject;

            objectLookups.Add(key, retval);

            return retval;
        }

        public static void TryAdd(this IDictionary dict, object key, object value)
        {
            if (!dict.Contains(key))
                dict.Add(key, value);
        }

        private static void Initialize()
        {
            REGION_DATA.Clear(); // All of the actual region data is in the region data class itself
            for (int i = 0; i < 5; i++)
                REGION_DATA.Add(i, new(i));

            defaultPrefab = Resources.Load<GameObject>("prefabs/map/holomapareas/holomaparea");
            InfiniscryptionP03Plugin.Log.LogInfo($"Default prefab is {defaultPrefab}");

            bossPrefabs.TryAdd(Opponent.Type.ArchivistBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleUndeadBoss"));
            bossPrefabs.TryAdd(Opponent.Type.PhotographerBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleNatureBoss"));
            bossPrefabs.TryAdd(Opponent.Type.TelegrapherBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleTech_1"));
            bossPrefabs.TryAdd(Opponent.Type.CanvasBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleWizardBoss"));

            // Let's instantiate the battle arrow prefabs
            arrowPrefabs = new();
            arrowPrefabs.Add(EAST | ENEMY, GetGameObject("neutraleastmain_3", "Nodes/MoveArea_E"));
            arrowPrefabs.Add(SOUTH | ENEMY, GetGameObject("UndeadEntrance", "Nodes/MoveArea_S"));
            arrowPrefabs.Add(NORTH | ENEMY, GetGameObject("naturemainpath_2", "Nodes/MoveArea_N"));
            arrowPrefabs.Add(WEST | ENEMY, GetGameObject("neutralwestmain_2", "Nodes/MoveArea_W"));

            arrowPrefabs.Add(WEST | COUNTDOWN, GetGameObject("natureentrance", "Nodes/MoveArea_W"));
            arrowPrefabs.Add(SOUTH | COUNTDOWN, GetGameObject("wizardmainpath_3", "Nodes/MoveArea_S"));

            // This generates 'pseudo-prefab' objects
            // We will have one for each zone
            // Each random node will randomly turn scenery nodes on and off
            // And will set the arrows appropriately.
            neutralHoloPrefab = GameObject.Instantiate(defaultPrefab);
        }

        private static int xRelative(float x)
        {
            if (x <= -1.1f)
                return -1;
            if (x < 1.1f)
                return 0;
            return 1;
        }

        private static int zRelative(float z)
        {
            if (z <= -1.1f)
                return -1;
            if (z < 1.1f)
                return 0;
            return 1;
        }

        private static bool isValidLocation(float x, float z, int moveLocations)
        {
            int zR = zRelative(z);
            int xR = xRelative(x);
            if (zR == 0 && xR == 0)
                return false;

            if ((moveLocations & NORTH) != 0)
                if (zR == 1 && xR == 0)
                    return false;

            if ((moveLocations & SOUTH) != 0)
                if (zR == -1 && xR == 0)
                    return false;

            if ((moveLocations & EAST) != 0)
                if (zR == 0 && xR == 1)
                    return false;

            if ((moveLocations & WEST) != 0)
                if (zR == 0 && xR == -1)
                    return false;

            return true;
        }

        private static Vector3 GetRandomLocation(int seed, int moveLocations)
        {
            float x = 0f;
            float z = 0f;
            for (int i = 0; i < 100; i++)
            {
                Random.InitState(seed + i);
                x = Random.Range(-3.2f, 3.2f);
                z = Random.Range(-2.02f, 2.02f);

                if (isValidLocation(x, z, moveLocations))
                    return new Vector3(x, 0.1f, z);

                seed += 10;
            }
            return new Vector3(x, 0.1f, z); // give up
        }

        private static GameObject BuildMapAreaPrefab(int regionId, HoloMapBlueprint bp)
        {
            InfiniscryptionP03Plugin.Log.LogInfo($"Building gameobject for [{bp.x},{bp.y}]");

            if (bp.opponent != Opponent.Type.Default)
                return GameObject.Instantiate(bossPrefabs[bp.opponent]);

            InfiniscryptionP03Plugin.Log.LogInfo($"Instantiating base object {neutralHoloPrefab}");
            GameObject area = GameObject.Instantiate(neutralHoloPrefab);

            InfiniscryptionP03Plugin.Log.LogInfo($"Getting nodes");
            GameObject nodes = area.transform.Find("Nodes").gameObject;

            if (DIR_LOOKUP.ContainsKey(bp.enemyDirection))
            {
                InfiniscryptionP03Plugin.Log.LogInfo($"Finding arrow to destroy");
                GameObject arrowToReplace = area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[bp.enemyDirection]}").gameObject;
                InfiniscryptionP03Plugin.Log.LogInfo($"Destroying arrow");
                GameObject.Destroy(arrowToReplace);
                
                InfiniscryptionP03Plugin.Log.LogInfo($"Copying arrow");
                GameObject newArrow = GameObject.Instantiate(arrowPrefabs[bp.enemyDirection | ENEMY], nodes.transform);
            }

            InfiniscryptionP03Plugin.Log.LogInfo($"Setting arrows and walls active");
            Transform scenery = area.transform.Find("Scenery");
            foreach (int key in DIR_LOOKUP.Keys)
            {
                GameObject wall = GetGameObject(REGION_DATA[regionId].wall);
                area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.SetActive((bp.arrowDirections & key) != 0);
                if ((bp.arrowDirections & key) == 0)
                {
                    GameObject wallClone = GameObject.Instantiate(wall, scenery);
                    wallClone.transform.localPosition = REGION_DATA[regionId].wallOrientations[key].Item1;
                    wallClone.transform.localEulerAngles = REGION_DATA[regionId].wallOrientations[key].Item2;
                }
            }

            InfiniscryptionP03Plugin.Log.LogInfo($"Generating random scenery");
            for (int i = 0; i < 20; i++)
            {
                int randomSeed = bp.randomSeed + i * 100;
                Random.InitState(randomSeed);

                string[] scenerySource = i % 7 == 0 ? REGION_DATA[regionId].objectRandoms : REGION_DATA[regionId].terrainRandoms;

                string sceneryKey = scenerySource[Random.Range(0, scenerySource.Length)];
                GameObject sceneryObject = GameObject.Instantiate(GetGameObject(sceneryKey), scenery);
                sceneryObject.transform.localPosition = GetRandomLocation(randomSeed, bp.arrowDirections);
                sceneryObject.transform.localEulerAngles = new Vector3(7.4407f, Random.Range(0f, 360f), .0297f);
            }

            InfiniscryptionP03Plugin.Log.LogInfo($"Setting grid data");
            HoloMapArea areaData = area.GetComponent<HoloMapArea>();
            Traverse areaTrav = Traverse.Create(areaData);
            areaData.GridX = bp.x;
            areaData.GridY = bp.y;
            areaTrav.Field("mainColor").SetValue(REGION_DATA[regionId].mainColor);
            areaTrav.Field("lightColor").SetValue(REGION_DATA[regionId].mainColor);

            return area;
        }

        private static void ConnectArea(HoloMapWorldData.AreaData[,] map, HoloMapBlueprint bp)
        {
            GameObject area = map[bp.x, bp.y].prefab;

            if (area == null)
                return;

            HoloMapArea areaData = area.GetComponent<HoloMapArea>();

            // The order of these adds is super important, as it corresponds to the values of the LookDirection enum
            // So I'm not going to use any enumeration just to make it crystal clear what order they go in
            areaData.DirectionNodes.Clear();
            areaData.DirectionNodes.Add((bp.arrowDirections & NORTH) == 0 ? null : area.transform.Find("Nodes/MoveArea_N").gameObject.GetComponent<MoveHoloMapAreaNode>());
            areaData.DirectionNodes.Add((bp.arrowDirections & EAST) == 0 ? null : area.transform.Find("Nodes/MoveArea_E").gameObject.GetComponent<MoveHoloMapAreaNode>());
            areaData.DirectionNodes.Add((bp.arrowDirections & SOUTH) == 0 ? null : area.transform.Find("Nodes/MoveArea_S").gameObject.GetComponent<MoveHoloMapAreaNode>());
            areaData.DirectionNodes.Add((bp.arrowDirections & WEST) == 0 ? null : area.transform.Find("Nodes/MoveArea_W").gameObject.GetComponent<MoveHoloMapAreaNode>());

        }

        private static readonly int[][] NSEW = new int[][] { new int[]{ -1, 0 }, new int[]{1, 0}, new int[]{0, -1}, new int[]{0, 1}};

        private static IEnumerable<HoloMapBlueprint> AdjacentToQuadrant(this HoloMapBlueprint[,] map, int x, int y)
        {
            int minX = x <= 2 ? 0 : 3;
            int minY = y <= 2 ? 0 : 3;
            int maxX = x <= 2 ? 2 : 5;
            int maxY = y <= 2 ? 2 : 5;

            return NSEW.Where(p => x + p[0] >= minX &&
                                   y + p[1] >= minY &&
                                   x + p[0] <= maxX &&
                                   y + p[1] <= maxY)
                       .Select(p => map[x + p[0], y + p[1]]);
        }

        private static IEnumerable<HoloMapBlueprint> AdjacentToQuadrant(this HoloMapBlueprint[,] map, HoloMapBlueprint node)
        {
            return map.AdjacentToQuadrant(node.x, node.y);
        }
        
        private static IEnumerable<HoloMapBlueprint> AdjacentTo(this HoloMapBlueprint[,] map, int x, int y)
        {
            return NSEW.Where(p => x + p[0] >= 0 &&
                                   y + p[1] >= 0 &&
                                   x + p[0] < map.GetLength(0) &&
                                   y + p[1] < map.GetLength(1))
                       .Select(p => map[x + p[0], y + p[1]]);
        }

        private static IEnumerable<HoloMapBlueprint> AdjacentTo(this HoloMapBlueprint[,] map, HoloMapBlueprint node)
        {
            return map.AdjacentTo(node.x, node.y);
        }

        private static void PaintQuadrant(HoloMapBlueprint[,] map, int x, int y, int color)
        {
            if (map[x, y] == null)
            {
                foreach (HoloMapBlueprint adjNode in map.AdjacentTo(x, y))
                {
                    if (adjNode != null)
                    {
                        PaintQuadrant(map, adjNode.x, adjNode.y, color);
                        return;
                    }
                }
            }

            // Staying within the given quadrant, paint all adjacent nodes the same color as you
            map[x, y].color = color;

            foreach (HoloMapBlueprint adjNode in map.AdjacentToQuadrant(x, y))
                if (adjNode != null && adjNode.color == 0)
                    PaintQuadrant(map, adjNode.x, adjNode.y, color);
        }

        private static void FixPaint(HoloMapBlueprint[,] map, int x, int y)
        {
            if (map[x,y] != null && map[x,y].color == 0)
            {
                foreach (HoloMapBlueprint adj in map.AdjacentTo(x, y))
                {
                    if (adj != null)
                    {
                        map[x,y].color = adj.color;
                        return;
                    }
                }
            }
        }

        private static int DirTo(this HoloMapBlueprint start, HoloMapBlueprint end)
        {
            int retval = BLANK;
            retval = retval | (start.x == end.x ? 0 : start.x < end.x ? EAST : WEST);
            retval = retval | (start.y == end.y ? 0 : start.y < end.y ? SOUTH : NORTH);
            return retval;
        }

        private static void CrawlQuadrant(HoloMapBlueprint[,] map, int x, int y)
        {
            // Find all adjacent uncrawled nodes
            List<HoloMapBlueprint> uncrawled = map.AdjacentTo(x, y)
                                                  .Where(bp => bp != null)
                                                  .Where(bp => map[x,y].color == bp.color)
                                                  .Where(bp => bp.arrowDirections == BLANK)
                                                  .ToList();

            if (uncrawled.Count == 0)
                return;            

            // Pick a random adjacent uncrawled node
            HoloMapBlueprint current = map[x,y];
            HoloMapBlueprint next = uncrawled[Random.Range(0, uncrawled.Count)];
            current.arrowDirections = current.arrowDirections | current.DirTo(next);
            next.arrowDirections = next.arrowDirections | next.DirTo(current);

            CrawlQuadrant(map, next.x, next.y);
            
            // double check this one again
            CrawlQuadrant(map, x, y);
        }

        private static void ConnectQuadrants(HoloMapBlueprint[,] map)
        {
            // This is too hard to generalize, although maybe I'll come up with a way to do it?
            
            for (int i = 2; i >= 0; i--)
            {
                if (map[i, 2] != null && map[i, 3] != null)
                {
                    map[i, 2].arrowDirections = map[i, 2].arrowDirections | SOUTH;
                    map[i, 3].arrowDirections = map[i, 3].arrowDirections | NORTH;
                    break;
                }
            }

            for (int i = 3; i <= 5; i++)
            {
                if (map[i, 2] != null && map[i, 3] != null)
                {
                    map[i, 2].arrowDirections = map[i, 2].arrowDirections | SOUTH;
                    map[i, 3].arrowDirections = map[i, 3].arrowDirections | NORTH;
                    break;
                }
            }

            for (int j = 2; j >= 0; j--)
            {
                if (map[2, j] != null && map[3, j] != null)
                {
                    map[2, j].arrowDirections = map[2, j].arrowDirections | EAST;
                    map[3, j].arrowDirections = map[3, j].arrowDirections | WEST;
                    break;
                }
            }

            for (int j = 3; j <= 5; j++)
            {
                if (map[2, j] != null && map[3, j] != null)
                {
                    map[2, j].arrowDirections = map[2, j].arrowDirections | EAST;
                    map[3, j].arrowDirections = map[3, j].arrowDirections | WEST;
                    break;
                }
            }
        }

        private static List<HoloMapBlueprint> BuildBlueprint(string id, int seed)
        {
            string blueprintKey = $"ascensionBlueprint{id}";
            string savedBlueprint = RunStateHelper.GetValue(blueprintKey);

            if (savedBlueprint != default(string))
                return savedBlueprint.Split('|').Select(s => new HoloMapBlueprint(s)).ToList();

            Random.InitState(seed);
            int x = 0;
            int y = 0;

            // Start with a 6x6 grid
            HoloMapBlueprint[,] bpBlueprint = new HoloMapBlueprint[6,6];
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j ++)
                    bpBlueprint[i, j] = new HoloMapBlueprint(seed + 10*i + 100*j) { x = i, y = j, arrowDirections = BLANK };

            // Set the corners empty
            bpBlueprint[0,0] = bpBlueprint[0,1] = bpBlueprint[1,0] = null;
            bpBlueprint[0,4] = bpBlueprint[0,5] = bpBlueprint[1,5] = null;
            bpBlueprint[4,0] = bpBlueprint[5,0] = bpBlueprint[5,1] = null;
            bpBlueprint[4,5] = bpBlueprint[5,5] = bpBlueprint[5,4] = null;
            
            // Randomly chop some rooms
            // Chop one of the middle rooms
            bpBlueprint[Random.Range(2,4), Random.Range(2,4)] = null;
            
            // Randomly chop a corner
            int[] corners = new int[] { 1, 4 };
            bpBlueprint[corners[Random.Range(0, 2)], corners[Random.Range(0, 2)]] = null;

            // Randomly chop an interior side
            x = Random.Range(1, 5);
            y = x == 1 || x == 4 ? Random.Range(2, 4) : corners[Random.Range(0, 2)];
            bpBlueprint[x, y] = null;

            // Paint each quadrant
            PaintQuadrant(bpBlueprint, 1, 1, 1);
            PaintQuadrant(bpBlueprint, 1, 4, 2);
            PaintQuadrant(bpBlueprint, 4, 1, 3);
            PaintQuadrant(bpBlueprint, 4, 4, 4);

            // Make sure all rooms are painted
            for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                for (int j = 0; j < bpBlueprint.GetLength(1); j++)
                    FixPaint(bpBlueprint, i, j);

            // Crawl and mark each quadrant.
            while (bpBlueprint[x = Random.Range(0, 3), y = Random.Range(0, 3)] == null); CrawlQuadrant(bpBlueprint, x, y);
            while (bpBlueprint[x = Random.Range(0, 3), y = Random.Range(3, 6)] == null); CrawlQuadrant(bpBlueprint, x, y);
            while (bpBlueprint[x = Random.Range(3, 6), y = Random.Range(0, 3)] == null); CrawlQuadrant(bpBlueprint, x, y);
            while (bpBlueprint[x = Random.Range(3, 6), y = Random.Range(3, 6)] == null); CrawlQuadrant(bpBlueprint, x, y);

            // Set up the connections between quadrants
            ConnectQuadrants(bpBlueprint);

            // Figure out the starting space
            HoloMapBlueprint startSpace = bpBlueprint[0, 2];
            List<HoloMapBlueprint> retval = new() { startSpace };
            for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                for (int j = 0; j < bpBlueprint.GetLength(1); j ++)
                    if (bpBlueprint[i, j] != null && bpBlueprint[i, j] != startSpace)
                        retval.Add(bpBlueprint[i, j]);

            // 

            savedBlueprint = string.Join("|", retval.Select(b => b.ToString()));
            RunStateHelper.SetValue(blueprintKey, savedBlueprint);
            SaveManager.SaveToFile();
            return retval;
        }

        public static HoloMapWorldData GetAscensionWorldbyId(string id)
        {
            HoloMapWorldData data = ScriptableObject.CreateInstance<HoloMapWorldData>();
            data.name = id;

            List<HoloMapBlueprint> blueprints = BuildBlueprint(id, SaveManager.SaveFile.randomSeed);
            int xDimension = blueprints.Select(b => b.x).Max() + 1;
            int yDimension = blueprints.Select(b => b.y).Max() + 1;

            data.areas = new HoloMapWorldData.AreaData[xDimension, yDimension];

            foreach(HoloMapBlueprint bp in blueprints)
                data.areas[bp.x, bp.y] = new() { prefab = BuildMapAreaPrefab(0, bp) };

            // The second pass creates relationships between everything
            foreach(HoloMapBlueprint bp in blueprints)
                ConnectArea(data.areas, bp);

            return data;
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        public static void EnsurePart3Saved()
        {
            if (SaveFile.IsAscension)
            {
                // Check to see if there is a part 3 save data yet
                P03AscensionSaveData.EnsureRegularSave();
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), "GetCurrentArea")]
        [HarmonyPrefix]
        public static void LogData()
        {
            InfiniscryptionP03Plugin.Log.LogInfo($"{Part3SaveData.Data.playerPos.worldId},{Part3SaveData.Data.playerPos.gridX},{Part3SaveData.Data.playerPos.gridY}");
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPostfix]
        private static void RewritePart3IntroSequence(ref Part3SaveData __instance)
        {
            if (SaveFile.IsAscension)
            {
                __instance.playerPos = new("ascension_test", 0, 2) { gridX = 0, gridY = 2};
                __instance.checkpointPos = new Part3SaveData.WorldPosition(__instance.playerPos);
                __instance.reachedCheckpoints = new List<string>() { __instance.playerPos.worldId };
            }
        }

        private static bool ValidateWorldData(HoloMapWorldData data)
        {
            if (data == null || data.areas == null)
                return false;

            for (int i = 0; i < data.areas.GetLength(0); i++)
                for (int j = 0; j < data.areas.GetLength(1); j++)
                    if (data.areas[i,j] != null && data.areas[i,j].prefab != null)
                        return true;

            return false;
        }

        [HarmonyPatch(typeof(HoloMapDataLoader), "GetWorldById")]
        [HarmonyPrefix]
        private static bool PatchGetAscensionWorldById(ref HoloMapWorldData __result, string id)
        {
            if (id.ToLowerInvariant().StartsWith("ascension_"))
            {
                if (worldDataCache.ContainsKey(id) && ValidateWorldData(worldDataCache[id]))
                {
                    __result = worldDataCache[id];
                    return false;
                }

                Initialize();
                if (worldDataCache.ContainsKey(id))
                    worldDataCache.Remove(id);
                worldDataCache.Add(id, GetAscensionWorldbyId(id.Replace("ascension_", "")));
                __result = worldDataCache[id];
                return false;
            }                
            return true;        
        }
    }
}