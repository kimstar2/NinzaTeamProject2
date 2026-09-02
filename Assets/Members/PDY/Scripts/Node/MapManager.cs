using UnityEngine;
using UnityEngine.SceneManagement;

namespace Members.PDY.Scripts.Node
{
    using System.Collections.Generic;
    using UnityEngine;

    // [추가됨] 인스펙터에서 노드 타입과 SO를 짝지어줄 수 있는 구조체
    [System.Serializable]
    public struct NodeEventMapping
    {
        public NodeType type;
        public NodeEventSO eventSO;
    }

    public class MapManager : MonoBehaviour
    {
        // ... (기존 설정 변수들 동일) ...
        [Header("Map Settings")]
        public MapDirection mapDirection = MapDirection.Horizontal; 
        public int columns = 15; 
        public int rows = 7;     
        public int startingPaths = 4;
        public float spacingX = 150f; 
        public float spacingY = 100f;

        [Header("Jitter Settings")]
        public float jitterX = 20f; 
        public float jitterY = 25f;

        [Header("UI References")]
        public RectTransform mapContainer; 
        public GameObject nodePrefab;      
        public GameObject linePrefab;      
        public float padding = 200f; 

        // [추가됨] 에디터에서 연결할 이벤트 리스트
        [Header("Node Events")]
        public List<NodeEventMapping> eventMappings;
        
        // 리스트를 빠르게 검색하기 위한 딕셔너리
        private Dictionary<NodeType, NodeEventSO> eventDictionary = new Dictionary<NodeType, NodeEventSO>();

        private List<UIMapNode> allUINodes = new List<UIMapNode>();
        private UIMapNode currentNode;

        void Start()
        {
            // 1. 인스펙터에서 설정한 리스트를 딕셔너리로 변환하여 초기화
            foreach (var mapping in eventMappings)
            {
                if (!eventDictionary.ContainsKey(mapping.type))
                {
                    eventDictionary.Add(mapping.type, mapping.eventSO);
                }
            }

            List<List<Node>> mapData = MapAlgorithm.GenerateMapData(columns, rows, startingPaths, spacingX, spacingY, mapDirection, jitterX, jitterY);
            AdjustContentSize();
            RenderMapUI(mapData);
        }

        // ... (AdjustContentSize, RenderMapUI 함수 등은 기존과 동일하므로 생략) ...
        private void AdjustContentSize()
        {
            float totalWidth = 0f;
            float totalHeight = 0f;

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

        void RenderMapUI(List<List<Node>> mapData)
        {
            // 1. [가장 먼저] 선(Line)들을 전부 다 생성합니다.
            // 먼저 생성된 오브젝트는 하이어라키 위쪽에 위치하게 되어, 화면상에서는 가장 밑바닥(배경)에 깔리게 됩니다.
            foreach (var col in mapData)
            {
                foreach (var node in col)
                {
                    if (node.isUsed)
                    {
                        foreach (var nextData in node.nextNodes)
                        {
                            GameObject lineObj = Instantiate(linePrefab, mapContainer);
                            UIMapLine lineUI = lineObj.GetComponent<UIMapLine>();
                            // UI 노드가 없어도, 순수 데이터(node.position)만으로 선을 그릴 수 있습니다.
                            lineUI.DrawLine(node.position, nextData.position, mapDirection);
                        }
                    }
                }
            }
            foreach (var col in mapData)
            {
                foreach (var node in col)
                {
                    if (node.isUsed)
                    {
                        GameObject obj = Instantiate(nodePrefab, mapContainer);
                        obj.GetComponent<RectTransform>().anchoredPosition = node.position;

                        UIMapNode uiNode = obj.GetComponent<UIMapNode>();
                        uiNode.Initialize(node);
                        uiNode.SetState(NodeState.Unreachable);
                    
                        uiNode.OnClicked += HandleNodeClicked; 
                        allUINodes.Add(uiNode);
                    }
                }
            }

            UIMapNode startUINode = allUINodes.Find(n => n.nodeData.type == NodeType.Start);
            if (startUINode != null)
            {
                currentNode = startUINode;
                startUINode.SetState(NodeState.Current);
                UpdateSelectableNodes(startUINode.nodeData);
            }

            foreach (var uiNode in allUINodes)
            {
                foreach (var nextData in uiNode.nodeData.nextNodes)
                {
                    GameObject lineObj = Instantiate(linePrefab, mapContainer);
                    lineObj.transform.SetAsFirstSibling(); 
                
                    UIMapLine lineUI = lineObj.GetComponent<UIMapLine>();
                    lineUI.DrawLine(uiNode.nodeData.position, nextData.position, mapDirection);
                }
            }
            UIMapNode uiMapNode = allUINodes.Find(n => n.nodeData.type == NodeType.Start);
            if (uiMapNode != null)
            {
                currentNode = uiMapNode;
                uiMapNode.SetState(NodeState.Current);
                UpdateSelectableNodes(uiMapNode.nodeData);
            }
        }

        private void HandleNodeClicked(UIMapNode clickedNode)
        {
            // 1. UI 상태 변경 (길 닫기, 현재 위치 갱신 등)
            currentNode.SetState(NodeState.Visited);

            foreach (var nextData in currentNode.nodeData.nextNodes)
            {
                UIMapNode sibling = allUINodes.Find(n => n.nodeData == nextData);
                if (sibling != null && sibling != clickedNode)
                {
                    sibling.SetState(NodeState.Unreachable);
                }
            }

            currentNode = clickedNode;
            currentNode.SetState(NodeState.Current);
            UpdateSelectableNodes(currentNode.nodeData);

            // 2. [추가됨] 해당 노드 타입에 맞는 이벤트 SO 실행
            NodeType clickedType = clickedNode.nodeData.type;
            if (eventDictionary.TryGetValue(clickedType, out NodeEventSO triggeredEvent))
            {
                // 실제 기능 실행 (씬 전환, UI 팝업 등)
                triggeredEvent.ExecuteEvent(clickedNode.nodeData);
            }
            else
            {
                Debug.LogWarning($"[{clickedType}] 타입에 연결된 이벤트 SO가 인스펙터에 없습니다!");
            }
        }

        private void UpdateSelectableNodes(Node nodeData)
        {
            foreach (var nextData in nodeData.nextNodes)
            {
                UIMapNode nextUI = allUINodes.Find(n => n.nodeData == nextData);
                if (nextUI != null) nextUI.SetState(NodeState.Selectable);
            }
        }
    }
}