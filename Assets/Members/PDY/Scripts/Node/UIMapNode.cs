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

        [Header("Node Type Colors")]
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
            // 1. 설정된 색상을 가져옵니다.
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

            // 2. [핵심] 인스펙터에서 투명도를 낮게 설정했더라도 무조건 완전 불투명하게 만듭니다.
            Color c = _nodeImage.color;
            c.a = 1f; 
            _nodeImage.color = c;
        }

        public void SetState(NodeState state)
        {
            SetTypeColor(); 
            Color baseColor = _nodeImage.color;

            switch (state)
            {
                case NodeState.Unreachable:
                    // 갈 수 없는 곳: 검은색(Color.black)과 60% 섞어서 탁하고 어둡게 만듭니다. (알파는 무조건 1f)
                    _nodeImage.color = Color.Lerp(baseColor, Color.black, 0.6f);
                    _nodeImage.color = new Color(_nodeImage.color.r, _nodeImage.color.g, _nodeImage.color.b, 1f);
                    _nodeButton.interactable = false;
                    break;
                    
                case NodeState.Selectable:
                    // 선택 가능: 원래의 밝고 쨍한 색상 그대로
                    _nodeImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
                    _nodeButton.interactable = true;
                    break;
                    
                case NodeState.Current:
                    // 현재 위치: 하얀색(Color.white)과 살짝 섞어서 더 밝게 빛나게 강조
                    _nodeImage.color = Color.Lerp(baseColor, Color.white, 0.3f);
                    _nodeImage.color = new Color(_nodeImage.color.r, _nodeImage.color.g, _nodeImage.color.b, 1f);
                    _nodeButton.interactable = false;
                    break;
                    
                case NodeState.Visited:
                    // 방문 완료: 검은색과 30%만 섞어서 갈 수 없는 곳보단 살짝 밝게
                    _nodeImage.color = Color.Lerp(baseColor, Color.black, 0.3f);
                    _nodeImage.color = new Color(_nodeImage.color.r, _nodeImage.color.g, _nodeImage.color.b, 1f);
                    _nodeButton.interactable = false;
                    break;
            }
        }
    }
}