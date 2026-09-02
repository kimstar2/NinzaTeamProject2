using UnityEngine;
using UnityEngine.SceneManagement;

namespace Members.PDY.Scripts.Node
{
    using System.Collections.Generic;
    using UnityEngine;

    public class MapManager : MonoBehaviour
    {
        [Header("Map Settings")]
        public MapDirection mapDirection = MapDirection.Horizontal; 
        public int columns = 15; 
        public int rows = 7;     
        public int startingPaths = 4;
        public float spacingX = 150f; 
        public float spacingY = 100f;

        // [추가됨] 노드의 위치를 불규칙하게 흔들어줄 범위 (픽셀 단위)
        [Header("Jitter Settings")]
        public float jitterX = 20f; 
        public float jitterY = 25f;

        [Header("UI References")]
        public RectTransform mapContainer; 
        public GameObject nodePrefab;      
        public GameObject linePrefab;      

        [Header("Scroll View Settings")]
        public float padding = 200f; 

        private List<UIMapNode> allUINodes = new List<UIMapNode>();
        private UIMapNode currentNode;

        void Start()
        {
            // [수정됨] jitterX와 jitterY를 인자로 추가 전달
            List<List<Node>> mapData = MapAlgorithm.GenerateMapData(columns, rows, startingPaths, spacingX, spacingY, mapDirection, jitterX, jitterY);
            
            AdjustContentSize();
            RenderMapUI(mapData);
        }

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
                    lineUI.DrawLine(uiNode.nodeData.position, nextData.position);
                }
            }
        }

        private void HandleNodeClicked(UIMapNode clickedNode)
        {
            if (currentNode.nodeData.type == NodeType.Event)
            {
                SceneManager.LoadScene("EventScene");
            }
            else if (currentNode.nodeData.type == NodeType.Rest)
            {
                SceneManager.LoadScene("RestScene");
            }
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