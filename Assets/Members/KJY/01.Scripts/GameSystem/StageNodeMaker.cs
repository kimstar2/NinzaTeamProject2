using System.Collections.Generic;
using System.Linq;
using _LumenLib.PoolingSystem.Runtime;
using Members.CJY.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Members.KJY._01.Scripts.GameSystem
{
    // CJY의 노드 데이터와 풀을 재사용하며, 스테이지별 지도 생성·저장만 KJY에서 관리한다.
    public class StageNodeMaker : MonoBehaviour
    {
        [Header("Node")]
        [SerializeField] private NodeInfoSO startNode;
        [SerializeField] private NodeInfoSO bossNode;
        [SerializeField] private List<NodeInfoSO> nodeInfos;

        [Header("Settings")]
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private int columnCount; // start, boss 노드를 제외한 열 개수
        [SerializeField] private int minNodeCount, maxNodeCount;

        [Header("UI")]
        [SerializeField] private RectTransform nodeParent;
        [SerializeField] private Transform lineParent;

        [Header("NodeInfo")]
        [SerializeField] private GameObject infoPrefab;
        [SerializeField] private Transform infoParent;

        private NodeEvent nodeEvent;
        private NodeConnect currentNode;
        private List<List<NodeConnect>> nodeConnects = new List<List<NodeConnect>>();
        private ScrollRect _scrollRect;
        private int _stageIndex;
        private string saveKey => GetSaveKey(_stageIndex);
        public static string GetSaveKey(int stage) => $"MapSaveData.Stage{stage}";
        public int LastColumn => columnCount + 1;
        public event System.Action<NodeConnect> OnNodeEntered;

        public void ConfigureStage(int stage) => _stageIndex = Mathf.Max(0, stage);

        private void Awake()
        {
            nodeEvent = GetComponent<NodeEvent>();
            _scrollRect = nodeParent.GetComponentInParent<ScrollRect>();
            nodeEvent.OnNodeSelected += HandleNodeSelected;

            MakeInfo();
        }

        private void MakeInfo()
        {
            List<NodeInfoSO> nodeInfo = new List<NodeInfoSO>();
            nodeInfo.Add(startNode);
            nodeInfo.Add(bossNode);
            nodeInfo.AddRange(nodeInfos);

            foreach (NodeInfoSO info in nodeInfo)
            {
                GameObject obj = Instantiate(infoPrefab, infoParent);
                obj.GetComponent<NodeInfoUI>().Init(info);
            }
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
            OnNodeEntered?.Invoke(nextNode);
        }

        private int[] NodeCount(int colCnt)
        {
            int[] cnt = new int[colCnt];
            for (int i = 0; i < colCnt; i++)
            {
                cnt[i] = Random.Range(minNodeCount, maxNodeCount+1);
                if (i > 1)
                {
                    if (cnt[i] == cnt[i - 1] && cnt[i] == cnt[i - 2])
                        cnt[i] = cnt[i] == 2 ? 3 : 2;
                }
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

                    NodeInfoSO info = nodeTypes[typeCount];
                    NodeInfoSO elite = nodeInfos.FirstOrDefault(x => x.type == NodeType.Elite);
                    NodeInfoSO battle = nodeInfos.FirstOrDefault(x => x.type == NodeType.Battle);
                    int column = nodeConnects.Count;
                    bool eliteColumn = column == Mathf.RoundToInt(columnCount * 0.4f) ||
                                       column == Mathf.RoundToInt(columnCount * 0.75f);
                    if (eliteColumn && i == count / 2 && elite != null) info = elite;
                    else if (info.type == NodeType.Elite && battle != null) info = battle;
                    if (column <= 2 && battle != null) info = battle;
                    NodeConnect connect = new NodeConnect(column, i, info);
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
                if (groupTrm == lineParent || !groupTrm.TryGetComponent<IPoolable>(out var group)) continue;

                for (int j = groupTrm.childCount - 1; j >= 0; j--)
                {
                    Transform nodeTrm = groupTrm.GetChild(j);
                    IPoolable node = groupTrm.GetChild(j).GetComponent<IPoolable>();

                    nodeTrm.SetParent(objectPool.transform, false);
                    objectPool.Push(node);
                }

                groupTrm.SetParent(objectPool.transform, false);
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
            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.horizontalNormalizedPosition = currentNode.column / (float)Mathf.Max(1, LastColumn);
            }
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
                        node.view.SetVisual(NodeState.Movable);
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
