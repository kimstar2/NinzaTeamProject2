namespace Members.PDY.Scripts.Node
{
    using System.Collections.Generic;
    using UnityEngine;

    public enum NodeType { Enemy, Elite, Rest, Merchant, Treasure, Event, Boss, Start }
    public enum NodeState { Unreachable, Selectable, Current, Visited }
    
    public enum MapDirection { Horizontal, Vertical }

    public class Node
    {
        // 이름은 범용적으로 쓰기 위해 depth(진행 단계)와 breadth(갈래)로 생각하시면 편합니다.
        public int column; 
        public int row;    
        public NodeType type;
        public Vector2 position; 
    
        public List<Node> nextNodes = new List<Node>();
        public bool isUsed = false; 

        public Node(int c, int r)
        {
            column = c;
            row = r;
        }
    }
}