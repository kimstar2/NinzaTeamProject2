using System;
using System.Collections.Generic;
using System.Linq;
using _LumenLib.PoolingSystem.Runtime;
using Members.PDY.Scripts.Node;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Members.CJY.Scripts
{
    public class NodeMaker : MonoBehaviour
    {
        [Header("NodeInfo")] 
        [SerializeField] private NodeInfoSO startNode;
        [SerializeField] private NodeInfoSO bossNode;
        [SerializeField] private List<NodeInfoSO> nodeInfos;

        [Header("Settings")] 
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private int columnCount; // start, boss 노드를 제외한 열 개수
        [SerializeField] private int minNodeCount, maxNodeCount;

        [Header("UI")] 
        [SerializeField] private RectTransform nodeParent;
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject nodeGroupPrefab;
        [SerializeField] private Transform lineParent;
        [SerializeField] private GameObject linePrefab;

        private NodeEvent nodeEvent;
        private NodeConnect currentNode;
        private List<List<NodeConnect>> nodeConnects = new List<List<NodeConnect>>();
        private const string saveKey = "MapSaveData";

        private void Awake()
        {
            nodeEvent = GetComponent<NodeEvent>();
            nodeEvent.OnNodeSelected += HandleNodeSelected;
        }

        private void OnDestroy()
        {
            nodeEvent.OnNodeSelected -= HandleNodeSelected;
        }

        private void HandleNodeSelected(NodeConnect nextNode)
        {
            if (currentNode == null) return;

            if (!currentNode.nextNodes.Contains(nextNode))
                return;

            currentNode = nextNode;
            NodeVisualSetting();
            SaveMap();
        }

        private int[] NodeCount(int colCnt)
        {
            int[] cnt = new int[colCnt];
            for (int i = 0; i < colCnt; i++)
            {
                cnt[i] = Random.Range(minNodeCount, maxNodeCount+1); 
            }

            return cnt;
        }

        public void MakeNode()
        {
            DeleteNode();
            
            int[] nodeCount = NodeCount(columnCount);
            int total =  nodeCount.Sum();
            List<NodeInfoSO> info =  NodeSetting(total);
            
            MakeSingleGroup(startNode);
            MakeGroup(nodeCount, info);
            MakeSingleGroup(bossNode);
            
            foreach (Transform groupTrm in nodeParent)
                LayoutRebuilder.ForceRebuildLayoutImmediate(groupTrm.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(nodeParent);
            
            ConnectNode();
            RenderLine();

            currentNode = nodeConnects[0][0];
            NodeVisualSetting();
            
            SaveMap();
        }

        public void OpenNode()
        {
            if (!LoadMap())
            {
                MakeNode();
            }
            else Debug.Log("노드를 정상적으로 불러왔습니다!!!!!!");
        }

        public void DeleteNode()
        {
            ResetNode();
            nodeConnects.Clear();
        }

        private void MakeGroup(int[] nodeCount, List<NodeInfoSO> nodeTypes)
        {
            int typeCount = 0;
            foreach (int count in nodeCount)
            {
                IPoolable groupItem = objectPool.Pop("NodeGroup");
                Transform groupTrm = groupItem.GameObject.transform;

                groupTrm.SetParent(nodeParent, false);
                
                List<NodeConnect> nodeGroups = new List<NodeConnect>();

                for (int i = 0; i < count; i++)
                {
                    IPoolable nodeItem = objectPool.Pop("Node");
                    Transform nodeTrm = nodeItem.GameObject.transform;
                    nodeTrm.SetParent(groupTrm, false);

                    NodeBT bt = nodeItem.GameObject.GetComponent<NodeBT>();
                    
                    NodeConnect connect = new NodeConnect(nodeConnects.Count, i, nodeTypes[typeCount]);
                    connect.view = bt;
                    nodeGroups.Add(connect);
                    bt.Init(connect, nodeEvent);
                    
                    typeCount++;
                }
                nodeConnects.Add(nodeGroups);
            }
        }

        private void MakeSingleGroup(NodeInfoSO info)
        {
            IPoolable groupItem = objectPool.Pop("NodeGroup");
            Transform groupTrm = groupItem.GameObject.transform;
            
            groupTrm.SetParent(nodeParent, false);
            
            IPoolable nodeItem = objectPool.Pop("Node");
            Transform nodeTrm = nodeItem.GameObject.transform;
            nodeTrm.SetParent(groupTrm, false);

            NodeBT bt = nodeItem.GameObject.GetComponent<NodeBT>();

            NodeConnect connect = new NodeConnect(nodeConnects.Count, 0, info);
            connect.view = bt;
            nodeConnects.Add(new List<NodeConnect>() {connect});
            bt.Init(connect, nodeEvent);
        }

        // 좀 비효율적이긴 함 (나중에 다시 보기)
        private void ResetNode()
        {
            for (int i = nodeParent.childCount - 1; i >= 0; i--)
            {
                Transform groupTrm = nodeParent.GetChild(i);
                
                for (int j = groupTrm.childCount - 1; j >= 0; j--)
                {
                    Transform nodeTrm = groupTrm.GetChild(j);
                    IPoolable node = groupTrm.GetChild(j).GetComponent<IPoolable>();
                    
                    nodeTrm.SetParent(objectPool.transform, false);
                    objectPool.Push(node);
                }
                
                IPoolable group = groupTrm.GetComponent<IPoolable>();
                groupTrm.SetParent(nodeParent, false);
                objectPool.Push(group);
            }

            for (int i = lineParent.childCount - 1; i >= 0; i--)
            {
                Transform lineTrm = lineParent.GetChild(i);
                IPoolable line = lineParent.GetChild(i).GetComponent<IPoolable>();
                
                lineTrm.SetParent(objectPool.transform, false);
                objectPool.Push(line);
            }
        }

        private List<NodeInfoSO> NodeSetting(int total)
        {
            List<NodeInfoSO> nodes = new List<NodeInfoSO>();

            foreach (NodeInfoSO info in nodeInfos)
            {
                int count = Mathf.RoundToInt(total * (info.percent / 100f));
                for (int i = 0; i < count; i++)
                {
                    nodes.Add(info);
                }
            }

            // 임시 (total 개수보다 적으면 랜덤으로 노드 하나 넣어주기)
            while (total > nodes.Count)
            {
                nodes.Add(nodeInfos[Random.Range(0, nodeInfos.Count)]);
            }
            
            // 섞기
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                int j = Random.Range(0, i+1);
                    
                NodeInfoSO temp =  nodes[i];
                nodes[i] = nodes[j];
                nodes[j] = temp;
            }

            return nodes;
        }

        private void ConnectNode()
        {
            float maxYDiff = 100f;

            for (int i = 0; i < nodeConnects.Count - 1; i++)
            {
                List<NodeConnect> currentColumn = nodeConnects[i];
                List<NodeConnect> nextColumn = nodeConnects[i + 1];
                
                foreach (NodeConnect next in nextColumn)
                {
                    float nextY = next.view.transform.position.y;

                    List<NodeConnect> candidates = new List<NodeConnect>();
                    foreach (NodeConnect current in currentColumn)
                    {
                        float currentY = current.view.transform.position.y;
                        if (Mathf.Abs(currentY - nextY) <= maxYDiff)
                        {
                            candidates.Add(current);
                        }
                    }

                    if (candidates.Count == 0)
                    {
                        NodeConnect closest = currentColumn[0];
                        float minDiff = Mathf.Abs(closest.view.transform.position.y - nextY);
                        foreach (NodeConnect current in currentColumn)
                        {
                            float diff = Mathf.Abs(current.view.transform.position.y - nextY);
                            if (diff < minDiff)
                            {
                                closest = current;
                                minDiff = diff;
                            }
                        }
                        candidates.Add(closest);
                    }

                    NodeConnect parent = candidates[Random.Range(0, candidates.Count)];
                    parent.nextNodes.Add(next);
                }

                // current 중에 나가는 연결이 하나도 없는 애가 있으면, 제일 가까운 next에 강제 연결
                foreach (NodeConnect current in currentColumn)
                {
                    if (current.nextNodes.Count == 0)
                    {
                        float currentY = current.view.transform.position.y;

                        NodeConnect closest = nextColumn[0];
                        float minDiff = Mathf.Abs(closest.view.transform.position.y - currentY);
                        foreach (NodeConnect next in nextColumn)
                        {
                            float diff = Mathf.Abs(next.view.transform.position.y - currentY);
                            if (diff < minDiff)
                            {
                                closest = next;
                                minDiff = diff;
                            }
                        }

                        current.nextNodes.Add(closest);
                    }
                }

                /*foreach (NodeConnect current in currentColumn)
                {
                    int lineCount = 1;

                    for (int j = 0; j < lineCount; j++)
                    {
                        int nextNode = Random.Range(0, nextColumn.Count);
                        NodeConnect target = nextColumn[nextNode];

                        if (!current.nextNodes.Contains(target))
                        {
                            current.nextNodes.Add(target);
                        }
                    }
                }

                // 다음 컬럼의 노드중에 현재 컬럼과 하나도 연결되어있지 않은 노드는
                // 다시 랜덤돌려서 현재 컬럼의 노드중 하나를 연결시켜줌
                foreach (NodeConnect next in nextColumn)
                {
                    bool connected = false;

                    foreach (NodeConnect current in currentColumn)
                    {
                        if (current.nextNodes.Contains(next))
                        {
                            connected = true;
                            break;
                        }
                    }

                    if (!connected)
                    {
                        int currentNode = Random.Range(0, currentColumn.Count);
                        currentColumn[currentNode].nextNodes.Add(next);
                    }
                }*/
            }
        }
        

        private void RenderLine()
        {
            foreach (List<NodeConnect> column in nodeConnects)
            {
                foreach (NodeConnect node in column)
                {
                    foreach (NodeConnect next in node.nextNodes)
                    {
                        Vector3 fromPos = node.view.transform.position;
                        Vector3 toPos = next.view.transform.position;
                        
                        IPoolable lineItem = objectPool.Pop("Line");
                        Transform lineTrm = lineItem.GameObject.transform;
                        lineTrm.SetParent(lineParent, false);
                        lineItem.GameObject.SetActive(true);

                        RectTransform rt = lineTrm.GetComponent<RectTransform>();
                        Vector3 dir = toPos - fromPos;
                        float distance = dir.magnitude;
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                        rt.pivot = new Vector2(0f, 0.5f);
                        rt.position = fromPos;
                        rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y);
                        rt.rotation = Quaternion.Euler(0f, 0f, angle);
                    }
                }
            }
        }

        private void NodeVisualSetting()
        {
            foreach (List<NodeConnect> column in nodeConnects)
            {
                foreach (NodeConnect node in column)
                {
                    if (node == currentNode)
                    {
                        node.view.SetVisual(NodeState.Current);
                    }
                    else if (currentNode.nextNodes.Contains(node))
                    {
                        node.view.SetVisual(NodeState.Moveable);
                    }
                    else
                    {
                        node.view.SetVisual(NodeState.Locked);
                    }
                }
            }
        }

        private void SaveMap()
        {
            MapSaveData saveData = new MapSaveData
            {
                hasData = true
            };

            foreach (List<NodeConnect> column in nodeConnects)
            {
                foreach (NodeConnect node in column)
                {
                    NodeSaveData data = new NodeSaveData
                    {
                        column = node.column,
                        lane = node.lane,
                        type = node.info.type
                    };


                    foreach (NodeConnect next in node.nextNodes)
                    {
                        data.nextNode.Add(new NodeData
                        {
                            column = next.column,
                            lane = next.lane,
                        });
                    }
                    
                    saveData.nodes.Add(data);
                }
            }

            saveData.currentNode = new NodeData
            {
                column = currentNode.column,
                lane = currentNode.lane
            };
            
            string json = JsonUtility.ToJson(saveData, true);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }
        
        private NodeInfoSO GetInfoByType(NodeType type)
        {
            if (type == NodeType.Start) return startNode;
            if (type == NodeType.Boss) return bossNode;
            
            return nodeInfos.FirstOrDefault(x => x.type == type);
        }

        private bool LoadMap()
        {
            if (!PlayerPrefs.HasKey(saveKey)) return false;
            
            string json = PlayerPrefs.GetString(saveKey);
            MapSaveData saveData = JsonUtility.FromJson<MapSaveData>(json);
            if (!saveData.hasData) return false;
            
            ResetNode();
            nodeConnects.Clear();
            
            // =================================
            
            Dictionary<(int, int), NodeConnect> connects = new Dictionary<(int, int), NodeConnect>();
            Dictionary<int, List<NodeConnect>> columns = new Dictionary<int, List<NodeConnect>>();
            Dictionary<int, Transform> groupTransforms = new Dictionary<int, Transform>();
            
            foreach (NodeSaveData data in saveData.nodes)
            {
                if (!groupTransforms.TryGetValue(data.column, out Transform groupTrm))
                {
                    IPoolable groupItem = objectPool.Pop("NodeGroup");
                    groupTrm = groupItem.GameObject.transform;
                    groupTrm.SetParent(nodeParent, false);

                    groupTransforms.Add(data.column, groupTrm);
                    columns.Add(data.column, new List<NodeConnect>());
                }

                NodeInfoSO info = GetInfoByType(data.type);

                IPoolable nodeItem = objectPool.Pop("Node");
                Transform nodeTrm = nodeItem.GameObject.transform;
                nodeTrm.SetParent(groupTrm, false);

                NodeBT bt = nodeItem.GameObject.GetComponent<NodeBT>();
                NodeConnect connect = new NodeConnect(data.column, data.lane, info);
                connect.view = bt;
                bt.Init(connect, nodeEvent);

                columns[data.column].Add(connect);
                connects[(data.column, data.lane)] = connect; 
            }

            foreach (int col in columns.Keys.OrderBy(c => c))
                nodeConnects.Add(columns[col]);
            
            foreach (NodeSaveData data in saveData.nodes)
            {
                NodeConnect node = connects[(data.column, data.lane)];
                foreach (NodeData key in data.nextNode)
                {
                    node.nextNodes.Add(connects[(key.column, key.lane)]);
                }
            }

            currentNode = connects[(saveData.currentNode.column, saveData.currentNode.lane)];

            foreach (Transform groupTrm in nodeParent)
                LayoutRebuilder.ForceRebuildLayoutImmediate(groupTrm.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(nodeParent);

            RenderLine();
            NodeVisualSetting();

            return true;
        }
    }
}