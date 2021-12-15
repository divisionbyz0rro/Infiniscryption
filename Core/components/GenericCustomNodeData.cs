using System;
using DiskCardGame;

namespace Infiniscryption.Core.Components
{
    public sealed class GenericCustomNodeData : SpecialNodeData
    {
        public GenericCustomNodeData(Type type, string prefabPathOverride)
        {
            this.guid = type.FullName;
            this.prefabPathOverride = prefabPathOverride;
            this.tag = string.Empty;
        }

        public string prefabPathOverride;

        public string guid;

        public string tag;

        public override string PrefabPath
		{
			get
			{
                // This syntax works with our custom resource bank loaders
				return $"Prefabs/Map/MapNodesPart1/MapNode_BuyPelts@{prefabPathOverride}";
			}
		}
    }
}