using System;
using UnityEngine;

namespace Members.CJY.Scripts
{
    public class NodeEvent : MonoBehaviour
    {
        public event Action<NodeType> OnNodeSelected;

        public void SelectNode(NodeType type)
        {
            OnNodeSelected?.Invoke(type);
        }
        
        // 이런식으로 알잘딱 구독해서 사용하셈
        /*private void HandleNodeSelected(NodeType type)
        {
            if (type == NodeType.Battle)
            {
                
            }
        }*/ 
    }
}