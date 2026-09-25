using System.Collections.Generic;
using UnityEngine;

namespace Members.CJY.Scripts
{
    [System.Serializable]
    public struct NodeData
    {
        public int column;
        public int lane;
    }
    
    [System.Serializable]
    public class NodeSaveData
    {
        public int column;
        public int lane;
        public NodeType type;
        public List<NodeData> nextNode = new List<NodeData>();
    }
    
    [System.Serializable]
    public class MapSaveData
    {
        public List<NodeSaveData> nodes = new List<NodeSaveData>();
        public NodeData currentNode;
        public bool hasData;
    }
}