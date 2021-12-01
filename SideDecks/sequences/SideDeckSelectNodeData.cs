using DiskCardGame;

namespace Infiniscryption.SideDecks.Sequences
{
    public class SideDeckSelectNodeData : SpecialNodeData
    {
        public static SideDeckSelectNodeData Instance = new SideDeckSelectNodeData();

        public override string PrefabPath
		{
			get
			{
                // This syntax works with our custom resource bank loaders
				return "Prefabs/Map/MapNodesPart1/MapNode_BuyPelts@animated_sidedeck";
			}
		}
    }
}