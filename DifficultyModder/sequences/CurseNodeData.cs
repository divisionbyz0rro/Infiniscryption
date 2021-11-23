using DiskCardGame;

namespace Infiniscryption.Curses.Sequences
{
    public class CurseNodeData : SpecialNodeData
    {
        public static CurseNodeData Instance = new CurseNodeData();

        public override string PrefabPath
		{
			get
			{
                // This syntax works with our custom resource bank loaders
				return "Prefabs/Map/MapNodesPart1/MapNode_BuyPelts@animated_cursenode";
			}
		}
    }
}