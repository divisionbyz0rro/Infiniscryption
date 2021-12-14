using System.Collections;

namespace Infiniscryption.Core.Components
{
    public interface ICustomNodeSequence
    {
        public IEnumerator ExecuteCustomSequence(GenericCustomNodeData nodeData);
    }
}