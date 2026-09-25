using UnityEngine;

namespace Members.CJY.Scripts
{
    public enum NodeType
    {
        Start,
        Battle,
        Elite,
        Shop,
        Event,
        Rest,
        Boss
    }
    
    [CreateAssetMenu(fileName = "NodeInfo", menuName = "CJY/NodeInfo", order = 0)]
    public class NodeInfoSO : ScriptableObject
    {
        public string typeName;
        public NodeType type;
        public Sprite icon;
        [Range(0, 100)] public float percent;
    }
}