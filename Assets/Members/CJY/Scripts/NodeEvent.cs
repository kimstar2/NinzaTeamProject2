using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Members.KJY._01.Scripts.Service;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Members.CJY.Scripts
{
    public class NodeEvent : MonoBehaviour
    {
        [Serializable]
        private struct NodeScene
        {
            [field:SerializeField] public NodeType Type {get; private set;}
            [field:SerializeField , Scene] public int Scene {get; private set;}
            [field:SerializeField] public bool ImmediatelyChangeScene {get; private set;}
            
            public UnityEvent onNodeSelected;
            
            public void ChangeScene()
            {
                if (ImmediatelyChangeScene)
                    SceneManager.LoadScene(Scene);
            }
        }
        
        [SerializeField] private List<NodeScene> nodeScenes;
        [SerializeField] private BattleDataStorage battleDataStorage;
        private NodeMaker _nodeMaker;
        public event Action<NodeConnect> OnNodeSelected;

        private void Awake() => _nodeMaker = GetComponent<NodeMaker>();

        public void SelectNode(NodeConnect node)
        {
            if (node == null || _nodeMaker == null || !_nodeMaker.CanEnter(node)) return;
            if (node.IsBattle && (node.view is not BattleNodeBT battleNode || battleDataStorage == null ||
                !battleDataStorage.TrySetBattle(battleNode.CurrentBattleData))) return;
            OnNodeSelected?.Invoke(node);
            NodeSelected(node.info.type);
        }

        private void NodeSelected(NodeType type)
        {
            if (nodeScenes == null) return;
            foreach (var nodeScene in nodeScenes)
            {
                if (nodeScene.Type != type) continue;
                nodeScene.onNodeSelected?.Invoke();
                nodeScene.ChangeScene();
                break;
            }
        }
    }
}