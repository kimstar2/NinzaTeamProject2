using System;
using UnityEngine;
using UnityEngine.UI;

namespace Members.CJY.Scripts
{
    public class NodeBT : MonoBehaviour
    {
        private Image iconImage;
        private NodeEvent nodeEvent;
        private NodeInfoSO nodeInfo;

        private void Awake()
        {
            iconImage = GetComponent<Image>();
        }

        public void Init(NodeInfoSO info, NodeEvent nodeEv)
        {
            iconImage.sprite = info.icon;
            nodeEvent = nodeEv;
            nodeInfo = info;
        }

        public void OnClickNode()
        {
            nodeEvent.SelectNode(nodeInfo.type);
        }
        
    }
}