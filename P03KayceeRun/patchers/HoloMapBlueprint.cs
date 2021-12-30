using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class HoloMapBlueprint
    {
        public static readonly Dictionary<int, HoloMapSpecialNode.NodeDataType> REGIONAL_NODES = new()
        {
            { RunBasedHoloMap.NEUTRAL, HoloMapSpecialNode.NodeDataType.AddCardAbility },
            { RunBasedHoloMap.NATURE, HoloMapSpecialNode.NodeDataType.CreateTransformer },
            { RunBasedHoloMap.UNDEAD, HoloMapSpecialNode.NodeDataType.OverclockCard },
            { RunBasedHoloMap.MAGIC, HoloMapSpecialNode.NodeDataType.AttachGem },
            { RunBasedHoloMap.TECH, HoloMapSpecialNode.NodeDataType.BuildACard }
        };

        public static int NO_SPECIAL = 0;
        public static int LEFT_BRIDGE = 1;
        public static int RIGHT_BRIDGE = 2;
        public static int FULL_BRIDGE = 4;
        public static int NORTH_BUILDING_ENTRANCE = 8;
        public static int NORTH_GATEWAY = 16;

        public int randomSeed;
        public int x;
        public int y;
        public int arrowDirections;
        public int enemyDirection;
        public int enemyType;
        public int enemyIndex;
        public Opponent.Type opponent;
        public HoloMapSpecialNode.NodeDataType upgrade;
        public int specialTerrain;

        public int distance; // used only for generation - doesn't get saved or parsed
        public int color; // used only for generation - doesn't get saved or parsed

        public override string ToString()
        {
            return $"[{randomSeed},{x},{y},{arrowDirections},{enemyDirection},{enemyType},{enemyIndex},{(int)opponent},{(int)upgrade},{specialTerrain}]";
        }

        public HoloMapBlueprint(int randomSeed) { this.randomSeed = randomSeed; this.upgrade = HoloMapSpecialNode.NodeDataType.MoveArea; }

        public HoloMapBlueprint(string parsed)
        {
            string[] split = parsed.Replace("[", "").Replace("]", "").Split(',');
            this.randomSeed = int.Parse(split[0]);
            x = int.Parse(split[1]);
            y = int.Parse(split[2]);
            arrowDirections = int.Parse(split[3]);
            enemyDirection = int.Parse(split[4]);
            enemyType = int.Parse(split[5]);
            enemyIndex = int.Parse(split[6]);
            opponent = (Opponent.Type)int.Parse(split[7]);
            upgrade = (HoloMapSpecialNode.NodeDataType)int.Parse(split[8]);
            specialTerrain = int.Parse(split[9]);
        }
    }
}