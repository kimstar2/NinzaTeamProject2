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
        private NodeConnect node;

        [SerializeField] private PoolItemSO poolItem;
        public PoolItemSO Item => poolItem;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            iconImage = GetComponent<Image>();
        }

        public void Init(NodeConnect nodeObj, NodeEvent nodeEv)
        {
            iconImage.sprite = nodeObj.info.icon;
            nodeEvent = nodeEv;
            node = nodeObj;
        }

        public void OnClickNode()
        {
            nodeEvent.SelectNode(node);
        }
        
        public void ResetItem()
        {
            iconImage.sprite = null;
            node = null;
        }
        
    }
}