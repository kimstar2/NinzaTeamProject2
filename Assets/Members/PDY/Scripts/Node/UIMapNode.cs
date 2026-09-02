using UnityEngine;

namespace Members.PDY.Scripts.Node
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIMapNode : MonoBehaviour
    {
        public Node nodeData;
        public Action<UIMapNode> OnClicked; 

        [Header("UI Components")]
        [SerializeField] private Image _nodeImage;
        [SerializeField] private Button _nodeButton;

        [Header("Node Type Colors (종류별 색상)")]
        public Color colorEnemy = Color.white;
        public Color colorElite = Color.red;
        public Color colorRest = Color.cyan;
        public Color colorMerchant = Color.yellow;
        public Color colorTreasure = Color.magenta;
        public Color colorEvent = Color.blue;
        public Color colorBoss = new Color(0.8f, 0f, 0f); 
        public Color colorStart = Color.green;

        public void Initialize(Node data)
        {
            nodeData = data;
            _nodeButton.onClick.AddListener(() => OnClicked?.Invoke(this));
            
            SetTypeColor(); 
        }

        private void SetTypeColor()
        {
            switch (nodeData.type)
            {
                case NodeType.Enemy: _nodeImage.color = colorEnemy; break;
                case NodeType.Elite: _nodeImage.color = colorElite; break;
                case NodeType.Rest: _nodeImage.color = colorRest; break;
                case NodeType.Merchant: _nodeImage.color = colorMerchant; break;
                case NodeType.Treasure: _nodeImage.color = colorTreasure; break;
                case NodeType.Event: _nodeImage.color = colorEvent; break;
                case NodeType.Boss: _nodeImage.color = colorBoss; break;
                case NodeType.Start: _nodeImage.color = colorStart; break;
            }
        }

        public void SetState(NodeState state)
        {
            Color baseColor = _nodeImage.color;

            switch (state)
            {
                case NodeState.Unreachable:
                    // [수정됨] 0.3f -> 0.6f (너무 어둡지 않게 밝기 상향)
                    _nodeImage.color = new Color(baseColor.r * 0.6f, baseColor.g * 0.6f, baseColor.b * 0.6f, 1f);
                    _nodeButton.interactable = false;
                    break;
                case NodeState.Selectable:
                    SetTypeColor();
                    _nodeButton.interactable = true;
                    break;
                case NodeState.Current:
                    SetTypeColor();
                    _nodeButton.interactable = false;
                    break;
                case NodeState.Visited:
                    // [수정됨] 0.3f -> 0.6f (너무 투명하지 않게 불투명도 상향)
                    SetTypeColor(); 
                    baseColor = _nodeImage.color;
                    _nodeImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.6f);
                    _nodeButton.interactable = false;
                    break;
            }
        }
    }
}