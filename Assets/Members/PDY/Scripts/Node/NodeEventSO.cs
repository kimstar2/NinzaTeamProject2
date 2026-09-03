using UnityEngine;

namespace Members.PDY.Scripts.Node
{
    public abstract class NodeEventSO : ScriptableObject
    {
        public abstract void ExecuteEvent(Node nodeData);
    }
}
