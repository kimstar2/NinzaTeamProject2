using System.Collections.Generic;
using UnityEngine;

namespace Members.PDY.Scripts.Node
{
    public enum NodeType { Enemy, Elite, Rest, Merchant, Treasure, Event, Boss, Start }
    public enum NodeState { Unreachable, Selectable, Current, Visited }
    public enum MapDirection { Horizontal, Vertical }

    public class Node
    {
        public int column;
        public int row;
        public NodeType type;
        public Vector2 position;
        public readonly List<Node> nextNodes = new();
        public bool isUsed;

        public Node(int column, int row)
        {
            this.column = column;
            this.row = row;
        }
    }
}
