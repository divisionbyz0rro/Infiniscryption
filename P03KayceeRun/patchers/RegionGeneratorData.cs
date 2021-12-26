using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class RegionGeneratorData
    {
        public int regionCode;

        public string[] encounters;

        public string[] terrainRandoms;

        public string[] objectRandoms;

        public string wall;

        public Dictionary<int, Tuple<Vector3, Vector3>> wallOrientations;

        public Color lightColor;

        public Color mainColor;

        public RegionGeneratorData(int regionCode)
        {
            this.regionCode = regionCode;
            switch (regionCode)
            {
                case RunBasedHoloMap.NEUTRAL:
                    this.encounters = new string[] { "neutral_alarmbots", "neutral_bombsandshields", "neutral_bridgebattle", "neutral_minecarts", "neutral_sentrywall", "neutral_swapbots"};
                    this.terrainRandoms = new string[] { "NeutralEastMain_4/Scenery/HoloGrass_Small (1)", "NeutralEastMain_4/Scenery/HoloGrass_Patch (1)" };
                    this.objectRandoms = new string[] { "StartingIslandBattery/Scenery/HoloMeter","StartingIslandBattery/Scenery/HoloDrone_Broken","StartingIslandJunction/Scenery/HoloBotPiece_Leg","StartingIslandJunction/Scenery/HoloBotPiece_Head","NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloPowerPoll_1" };
                    this.wall = "NeutralEastMain_4/Scenery/HoloWall (1)";
                    this.lightColor = new Color(0.5802f, 0.8996f, 1f);
                    this.mainColor = new Color(0f, 0.5157f, 0.6792f);
                    wallOrientations = new();
                    wallOrientations.Add(RunBasedHoloMap.NORTH, new(new(.08f, -.18f, 2.02f), new(7.4407f, 179.305f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.SOUTH, new(new(.08f, -.18f, -2.02f), new(7.4407f, 359.2266f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.WEST, new(new(-3.2f, -.18f, -.4f), new(7.4407f, 89.603f, .0297f)));
                    wallOrientations.Add(RunBasedHoloMap.EAST, new(new(3.2f, -.18f, -.4f), new(7.4407f, 270.359f, .0297f)));
                    break;
                case RunBasedHoloMap.MAGIC:
                    this.encounters = new string[] { "wizard_bigripper", "wizard_gemexploder", "wizard_shieldgems" };
                    break;
                case RunBasedHoloMap.NATURE:
                    this.encounters = new string[] { "nature_battransformers", "nature_beartransformers", "nature_hounds" };
                    break;
                case RunBasedHoloMap.TECH:
                    this.encounters = new string[] { "tech_attackconduits", "tech_giftcells", "tech_splintercells" };
                    break;
                case RunBasedHoloMap.UNDEAD:
                    this.encounters = new string[] { "undead_bomblatchers", "undead_shieldlatchers", "undead_skeleswarm" };
                    break;
                default:
                    break;
            }
        }
    }
}