using System;
using System.Collections.Generic;
using UnityEngine;

namespace Members.PDY.Scripts.Node
{
    [Serializable]
    public struct NodeEventMapping
    {
        public NodeType type;
        public NodeEventSO eventSO;
    }

    public class MapManager : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private MapDirection mapDirection = MapDirection.Horizontal;
        [SerializeField] private int columns = 15;
        [SerializeField] private int rows = 7;
        [SerializeField] private int startingPaths = 4;
        [SerializeField] private float spacingX = 150f;
        [SerializeField] private float spacingY = 100f;

        [Header("Jitter Settings")]
        [SerializeField] private float jitterX = 20f;
        [SerializeField] private float jitterY = 25f;

        [Header("UI References")]
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private float padding = 200f;

        [Header("Node Events")]
        [SerializeField] private List<NodeEventMapping> eventMappings = new();

        private readonly Dictionary<NodeType, NodeEventSO> _eventByType = new();
        private readonly Dictionary<Node, UIMapNode> _viewByNode = new();

        private UIMapNode _currentNode;

        private void Start()
        {
            if (!TryValidateConfiguration())
            {
                enabled = false;
                return;
            }

            BuildEventLookup();

            List<List<Node>> mapData = MapAlgorithm.GenerateMapData(
                columns,
                rows,
                startingPaths,
                spacingX,
                spacingY,
                mapDirection,
                jitterX,
                jitterY);

            AdjustContentSize();
            RenderMap(mapData);
        }

        private void OnDestroy()
        {
            foreach (UIMapNode nodeView in _viewByNode.Values)
            {
                if (nodeView != null)
                    nodeView.OnClicked -= HandleNodeClicked;
            }

            _viewByNode.Clear();
        }

        private bool TryValidateConfiguration()
        {
            if (columns < 2 || rows < 1)
            {
                Debug.LogError("Map requires at least 2 columns and 1 row.", this);
                return false;
            }

            if (mapContainer == null || nodePrefab == null || linePrefab == null)
            {
                Debug.LogError("MapManager is missing a map container, node prefab, or line prefab.", this);
                return false;
            }

            if (nodePrefab.GetComponent<UIMapNode>() == null)
            {
                Debug.LogError("The node prefab must contain a UIMapNode component.", nodePrefab);
                return false;
            }

            if (linePrefab.GetComponent<UIMapLine>() == null)
            {
                Debug.LogError("The line prefab must contain a UIMapLine component.", linePrefab);
                return false;
            }

            return true;
        }

        private void BuildEventLookup()
        {
            _eventByType.Clear();

            foreach (NodeEventMapping mapping in eventMappings)
            {
                if (mapping.eventSO == null)
                {
                    Debug.LogWarning($"[{mapping.type}] 타입의 이벤트 SO가 비어 있습니다.", this);
                    continue;
                }

                if (!_eventByType.TryAdd(mapping.type, mapping.eventSO))
                    Debug.LogWarning($"[{mapping.type}] 타입의 이벤트가 중복되어 첫 번째 설정을 사용합니다.", this);
            }
        }

        private void AdjustContentSize()
        {
            float totalWidth;
            float totalHeight;

            if (mapDirection == MapDirection.Horizontal)
            {
                totalWidth = (columns - 1) * spacingX + padding;
                totalHeight = (rows - 1) * spacingY + padding;
            }
            else
            {
                totalWidth = (rows - 1) * spacingX + padding;
                totalHeight = (columns - 1) * spacingY + padding;
            }

            mapContainer.sizeDelta = new Vector2(totalWidth, totalHeight);
        }

        private void RenderMap(List<List<Node>> mapData)
        {
            RenderLines(mapData);
            RenderNodes(mapData);
            InitializeStartNode(mapData[0][rows / 2]);
        }

        private void RenderLines(IEnumerable<List<Node>> mapData)
        {
            foreach (List<Node> column in mapData)
            {
                foreach (Node node in column)
                {
                    if (!node.isUsed)
                        continue;

                    foreach (Node nextNode in node.nextNodes)
                    {
                        UIMapLine lineView = Instantiate(linePrefab, mapContainer).GetComponent<UIMapLine>();
                        PreserveLegacyLineOpacity(lineView);
                        lineView.DrawLine(node.position, nextNode.position, mapDirection);
                    }
                }
            }
        }

        private static void PreserveLegacyLineOpacity(UIMapLine lineView)
        {
            Color lineColor = lineView.color;
            float inverseAlpha = 1f - lineColor.a;
            lineColor.a = 1f - inverseAlpha * inverseAlpha;
            lineView.color = lineColor;
        }

        private void RenderNodes(IEnumerable<List<Node>> mapData)
        {
            foreach (List<Node> column in mapData)
            {
                foreach (Node node in column)
                {
                    if (!node.isUsed)
                        continue;

                    GameObject nodeObject = Instantiate(nodePrefab, mapContainer);
                    nodeObject.GetComponent<RectTransform>().anchoredPosition = node.position;

                    UIMapNode nodeView = nodeObject.GetComponent<UIMapNode>();
                    nodeView.Initialize(node);
                    nodeView.SetState(NodeState.Unreachable);
                    nodeView.OnClicked += HandleNodeClicked;

                    _viewByNode.Add(node, nodeView);
                }
            }
        }

        private void InitializeStartNode(Node startNode)
        {
            if (!_viewByNode.TryGetValue(startNode, out UIMapNode startNodeView))
            {
                Debug.LogError("생성된 맵에서 시작 노드 UI를 찾을 수 없습니다.", this);
                return;
            }

            _currentNode = startNodeView;
            _currentNode.SetState(NodeState.Current);
            UpdateSelectableNodes(startNode);
        }

        private void HandleNodeClicked(UIMapNode clickedNode)
        {
            if (_currentNode == null || clickedNode == null)
                return;

            if (!_currentNode.NodeData.nextNodes.Contains(clickedNode.NodeData))
            {
                Debug.LogWarning("현재 노드에서 이동할 수 없는 노드입니다.", clickedNode);
                return;
            }

            MarkPreviousChoicesUnavailable(clickedNode);

            _currentNode = clickedNode;
            _currentNode.SetState(NodeState.Current);
            UpdateSelectableNodes(_currentNode.NodeData);
            TriggerNodeEvent(_currentNode.NodeData);
        }

        private void MarkPreviousChoicesUnavailable(UIMapNode selectedNode)
        {
            _currentNode.SetState(NodeState.Visited);

            foreach (Node nextNode in _currentNode.NodeData.nextNodes)
            {
                if (nextNode != selectedNode.NodeData && _viewByNode.TryGetValue(nextNode, out UIMapNode sibling))
                    sibling.SetState(NodeState.Unreachable);
            }
        }

        private void UpdateSelectableNodes(Node node)
        {
            foreach (Node nextNode in node.nextNodes)
            {
                if (_viewByNode.TryGetValue(nextNode, out UIMapNode nextNodeView))
                    nextNodeView.SetState(NodeState.Selectable);
            }
        }

        private void TriggerNodeEvent(Node node)
        {
            if (_eventByType.TryGetValue(node.type, out NodeEventSO nodeEvent))
            {
                nodeEvent.ExecuteEvent(node);
                return;
            }

            Debug.LogWarning($"[{node.type}] 타입에 연결된 이벤트 SO가 인스펙터에 없습니다!");
        }
    }
}
