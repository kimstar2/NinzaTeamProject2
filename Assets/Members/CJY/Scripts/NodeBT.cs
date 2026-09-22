using System;
using _LumenLib.PoolingSystem.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Members.CJY.Scripts
{
    public class NodeBT : MonoBehaviour, IPoolable
    {
        private Image iconImage;
        private NodeEvent nodeEvent;
        private NodeInfoSO nodeInfo;

        [SerializeField] private PoolItemSO poolItem;
        public PoolItemSO Item => poolItem;
        public GameObject GameObject => gameObject;

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
        
        public void ResetItem()
        {
            iconImage.sprite = null;
            nodeInfo = null;
        }
        
    }
}