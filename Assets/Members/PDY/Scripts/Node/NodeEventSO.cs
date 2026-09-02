namespace Members.PDY.Scripts.Node
{
    using UnityEngine;
    
    public abstract class NodeEventSO : ScriptableObject
    {
        public abstract void ExecuteEvent(Node nodeData);
    }
}