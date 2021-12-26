using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class HoloMapBlueprint
    {
        public enum UpgradeType
        {
            None = 0,
            AddCardAbility = 1,
            BuildACard = 2,
            CardChoices = 3,
            CreateTransformer = 4,
            ModifySideDeck = 5, // This should only happen at the beginning of a zone and only if necessary
            OverclockCard = 6,
            RecycleCard = 7,
            UnlockPart3Item = 8,
            GainCurrency
        }

        public int randomSeed;
        public int x;
        public int y;
        public int arrowDirections;
        public int enemyDirection;
        public int enemyType;
        public int enemyIndex;
        public Opponent.Type opponent;

        public int distance; // used only for generation - doesn't get saved or parsed
        public int color; // used only for generation - doesn't get saved or parsed

        public override string ToString()
        {
            return $"[{randomSeed},{x},{y},{arrowDirections},{enemyDirection},{enemyType},{enemyIndex},{(int)opponent}]";
        }

        public HoloMapBlueprint(int randomSeed) { this.randomSeed = randomSeed; }

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
        }
    }
}