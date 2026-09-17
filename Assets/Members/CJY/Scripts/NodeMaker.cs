using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        [SerializeField] private int columnCount; // start, boss 노드를 제외한 열 개수
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject nodeGroupPrefab;

        [Header("UI")] 
        [SerializeField] private RectTransform nodeParent;
        
        
        private NodeEvent nodeEvent;

        private void Awake()
        {
            nodeEvent = GetComponent<NodeEvent>();
        }

        private int[] NodeCount(int colCnt)
        {
            int[] cnt = new int[colCnt];
            for (int i = 0; i < colCnt; i++)
            {
                cnt[i] = Random.Range(2, 4); 
            }

            return cnt;
        }

        public void MakeNode()
        {
            int[] nodeCount = NodeCount(columnCount);
            int total =  nodeCount.Sum();
            
            
        }
        
        
    }
}