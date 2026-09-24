using System;
using UnityEngine;

namespace Members.CJY.Scripts
{
    public class NodeEvent : MonoBehaviour
    {
        public event Action<NodeConnect> OnNodeSelected;

        public void SelectNode(NodeConnect node)
        {
            OnNodeSelected?.Invoke(node);
        }
        
        // 이런식으로 알잘딱 구독해서 사용하셈
        /*private void HandleNodeSelected(NodeConnect node)
        {
            if (node.info.type == NodeType.Battle)
            {
                
            }
        } */
    }
}