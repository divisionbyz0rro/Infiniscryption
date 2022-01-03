using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Saves;
using System;
using Infiniscryption.P03KayceeRun.Sequences;

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
        private static readonly Dictionary<string, LookDirection> LOOK_NAME_MAPPER = new () 
        {
            {"MoveArea_S", LookDirection.South}, {"MoveArea_N", LookDirection.North}, {"MoveArea_E", LookDirection.East}, {"MoveArea_W", LookDirection.West},
            {"MoveArea_W (NORTH)", LookDirection.North}, {"MoveArea_W (SOUTH)", LookDirection.South}, {"MoveArea_E (NORTH)", LookDirection.North}, {"MoveArea_E (SOUTH)", LookDirection.South}
        };
        
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
            specialNodePrefabs.TryAdd(HoloMapSpecialNode.NodeDataType.ModifySideDeckConduit, GetGameObject("TechEntrance", "Nodes/ModifySideDeckNode3D"));
            specialNodePrefabs.TryAdd(TradeChipsNodeData.TradeChipsForCards, GetDraftNode());

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

        private static GameObject GetDraftNode()
        {
            GameObject baseObject = GetGameObject("WizardMainPath_3", "Nodes/CardChoiceNode3D");
            GameObject retval = GameObject.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponent<HoloMapSpecialNode>();
            Traverse nodeTraverse = Traverse.Create(nodeData);
            nodeTraverse.Field("nodeType").SetValue(TradeChipsNodeData.TradeChipsForCards);
            nodeTraverse.Field("repeatable").SetValue(true);

            // Add an 'active only if' flag
            //ActiveIfStoryFlag flag = retval.AddComponent<ActiveIfStoryFlag>();
            //Traverse.Create(flag).Field("storyFlag").SetValue(EventManagement.HAS_DRAFT_TOKEN);

            return retval;
        }

        private static void BuildSpecialNode(HoloMapBlueprint blueprint, int regionId, Transform parent, Transform sceneryParent, float x, float z)
        {
            BuildSpecialNode(blueprint.upgrade, blueprint.specialTerrain, regionId, parent, sceneryParent, x, z);
        }

        private static void BuildSpecialNode(HoloMapSpecialNode.NodeDataType dataType, int specialTerrain, int regionId, Transform parent, Transform sceneryParent, float x, float z)
        {
            if (!specialNodePrefabs.ContainsKey(dataType))
                return;

            InfiniscryptionP03Plugin.Log.LogInfo($"Adding {dataType.ToString()} at {x},{z}");

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
                float yVal = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? newNode.transform.localPosition.y : .5f;
                newNode.transform.localPosition = new Vector3(x, yVal, z);

                yVal = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? .1f : 1.33f;

                GameObject nodeBase = GameObject.Instantiate(((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? HOLO_NODE_BASE : HOVER_HOLO_NODE_BASE, sceneryParent);
                nodeBase.transform.localPosition = new Vector3(newNode.transform.localPosition.x, yVal, newNode.transform.localPosition.z);
            }
        }

        private static GameObject BuildHubNode()
        {
            GameObject hubNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandWaypoint");
            GameObject retval = GameObject.Instantiate(hubNodeBase);
            
            // We don't want the bottom arrow
            retval.transform.Find("Nodes/MoveArea_S").gameObject.SetActive(false);
            retval.transform.Find("Nodes/CurrencyGainNode3D").gameObject.SetActive(false);

            // We need to set a conditional up arrow
            HoloMapArea areaData = retval.GetComponent<HoloMapArea>();
            Traverse areaTrav = Traverse.Create(areaData);
            BlockDirections(retval, areaTrav, NORTH, EventManagement.ALL_BOSSES_KILLED);

            // We need to add the draft node
            Transform nodes = retval.transform.Find("Nodes");
            Transform scenery = retval.transform.Find("Scenery");
            BuildSpecialNode(TradeChipsNodeData.TradeChipsForCards, 0, NEUTRAL, nodes, scenery, 1.5f, 0f);
            
            retval.SetActive(false);
            return retval;
        }

        private static void BlockDirections(GameObject area, Traverse areaTrav, int blocked, StoryEvent storyEvent)
        {
            InfiniscryptionP03Plugin.Log.LogInfo($"Blocking directions");
            List<GameObject> blockIcons = new();
            List<LookDirection> blockedDirections = new();
            foreach (int direction in GetDirections(blocked, true))
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
            blockTraverse.Field("unblockStoryEvent").SetValue(storyEvent);
            blockTraverse.Field("blockedDirections").SetValue(blockedDirections);
            areaTrav.Field("specialSequencer").SetValue(sequencer);
        }

        private static GameObject BuildLowerTowerRoom()
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleWizardEntrance");
            GameObject retval = GameObject.Instantiate(prefab);

            // Kill the shop node:
            retval.transform.Find("Nodes/MoveArea_E").gameObject.SetActive(false);

            // Kill the open door
            retval.transform.Find("Scenery/Doorframe").gameObject.SetActive(false);

            // Fill the open door with a clone of the wall piece
            GameObject rightWall = retval.transform.Find("Scenery/RightWall").gameObject;
            GameObject newRightWall = GameObject.Instantiate(rightWall, rightWall.transform.parent);
            newRightWall.transform.localPosition = new Vector3(rightWall.transform.localPosition.x, rightWall.transform.localPosition.y, 0.45f);

            retval.SetActive(false);
            return retval;
        }

        private static GameObject BuildMapAreaPrefab(int regionId, HoloMapBlueprint bp)
        {
            InfiniscryptionP03Plugin.Log.LogInfo($"Building gameobject for [{bp.x},{bp.y}]");

            if (bp.opponent != Opponent.Type.Default)
            {
                GameObject retval = GameObject.Instantiate(bossPrefabs[bp.opponent]);
                if (bp.opponent == Opponent.Type.TelegrapherBoss)
                {
                    retval.transform.Find("Nodes/MoveArea_E").gameObject.SetActive(false);
                    retval.transform.Find("Nodes/MoveArea_W").gameObject.SetActive(false);
                }
                return retval;
            }

            if (bp.upgrade == HoloMapSpecialNode.NodeDataType.FastTravel)
                return BuildHubNode();

            if (bp.specialTerrain == HoloMapBlueprint.LOWER_TOWER_ROOM)
                return BuildLowerTowerRoom();

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

            // Add the landmarks if necessary
            if ((bp.specialTerrain & HoloMapBlueprint.LANDMARKER) != 0)
                foreach (string objId in REGION_DATA[regionId].landmarks[bp.color - 1])
                    GameObject.Instantiate(GetGameObject(objId), scenery);

            // Add the normal scenery
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

                    if (firstObject && (bp.specialTerrain & HoloMapBlueprint.LANDMARKER) != 0)
                    {
                        firstObject = false;
                        continue;
                    }

                    string[] scenerySource = firstObject ? REGION_DATA[regionId].objectRandoms : REGION_DATA[regionId].terrainRandoms;

                    firstQuadrant = false;
                    firstObject = false;

                    if (scenerySource.Length == 0)
                        continue;

                    string sceneryKey = scenerySource[UnityEngine.Random.Range(0, scenerySource.Length)];
                    GameObject sceneryObject = GameObject.Instantiate(GetGameObject(sceneryKey), scenery);
                    sceneryObject.transform.localPosition = new Vector3(specialLocation.Item1, sceneryObject.transform.localPosition.y, specialLocation.Item2);
                    sceneryObject.transform.localEulerAngles = new Vector3(sceneryObject.transform.localEulerAngles.x, UnityEngine.Random.Range(0f, 360f), sceneryObject.transform.localEulerAngles.z);
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
                BlockDirections(area, areaTrav, bp.blockedDirections, EventManagement.ALL_ZONE_ENEMIES_KILLED);

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
                    areaData.DirectionNodes[(int)LOOK_NAME_MAPPER[arrow.gameObject.name]] = arrow.gameObject.activeSelf ? arrow.gameObject.GetComponent<MoveHoloMapAreaNode>() : null;
        }

        public static string GetAscensionWorldID(int regionCode)
        {
            return $"ascension_{P03AscensionSaveData.CompletedZones.Count}_{regionCode}";
        }

        public static Tuple<int, int> GetStartingSpace(int regionCode)
        {
            return regionCode == NEUTRAL ? new(0, 0) : new(0, 2);
        }

        public static HoloMapWorldData GetAscensionWorldbyId(string id)
        {
            InfiniscryptionP03Plugin.Log.LogInfo($"Getting world for {id}");

            HoloMapWorldData data = ScriptableObject.CreateInstance<HoloMapWorldData>();
            data.name = id;

            string[] idSplit = id.Split('_');
            int regionCount = int.Parse(idSplit[1]);
            int regionCode = int.Parse(idSplit[2]);

            List<HoloMapBlueprint> blueprints = BuildBlueprint(regionCount, regionCode, P03AscensionSaveData.RandomSeed);
            int xDimension = blueprints.Select(b => b.x).Max() + 1;
            int yDimension = blueprints.Select(b => b.y).Max() + 1;

            data.areas = new HoloMapWorldData.AreaData[xDimension, yDimension];

            foreach(HoloMapBlueprint bp in blueprints)
                data.areas[bp.x, bp.y] = new() { prefab = BuildMapAreaPrefab(regionCode, bp) };

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

        public static void ClearWorldData()
        {
            worldDataCache.Clear();
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
                worldDataCache.Add(id, GetAscensionWorldbyId(id));
                __result = worldDataCache[id];
                return false;
            }                
            return true;        
        }
    }
}