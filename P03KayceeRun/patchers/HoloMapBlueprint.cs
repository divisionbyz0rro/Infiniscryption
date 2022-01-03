using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class HoloMapBlueprint
    {
        public static int NO_SPECIAL = 0;
        public static int LEFT_BRIDGE = 1;
        public static int RIGHT_BRIDGE = 2;
        public static int FULL_BRIDGE = 4;
        public static int NORTH_BUILDING_ENTRANCE = 8;
        public static int NORTH_GATEWAY = 16;
        public static int NORTH_CABIN = 32;
        public static int LOWER_TOWER_ROOM = 64;
        public static int LANDMARKER = 128;

        public int randomSeed;
        public int x;
        public int y;
        public int arrowDirections;
        public int specialDirection;
        public int enemyType;
        public int enemyIndex;
        public Opponent.Type opponent;
        public HoloMapSpecialNode.NodeDataType upgrade;
        public int specialTerrain;
        public int blockedDirections;
        public StoryEvent blockEvent;
        public int battleTerrainIndex;

        public int distance; // used only for generation - doesn't get saved or parsed
        public int color; // used only for generation - doesn't get saved or parsed

        public override string ToString()
        {
            return $"[{randomSeed},{x},{y},{arrowDirections},{specialDirection},{enemyType},{enemyIndex},{(int)opponent},{(int)upgrade},{specialTerrain},{blockedDirections},{(int)blockEvent},{battleTerrainIndex}]";
        }

        public HoloMapBlueprint(int randomSeed) { this.randomSeed = randomSeed; this.upgrade = HoloMapSpecialNode.NodeDataType.MoveArea; }

        public HoloMapBlueprint(string parsed)
        {
            string[] split = parsed.Replace("[", "").Replace("]", "").Split(',');
            this.randomSeed = int.Parse(split[0]);
            x = int.Parse(split[1]);
            y = int.Parse(split[2]);
            arrowDirections = int.Parse(split[3]);
            specialDirection = int.Parse(split[4]);
            enemyType = int.Parse(split[5]);
            enemyIndex = int.Parse(split[6]);
            opponent = (Opponent.Type)int.Parse(split[7]);
            upgrade = (HoloMapSpecialNode.NodeDataType)int.Parse(split[8]);
            specialTerrain = int.Parse(split[9]);
            blockedDirections = int.Parse(split[10]);
            blockEvent = (StoryEvent)int.Parse(split[11]);
            battleTerrainIndex = int.Parse(split[12]);
        }

        public bool EligibleForUpgrade
        {
            get
            {
                return this.opponent == Opponent.Type.Default && this.upgrade == HoloMapNode.NodeDataType.MoveArea && (this.specialTerrain & LANDMARKER) == 0;
            }
        }

        public bool IsDeadEnd
        {
            get
            {
                return this.arrowDirections == RunBasedHoloMap.NORTH || 
                       this.arrowDirections == RunBasedHoloMap.SOUTH || 
                       this.arrowDirections == RunBasedHoloMap.WEST || 
                       this.arrowDirections == RunBasedHoloMap.EAST;
            }
        }

        public int NumberOfArrows
        {
            get
            {
                return (((this.arrowDirections & RunBasedHoloMap.NORTH) != 0) ? 1 : 0) +
                       (((this.arrowDirections & RunBasedHoloMap.SOUTH) != 0) ? 1 : 0) +
                       (((this.arrowDirections & RunBasedHoloMap.EAST) != 0) ? 1 : 0) +
                       (((this.arrowDirections & RunBasedHoloMap.WEST) != 0) ? 1 : 0);
            }
        }

        public List<string> DebugString
        {
            get
            {
                List<string> retval = new();
                string code = ((this.specialTerrain & LANDMARKER) != 0) ? "L" : this.opponent != Opponent.Type.Default ? "B" : this.specialDirection != RunBasedHoloMap.BLANK ? "E" : this.upgrade != HoloMapSpecialNode.NodeDataType.MoveArea ? "U" : " ";
                retval.Add("#---#");
                retval.Add((this.arrowDirections & RunBasedHoloMap.NORTH) != 0 ? $"|{this.color}| |" : $"|{this.color}  |");
                retval.Add("|" + ((this.arrowDirections & RunBasedHoloMap.WEST) != 0 ? $"-{code}" : $" {code}") + ((this.arrowDirections & RunBasedHoloMap.EAST) != 0 ? "-|" : " |"));
                retval.Add((this.arrowDirections & RunBasedHoloMap.SOUTH) != 0 ? "| | |" : "|   |");
                retval.Add("#---#");
                return retval;
            }
        }
    }
}