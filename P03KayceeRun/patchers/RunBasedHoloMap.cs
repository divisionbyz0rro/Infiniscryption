using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Saves;
using System;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static partial class RunBasedHoloMap
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
        private static Dictionary<HoloMapSpecialNode.NodeDataType, GameObject> specialNodePrefabs = new();
        private static Dictionary<int, GameObject[]> specialTerrainPrefabs = new();
        private static Dictionary<int, GameObject> arrowPrefabs = new();
        
        public const int NEUTRAL = 0;
        public const int TECH = 1;
        public const int UNDEAD = 2;
        public const int NATURE = 3;
        public const int MAGIC = 4;

        private static readonly Dictionary<int, RegionGeneratorData> REGION_DATA = new();

        private static Dictionary<string, GameObject> objectLookups = new();

        private static GameObject HOLO_NODE_BASE = GetGameObject("StartingIslandJunction", "Scenery/HoloNodeBase");
        private static GameObject HOVER_HOLO_NODE_BASE = GetGameObject("Shop", "Scenery/HoloDrone_HoldingPlatform_Undead");
        private static GameObject BLOCK_ICON = GetGameObject("UndeadShortcut_Exit", "HoloStopIcon");

        public static readonly int EMPTY = -1;
        public static readonly int BLANK = 0;
        public static readonly int NORTH = 1;
        public static readonly int EAST = 2;
        public static readonly int SOUTH = 4;
        public static readonly int WEST = 8;
        public static readonly int ENEMY = 16;
        public static readonly int COUNTDOWN = 32;
        public static readonly int ALL_DIRECTIONS = NORTH | EAST | SOUTH | WEST;
        private static readonly Dictionary<int, string> DIR_LOOKUP = new() {{SOUTH, "S"}, {WEST, "W"}, {NORTH, "N"}, {EAST, "E"}};
        private static readonly Dictionary<int, LookDirection> LOOK_MAPPER = new() {{SOUTH, LookDirection.South}, {NORTH, LookDirection.North}, {EAST, LookDirection.East}, {WEST, LookDirection.West}};
        private static readonly Dictionary<char, LookDirection> LOOK_CHAR_MAPPER = new () {{'S', LookDirection.South}, {'N', LookDirection.North}, {'E', LookDirection.East}, {'W', LookDirection.West}};
        
        private static IEnumerable<int> GetDirections(int compound, bool inclusive=true)
        {
            if (inclusive)
            {
                if ((compound & NORTH) != 0) yield return NORTH;
                if ((compound & EAST) != 0) yield return EAST;
                if ((compound & SOUTH) != 0) yield return SOUTH;
                if ((compound & WEST) != 0) yield return WEST;
                yield break;
            }

            yield return NORTH | WEST;
            yield return NORTH | EAST;
            yield return SOUTH | WEST;
            yield return SOUTH | EAST;
            if ((compound & NORTH) == 0) yield return NORTH;
            if ((compound & EAST) == 0) yield return EAST;
            if ((compound & SOUTH) == 0) yield return SOUTH;
            if ((compound & WEST) == 0) yield return WEST;
        }

        private static GameObject GetGameObject(string singleMapKey)
        {
            if (singleMapKey == default(string))
                return null;
            string holoMapKey = singleMapKey.Split('/')[0];
            string findPath = singleMapKey.Replace($"{holoMapKey}/", "");
            return GetGameObject(holoMapKey, findPath);
        }

        private static GameObject[] GetGameObject(string[] multiMapKey)
        {
            return multiMapKey.Select(s => GetGameObject(s)).ToArray();
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

            // Boss prefabs
            bossPrefabs.TryAdd(Opponent.Type.ArchivistBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleUndeadBoss"));
            bossPrefabs.TryAdd(Opponent.Type.PhotographerBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleNatureBoss"));
            bossPrefabs.TryAdd(Opponent.Type.TelegrapherBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleTech_1"));
            bossPrefabs.TryAdd(Opponent.Type.CanvasBoss, Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleWizardBoss"));

            // Special node prefabs
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.CardChoice, GetGameObject("StartingIslandJunction", "Nodes/CardChoiceNode3D"));
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.AddCardAbility, GetGameObject("Shop", "Nodes/ShopNode3D_AddAbility"));
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.OverclockCard, GetGameObject("Shop", "Nodes/ShopNode3D_Overclock"));
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.CreateTransformer, GetGameObject("Shop", "Nodes/ShopNode3D_Transformer"));
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.AttachGem, GetGameObject("Shop", "Nodes/ShopNode3D_AttachGem"));
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.RecycleCard, GetGameObject("NeutralWestMain_1", "Nodes/RecycleCardNode3D"));
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.BuildACard, GetGameObject("TechTower_SW", "Nodes/BuildACardNode3D"));
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.GainCurrency, GetGameObject("NatureMainPath_3", "Nodes/CurrencyGainNode3D"));

            // Special terrain prefabs
            specialTerrainPrefabs.TryAdd(HoloMapBlueprint.RIGHT_BRIDGE, new GameObject[] { GetGameObject("UndeadMainPath_4", "Scenery/HoloBridge_Entrance") });
            specialTerrainPrefabs.TryAdd(HoloMapBlueprint.LEFT_BRIDGE, new GameObject[] { GetGameObject("UndeadMainPath_3", "Scenery/HoloBridge_Entrance") });
            specialTerrainPrefabs.TryAdd(HoloMapBlueprint.FULL_BRIDGE, new GameObject[] { GetGameObject("NeutralEastMain_2", "Scenery") });
            specialTerrainPrefabs.TryAdd(HoloMapBlueprint.NORTH_BUILDING_ENTRANCE, GetGameObject(new string[] { "UndeadMainPath_4/Scenery/SM_Bld_Wall_Exterior_04", "UndeadMainPath_4/Scenery/SM_Bld_Wall_Exterior_04 (1)", "UndeadMainPath_4/Scenery/SM_Bld_Wall_Doorframe_02" }));
            specialTerrainPrefabs.TryAdd(HoloMapBlueprint.NORTH_GATEWAY, new GameObject[] { GetGameObject("NatureMainPath_2", "Scenery/HoloGateway") });
            specialTerrainPrefabs.TryAdd(HoloMapBlueprint.NORTH_CABIN, new GameObject[] { GetGameObject("TempleNature_4", "Scenery/Cabin")});

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
            neutralHoloPrefab.SetActive(false);
        }

        private static float[] MULTIPLIERS = new float[] { 0.33f, 0.66f };
        private static List<Tuple<float, float>> GetSpotsForQuadrant(int quadrant)
        {
            float minX = ((quadrant & WEST) != 0) ? -3.2f : ((quadrant & EAST) != 0) ? 1.1f : -1.1f;
            float maxX = ((quadrant & WEST) != 0) ? -1.1f : ((quadrant & EAST) != 0) ? 3.2f : 1.1f;
            float minZ = ((quadrant & NORTH) != 0) ? 1.1f : ((quadrant & SOUTH) != 0) ? -2.02f : -1.1f;
            float maxZ = ((quadrant & NORTH) != 0) ? 2.02f : ((quadrant & SOUTH) != 0) ? -1.1f : 1.1f;
            
            List<Tuple<float, float>> retval = new();
            foreach (float m in MULTIPLIERS)
                foreach (float n in MULTIPLIERS)
                    retval.Add(new(minX + m * (maxX - minX) - .025f + .05f * UnityEngine.Random.value, minZ + n * (maxZ - minZ) - .025f + .05f * UnityEngine.Random.value));

            return retval;
        }

        private static void BuildSpecialNode(HoloMapBlueprint blueprint, int regionId, Transform parent, Transform sceneryParent, float x, float z)
        {
            HoloMapSpecialNode.NodeDataType dataType = blueprint.upgrade;
            if (!specialNodePrefabs.ContainsKey(dataType))
                return;

            InfiniscryptionP03Plugin.Log.LogInfo($"Adding {blueprint.upgrade.ToString()} at {x},{z}");

            GameObject defaultNode = specialNodePrefabs[dataType];
            GameObject newNode = GameObject.Instantiate(defaultNode, parent);

            HoloMapShopNode shopNode = newNode.GetComponent<HoloMapShopNode>();
            if (shopNode != null)
            {
                // This is a shop node but we want it to behave differently than the in-game shop nodes
                Traverse shopTraverse = Traverse.Create(shopNode);
                shopTraverse.Field("cost").SetValue(8);
                shopTraverse.Field("repeatable").SetValue(false);
            }

            if (dataType == HoloMapSpecialNode.NodeDataType.GainCurrency)
            {
                string sceneryKey = REGION_DATA[regionId].objectRandoms[UnityEngine.Random.Range(0, REGION_DATA[regionId].objectRandoms.Length)];
                GameObject sceneryObject = GameObject.Instantiate(GetGameObject(sceneryKey), sceneryParent);
                sceneryObject.transform.localPosition = new Vector3(x, .1f, z);
                sceneryObject.transform.localEulerAngles = new Vector3(7.4407f, UnityEngine.Random.Range(0f, 360f), .0297f);

                // Our currency nodes are hidden...for fun!
                newNode.transform.localPosition = new Vector3(x, newNode.transform.localPosition.y, z);
                HoloMapGainCurrencyNode nodeData = newNode.GetComponent<HoloMapGainCurrencyNode>();
                Traverse nodeTraverse = Traverse.Create(nodeData);
                nodeTraverse.Field("secret").SetValue(true);
                nodeTraverse.Field("amount").SetValue(UnityEngine.Random.Range(7, 11));
            }
            else
            {
                float yVal = ((blueprint.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? newNode.transform.localPosition.y : .5f;
                newNode.transform.localPosition = new Vector3(x, yVal, z);

                yVal = ((blueprint.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? .1f : 1.33f;

                GameObject nodeBase = GameObject.Instantiate(((blueprint.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? HOLO_NODE_BASE : HOVER_HOLO_NODE_BASE, sceneryParent);
                nodeBase.transform.localPosition = new Vector3(newNode.transform.localPosition.x, yVal, newNode.transform.localPosition.z);
            }
        }

        private static GameObject BuildMapAreaPrefab(int regionId, HoloMapBlueprint bp)
        {
            InfiniscryptionP03Plugin.Log.LogInfo($"Building gameobject for [{bp.x},{bp.y}]");

            if (bp.opponent != Opponent.Type.Default)
                return GameObject.Instantiate(bossPrefabs[bp.opponent]);

            InfiniscryptionP03Plugin.Log.LogInfo($"Instantiating base object {neutralHoloPrefab}");
            GameObject area = GameObject.Instantiate(neutralHoloPrefab);
            area.name = $"ProceduralMapArea_{regionId}_{bp.x}_{bp.y})";

            InfiniscryptionP03Plugin.Log.LogInfo($"Getting nodes");
            GameObject nodes = area.transform.Find("Nodes").gameObject;

            if (DIR_LOOKUP.ContainsKey(bp.specialDirection))
            {
                InfiniscryptionP03Plugin.Log.LogInfo($"Finding arrow to destroy");
                GameObject arrowToReplace = area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[bp.specialDirection]}").gameObject;
                InfiniscryptionP03Plugin.Log.LogInfo($"Destroying arrow");
                GameObject.Destroy(arrowToReplace);
                
                InfiniscryptionP03Plugin.Log.LogInfo($"Copying arrow");
                GameObject newArrow = GameObject.Instantiate(arrowPrefabs[bp.specialDirection | ENEMY], nodes.transform);
                newArrow.name = $"MoveArea_{DIR_LOOKUP[bp.specialDirection]}";
                HoloMapNode node = newArrow.GetComponent<HoloMapNode>();
                Traverse nodeTraverse = Traverse.Create(node);
                nodeTraverse.Field("blueprintData").SetValue(Resources.Load<EncounterBlueprintData>($"data/encounterblueprints/part3/{REGION_DATA[regionId].encounters[bp.enemyIndex]}"));
                if ((bp.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) != 0)
                    nodeTraverse.Field("bridgeBattle").SetValue(true);
                
                if (bp.battleTerrainIndex > 0 && (bp.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0)
                {
                    string[] terrain = REGION_DATA[regionId].terrain[bp.battleTerrainIndex - 1];
                    nodeTraverse.Field("playerTerrain").SetValue(terrain.Take(5).Select(s => s == default(string) ? null : CardLoader.GetCardByName(s)).ToArray());
                    nodeTraverse.Field("opponentTerrain").SetValue(terrain.Skip(5).Select(s => s == default(string) ? null : CardLoader.GetCardByName(s)).ToArray());
                }
                else
                {
                    nodeTraverse.Field("playerTerrain").SetValue(new CardInfo[5]);
                    nodeTraverse.Field("opponentTerrain").SetValue(new CardInfo[5]);
                }
            }

            InfiniscryptionP03Plugin.Log.LogInfo($"Setting arrows and walls active");
            Transform scenery = area.transform.Find("Scenery");
            GameObject wall = GetGameObject(REGION_DATA[regionId].wall);
            foreach (int key in DIR_LOOKUP.Keys)
            {
                area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.SetActive((bp.arrowDirections & key) != 0);

                // Walls
                if (wall != null)
                {
                    if ((bp.arrowDirections & key) == 0)
                    {
                        GameObject wallClone = GameObject.Instantiate(wall, scenery);
                        wallClone.transform.localPosition = REGION_DATA[regionId].wallOrientations[key].Item1;
                        wallClone.transform.localEulerAngles = REGION_DATA[regionId].wallOrientations[key].Item2;
                    }
                }
            }

            InfiniscryptionP03Plugin.Log.LogInfo($"Generating random scenery");
            List<int> directions = GetDirections(bp.arrowDirections, false).ToList();
            bool firstQuadrant = true;
            while(directions.Count > 0)
            {
                int dir = directions[UnityEngine.Random.Range(0, directions.Count)];
                directions.Remove(dir);

                List<Tuple<float, float>> sceneryLocations = GetSpotsForQuadrant(dir);

                bool firstObject = true;
                while (sceneryLocations.Count > 0)
                {
                    int spIdx = UnityEngine.Random.Range(0, sceneryLocations.Count);
                    Tuple<float, float> specialLocation = sceneryLocations[spIdx];
                    sceneryLocations.RemoveAt(spIdx);

                    if (firstQuadrant && firstObject && bp.upgrade != HoloMapSpecialNode.NodeDataType.MoveArea)
                    {
                        BuildSpecialNode(bp, regionId, nodes.transform, scenery.transform, specialLocation.Item1, specialLocation.Item2);
                        firstQuadrant = false;
                        firstObject = false;
                        continue;
                    }

                    string[] scenerySource = firstObject ? REGION_DATA[regionId].objectRandoms : REGION_DATA[regionId].terrainRandoms;

                    string sceneryKey = scenerySource[UnityEngine.Random.Range(0, scenerySource.Length)];
                    GameObject sceneryObject = GameObject.Instantiate(GetGameObject(sceneryKey), scenery);
                    sceneryObject.transform.localPosition = new Vector3(specialLocation.Item1, .1f, specialLocation.Item2);
                    sceneryObject.transform.localEulerAngles = new Vector3(7.4407f, UnityEngine.Random.Range(0f, 360f), .0297f);

                    firstQuadrant = false;
                    firstObject = false;
                }
            }

            InfiniscryptionP03Plugin.Log.LogInfo($"Generating special terrain");
            foreach (int key in specialTerrainPrefabs.Keys)
                if ((bp.specialTerrain & key) != 0)
                    foreach (GameObject obj in specialTerrainPrefabs[key])
                        GameObject.Instantiate(obj, scenery);

            InfiniscryptionP03Plugin.Log.LogInfo($"Setting grid data");
            HoloMapArea areaData = area.GetComponent<HoloMapArea>();
            Traverse areaTrav = Traverse.Create(areaData);
            areaData.GridX = bp.x;
            areaData.GridY = bp.y;
            areaTrav.Field("mainColor").SetValue(REGION_DATA[regionId].mainColor);
            areaTrav.Field("lightColor").SetValue(REGION_DATA[regionId].mainColor);

            if (bp.blockedDirections != BLANK)
            {
                InfiniscryptionP03Plugin.Log.LogInfo($"Blocking directions");
                List<GameObject> blockIcons = new();
                List<LookDirection> blockedDirections = new();
                foreach (int direction in GetDirections(bp.blockedDirections, true))
                {
                    blockedDirections.Add(LOOK_MAPPER[direction]);

                    GameObject blockIcon = GameObject.Instantiate(BLOCK_ICON, area.transform);
                    blockIcons.Add(blockIcon);
                    Vector3 pos = REGION_DATA[NEUTRAL].wallOrientations[direction].Item1;
                    blockIcon.transform.localPosition = new (pos.x, 0.3f, pos.z);
                    blockIcon.transform.localEulerAngles = REGION_DATA[NEUTRAL].wallOrientations[direction].Item2;
                }

                BlockDirectionsAreaSequencer sequencer = area.AddComponent<BlockDirectionsAreaSequencer>();
                Traverse blockTraverse = Traverse.Create(sequencer);
                blockTraverse.Field("stopIcons").SetValue(blockIcons);
                blockTraverse.Field("unblockStoryEvent").SetValue(EventManagement.ALL_ZONE_ENEMIES_KILLED);
                blockTraverse.Field("blockedDirections").SetValue(blockedDirections);
                areaTrav.Field("specialSequencer").SetValue(sequencer);
            }

            // Give every node a unique id
            int nodeId = 1;
            foreach (MapNode node in area.GetComponentsInChildren<MapNode>())
                node.nodeId = nodeId++;

            area.SetActive(false);
            return area;
        }

        private static void ConnectArea(HoloMapWorldData.AreaData[,] map, HoloMapBlueprint bp)
        {
            GameObject area = map[bp.x, bp.y].prefab;

            if (area == null)
                return;

            HoloMapArea areaData = area.GetComponent<HoloMapArea>();

            // The index of DirectionNodes has to correspond to the integer value of the LookDirection enumeration
            areaData.DirectionNodes.Clear();
            for (int i = 0; i < 4; i++)
                areaData.DirectionNodes.Add(null);

            Transform nodes = area.transform.Find("Nodes");

            foreach (Transform arrow in nodes)
                if (arrow.gameObject.name.StartsWith("MoveArea"))
                    areaData.DirectionNodes[(int)LOOK_CHAR_MAPPER[arrow.gameObject.name.Last()]] = arrow.gameObject.activeSelf ? arrow.gameObject.GetComponent<MoveHoloMapAreaNode>() : null;
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

        private static HoloMapBlueprint GetAdjacentNode(this HoloMapBlueprint node, HoloMapBlueprint[,] map, int direction)
        {
            int x = direction == WEST ? node.x - 1 : direction == EAST ? node.x + 1 : node.x;
            int y = direction == NORTH ? node.y - 1 : direction == SOUTH ? node.y + 1 : node.y;
            if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1))
                return null;

            return map[x,y];
        }

        private static List<HoloMapBlueprint> GetPointOfInterestNodes(this List<HoloMapBlueprint> nodes, Func<HoloMapBlueprint, bool> filter = null)
        {
            Func<HoloMapBlueprint, bool> activeFilter = (filter == null) ? ((HoloMapBlueprint i) => true) : filter;
            List<HoloMapBlueprint> deadEndPOI = nodes.Where(activeFilter).Where(bp => bp.IsDeadEnd && bp.EligibleForUpgrade).ToList();
            if (deadEndPOI.Count > 0)
                return deadEndPOI;
            else
                return nodes.Where(activeFilter).Where(bp => bp.EligibleForUpgrade).ToList();
        }

        private static HoloMapBlueprint GetRandomPointOfInterest(this List<HoloMapBlueprint> nodes, Func<HoloMapBlueprint, bool> filter = null, int randomSeed = -1)
        {
            if (randomSeed != -1)
                UnityEngine.Random.InitState(randomSeed);

            List<HoloMapBlueprint> possibles = nodes.GetPointOfInterestNodes(filter: filter);
            return possibles.Count == 0 ? null : possibles[UnityEngine.Random.Range(0, possibles.Count)];
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
            if (map[x,y] != null && map[x,y].color <= 0)
            {
                foreach (HoloMapBlueprint adj in map.AdjacentTo(x, y))
                {
                    if (adj != null && adj.color > 0)
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

        private static void CrawlQuadrant(HoloMapBlueprint[,] map, int color)
        {
            List<HoloMapBlueprint> possibles = new();
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i,j] != null && map[i,j].color == color)
                        possibles.Add(map[i, j]);

            HoloMapBlueprint startNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
            CrawlQuadrant(map, startNode.x, startNode.y);
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
            HoloMapBlueprint next = uncrawled[UnityEngine.Random.Range(0, uncrawled.Count)];
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

        private static void DiscoverAndCreateBridge(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int region)
        {
            if (region == NATURE)
                return; // Nature doesn't have bridges

            // This is a goofy one. We're looking for a section on the map where the area could be a bridge.
            // If so, roll the dice and make a bridge
            float bridgeOdds = 0.95f;
            List<HoloMapBlueprint> bridgeNodes = nodes.Where(bp => bp.arrowDirections == (EAST | WEST)).ToList();
            while (bridgeNodes.Count > 0 && bridgeOdds > 0f)
            {
                HoloMapBlueprint bridge = bridgeNodes[UnityEngine.Random.Range(0, bridgeNodes.Count)];
                if (UnityEngine.Random.value < bridgeOdds)
                {
                    bridge.specialTerrain |= HoloMapBlueprint.FULL_BRIDGE;
                    map[bridge.x-1, bridge.y].specialTerrain |= HoloMapBlueprint.LEFT_BRIDGE;
                    map[bridge.x+1, bridge.y].specialTerrain |= HoloMapBlueprint.RIGHT_BRIDGE;
                    bridgeOdds -= 0.25f;
                }
                bridgeNodes.Remove(bridge);
            }
        }

        private static bool DiscoverAndCreateEnemyEncounter(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int region, HoloMapSpecialNode.NodeDataType reward, int color = -1)
        {
            // The goal here is to find four rooms that have only one entrance
            // Then back out to the first spot that doesn't have a choice
            // Then put an enemy encounter there
            // And put something of interest in the 

            HoloMapBlueprint enemyNode = null;
            HoloMapBlueprint rewardNode = null;
            if (color == nodes[0].color)
            {
                // If this is the region you start in, we do the work a little bit differently.
                // We walk until we find the first node with a choice
                enemyNode = nodes[0];
                rewardNode = enemyNode.GetAdjacentNode(map, enemyNode.arrowDirections);
                for (int i = 0; i < 3; i++)
                {
                    if (rewardNode.NumberOfArrows < 3)
                    {
                        int dirToEnemyNode = DirTo(rewardNode, enemyNode);
                        int dirToNextRewardNode = rewardNode.arrowDirections & ~dirToEnemyNode;
                        HoloMapBlueprint nextRewardNode = rewardNode.GetAdjacentNode(map, dirToNextRewardNode);
                        enemyNode = rewardNode;
                        rewardNode = nextRewardNode;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                rewardNode = nodes.GetRandomPointOfInterest(bp => (bp.color == color || color == -1) && bp.IsDeadEnd);
            }

            if (rewardNode != null)
            {
                enemyNode = enemyNode ?? rewardNode.GetAdjacentNode(map, rewardNode.arrowDirections);
                enemyNode.specialDirection = DirTo(enemyNode, rewardNode);
                enemyNode.enemyIndex = UnityEngine.Random.Range(0, REGION_DATA[region].encounters.Length);

                // 50% change of terrain
                if (UnityEngine.Random.value < 0.5f)
                    enemyNode.battleTerrainIndex = UnityEngine.Random.Range(0, REGION_DATA[region].terrain.Length) + 1;

                rewardNode.upgrade = reward;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void DiscoverAndCreateBossRoom(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int region)
        {
            if (region == NEUTRAL)
                return;

            // We need a room that has a blank room above it
            // And does not have the same color as the starting room
            List<HoloMapBlueprint> bossPossibles = nodes.Where(bp => (bp.arrowDirections & NORTH) == 0 && bp.y > 0 && bp.color != nodes[0].color).ToList();
            HoloMapBlueprint bossIntroRoom = bossPossibles[UnityEngine.Random.Range(0, bossPossibles.Count)];
            bossIntroRoom.specialTerrain |= HoloMapBlueprint.NORTH_BUILDING_ENTRANCE;
            bossIntroRoom.arrowDirections |= NORTH;
            bossIntroRoom.blockedDirections |= NORTH;
            bossIntroRoom.blockEvent = EventManagement.ALL_ZONE_ENEMIES_KILLED;

            HoloMapBlueprint bossRoom = new(bossIntroRoom.randomSeed + 200 * bossIntroRoom.x);
            bossRoom.x = bossIntroRoom.x;
            bossRoom.y = bossIntroRoom.y - 1;
            bossRoom.opponent = Opponent.Type.ArchivistBoss;
            bossRoom.arrowDirections |= SOUTH;
            bossRoom.color = bossIntroRoom.color;

            map[bossRoom.x, bossRoom.y] = bossRoom;
            nodes.Add(bossRoom);
        }

        private static List<HoloMapBlueprint> BuildBlueprint(int order, int region, int seed)
        {
            string blueprintKey = $"ascensionBlueprint{order}{region}";
            string savedBlueprint = ModdedSaveManager.RunState.GetValue(InfiniscryptionP03Plugin.PluginGuid, blueprintKey);

            if (savedBlueprint != default(string))
                return savedBlueprint.Split('|').Select(s => new HoloMapBlueprint(s)).ToList();

            UnityEngine.Random.InitState(seed);
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
            bpBlueprint[UnityEngine.Random.Range(2,4), UnityEngine.Random.Range(2,4)] = null;
            
            // Randomly chop a corner
            int[] corners = new int[] { 1, 4 };
            bpBlueprint[corners[UnityEngine.Random.Range(0, 2)], corners[UnityEngine.Random.Range(0, 2)]] = null;

            // Randomly chop an interior side
            x = UnityEngine.Random.Range(1, 5);
            y = x == 1 || x == 4 ? UnityEngine.Random.Range(2, 4) : corners[UnityEngine.Random.Range(0, 2)];
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
            for (int i = 1; i <= 4; i++)
                CrawlQuadrant(bpBlueprint, i);

            // Set up the connections between quadrants
            ConnectQuadrants(bpBlueprint);

            // Figure out the starting space
            HoloMapBlueprint startSpace = bpBlueprint[0, 2];
            List<HoloMapBlueprint> retval = new() { startSpace };
            for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                for (int j = 0; j < bpBlueprint.GetLength(1); j ++)
                    if (bpBlueprint[i, j] != null && bpBlueprint[i, j] != startSpace)
                        retval.Add(bpBlueprint[i, j]);

            // Do some special sequencing
            DiscoverAndCreateBridge(bpBlueprint, retval, region);
            DiscoverAndCreateBossRoom(bpBlueprint, retval, region);

            // Add four enemy encounters and rewards
            int seedForChoice = seed * 2 + 10;

            List<int> colorsWithoutEnemies = new() { 1, 2, 3, 4 };
            int numberOfEncountersAdded = 0;
            while (colorsWithoutEnemies.Count > 0)
            {
                UnityEngine.Random.InitState(seedForChoice + colorsWithoutEnemies.Count * 1000);
                int colorToUse = colorsWithoutEnemies[UnityEngine.Random.Range(0, colorsWithoutEnemies.Count)];
                HoloMapSpecialNode.NodeDataType type = colorsWithoutEnemies.Count <= 2 ? HoloMapSpecialNode.NodeDataType.AddCardAbility : REGION_DATA[region].defaultReward;
                if (DiscoverAndCreateEnemyEncounter(bpBlueprint, retval,  region, type, colorToUse))
                    numberOfEncountersAdded += 1;
                colorsWithoutEnemies.Remove(colorToUse);
            }

            int remainingEncountersToAdd = EventManagement.ENEMIES_TO_UNLOCK_BOSS - numberOfEncountersAdded;
            for (int i = 0; i < remainingEncountersToAdd; i++)
                if (DiscoverAndCreateEnemyEncounter(bpBlueprint, retval, region, REGION_DATA[region].defaultReward))
                    numberOfEncountersAdded += 1;

            InfiniscryptionP03Plugin.Log.LogInfo($"I have created {numberOfEncountersAdded} enemy encounters");

            // Add four card choice nodes
            for (int i = 1; i < 5; i++) // one for each color 1-4
                retval.GetRandomPointOfInterest(bp => bp.color == i).upgrade = HoloMapSpecialNode.NodeDataType.CardChoice;

            // Add two hidden currency nodes
            for (int i = 0; i < 2; i++)
                retval.GetRandomPointOfInterest().upgrade = HoloMapSpecialNode.NodeDataType.GainCurrency;

            // Log to the file for debug purposes
            for (int j = 0; j < bpBlueprint.GetLength(1); j++)
            {
                List<string> lines = new() { "", "", "", "", ""};
                for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                    for (int s = 0; s < lines.Count; s++)
                        lines[s] += bpBlueprint[i, j] == null ? "     " : bpBlueprint[i, j].DebugString[s];
                for (int s = 0; s < lines.Count; s++)
                    InfiniscryptionP03Plugin.Log.LogInfo(lines[s]);
            }
                

            savedBlueprint = string.Join("|", retval.Select(b => b.ToString()));
            ModdedSaveManager.RunState.SetValue(InfiniscryptionP03Plugin.PluginGuid, blueprintKey, savedBlueprint);
            SaveManager.SaveToFile();
            return retval;
        }

        public static HoloMapWorldData GetAscensionWorldbyId(string id)
        {
            HoloMapWorldData data = ScriptableObject.CreateInstance<HoloMapWorldData>();
            data.name = id;

            List<HoloMapBlueprint> blueprints = BuildBlueprint(0, UNDEAD, SaveManager.SaveFile.randomSeed);
            int xDimension = blueprints.Select(b => b.x).Max() + 1;
            int yDimension = blueprints.Select(b => b.y).Max() + 1;

            data.areas = new HoloMapWorldData.AreaData[xDimension, yDimension];

            foreach(HoloMapBlueprint bp in blueprints)
                data.areas[bp.x, bp.y] = new() { prefab = BuildMapAreaPrefab(UNDEAD, bp) };

            // The second pass creates relationships between everything
            foreach(HoloMapBlueprint bp in blueprints)
                ConnectArea(data.areas, bp);

            return data;
        }

        [HarmonyPatch(typeof(HoloMapArea), "OnAreaActive")]
        [HarmonyPrefix]
        public static void ActivateObject(ref HoloMapArea __instance)
        {
            if (SaveFile.IsAscension && !__instance.gameObject.activeSelf)
                __instance.gameObject.SetActive(true);
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

                EventManagement.NumberOfZoneEnemiesKilled = 0;
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