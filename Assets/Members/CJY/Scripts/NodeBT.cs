using _LumenLib.PoolingSystem.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Members.CJY.Scripts
{
    public enum NodeState
    {
        Current,
        Moveable,
        Locked
    }
    public class NodeBT : MonoBehaviour, IPoolable
    {
        private Image iconImage;
        private Button button;
        private NodeEvent nodeEvent;
        private NodeConnect node;

        [SerializeField] private PoolItemSO poolItem;
        public PoolItemSO Item => poolItem;
        public GameObject GameObject => gameObject;

        [Header("color")] 
        [SerializeField] private Color currentColor = new Color();
        [SerializeField] private Color movableColor = Color.white;
        [SerializeField] private Color lockedColor = new Color();

        private void Awake()
        {
            iconImage = GetComponent<Image>();
            button = GetComponent<Button>();
        }

        public void Init(NodeConnect nodeObj, NodeEvent nodeEv)
        {
            iconImage.sprite = nodeObj.info.icon;
            nodeEvent = nodeEv;
            node = nodeObj;
        }

        public void SetVisual(NodeState state)
        {
            switch (state)
            {
                case NodeState.Current:
                    iconImage.color = currentColor;
                    button.interactable = false;
                    break;
                case NodeState.Moveable:
                    iconImage.color = movableColor;
                    button.interactable = true;
                    break;
                case NodeState.Locked:
                    iconImage.color = lockedColor;
                    button.interactable = false;
                    break;
            }
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