using DiskCardGame;

namespace Infiniscryption.StarterDecks.Sequences
{
    public class SpendExcessTeethNodeData : SpecialNodeData
    {
        public static SpendExcessTeethNodeData Instance = new SpendExcessTeethNodeData();

        public override string PrefabPath
		{
            // This node will never actually exist on the map.
            // We're only ever going to jump into this node when
            // the user clicks the teeth on the free teeth skull in the cabin
			get
			{
				return "Prefabs/Map/MapNodesPart1/MapNode_Empty";
			}
		}
    }
}