using System;
using UnityEngine;
using UnityEngine.UI;

namespace Members.PDY.Scripts.Node
{
    public class UIMapNode : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _nodeImage;
        [SerializeField] private Button _nodeButton;

        [Header("Node Type Colors")]
        [SerializeField] private Color colorEnemy = Color.white;
        [SerializeField] private Color colorElite = Color.red;
        [SerializeField] private Color colorRest = Color.cyan;
        [SerializeField] private Color colorMerchant = Color.yellow;
        [SerializeField] private Color colorTreasure = Color.magenta;
        [SerializeField] private Color colorEvent = Color.blue;
        [SerializeField] private Color colorBoss = new(0.8f, 0f, 0f);
        [SerializeField] private Color colorStart = Color.green;

        public Node NodeData { get; private set; }

        public event Action<UIMapNode> OnClicked;

        public void Initialize(Node data)
        {
            NodeData = data ?? throw new ArgumentNullException(nameof(data));

            _nodeButton.onClick.RemoveListener(HandleButtonClicked);
            _nodeButton.onClick.AddListener(HandleButtonClicked);

            SetTypeColor();
        }

        public void SetState(NodeState state)
        {
            SetTypeColor();
            Color baseColor = _nodeImage.color;

            switch (state)
            {
                case NodeState.Unreachable:
                    SetColor(Color.Lerp(baseColor, Color.black, 0.6f));
                    _nodeButton.interactable = false;
                    break;

                case NodeState.Selectable:
                    SetColor(baseColor);
                    _nodeButton.interactable = true;
                    break;

                case NodeState.Current:
                    SetColor(Color.Lerp(baseColor, Color.white, 0.3f));
                    _nodeButton.interactable = false;
                    break;

                case NodeState.Visited:
                    SetColor(Color.Lerp(baseColor, Color.black, 0.3f));
                    _nodeButton.interactable = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnDestroy()
        {
            if (_nodeButton != null)
                _nodeButton.onClick.RemoveListener(HandleButtonClicked);

            OnClicked = null;
        }

        private void HandleButtonClicked()
        {
            OnClicked?.Invoke(this);
        }

        private void SetTypeColor()
        {
            Color typeColor = NodeData.type switch
            {
                NodeType.Enemy => colorEnemy,
                NodeType.Elite => colorElite,
                NodeType.Rest => colorRest,
                NodeType.Merchant => colorMerchant,
                NodeType.Treasure => colorTreasure,
                NodeType.Event => colorEvent,
                NodeType.Boss => colorBoss,
                NodeType.Start => colorStart,
                _ => Color.white
            };

            SetColor(typeColor);
        }

        private void SetColor(Color color)
        {
            color.a = 1f;
            _nodeImage.color = color;
        }
    }
}
