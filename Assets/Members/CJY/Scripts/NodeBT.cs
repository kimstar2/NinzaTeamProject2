using _LumenLib.PoolingSystem.Runtime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Members.CJY.Scripts
{
    public enum NodeState
    {
        Current,
        Movable,
        Preview,
        Locked
    }
    public class NodeBT : MonoBehaviour, IPoolable
    {
        private Image iconImage;
        private Button button;
        private NodeEvent nodeEvent;
        private NodeConnect node;
        private MouseEv mouseEv;
        private RectTransform rectTransform;

        [SerializeField] private PoolItemSO poolItem;
        public PoolItemSO Item => poolItem;
        public GameObject GameObject => gameObject;

        [Header("color")] 
        [SerializeField] private Color currentColor = new Color();
        [SerializeField] private Color movableColor = new  Color();
        [SerializeField] private Color previewColor = new Color();
        [SerializeField] private Color lockedColor = new Color();
        
        [Header("settings")]
        [SerializeField] private float maxScale = 1.3f;
        private Vector3 startScale;

        private void Awake()
        {
            iconImage = GetComponent<Image>();
            button = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
            startScale = rectTransform.localScale;
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
                case NodeState.Movable:
                    iconImage.color = movableColor;
                    button.interactable = true;
                    break;
                case NodeState.Preview:
                    iconImage.color = previewColor;
                    button.interactable = false;
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

        public void ScaleUp(float dur)
        {
            rectTransform.DOKill();
            rectTransform.DOScale(startScale * maxScale, dur);
        }

        public void ScaleDown(float dur)
        {
            rectTransform.DOKill();
            rectTransform.DOScale(startScale, dur);
        }

    }
}