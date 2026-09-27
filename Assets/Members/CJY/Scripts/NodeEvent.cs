using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Members.KJY._01.Scripts.Service;
using Members.KJY._01.Scripts.UI;
using DevLib.ServiceLocator;
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
                    SceneTransition.Load(Scene);
            }
        }
        
        [SerializeField] private List<NodeScene> nodeScenes;
        [SerializeField] private BattleDataStorage battleDataStorage;
        [SerializeField] private BattleNodePanel battlePanel;
        private NodeMaker _nodeMaker;
        private NodeConnect _pendingNode;
        public event Action<NodeConnect> OnNodeSelected;

        private void Awake() => _nodeMaker = GetComponent<NodeMaker>();

        public void SelectNode(NodeConnect node)
        {
            if (_pendingNode != null || node == null || _nodeMaker == null || !_nodeMaker.CanEnter(node)) return;
            if (ServiceLocator.TryGet<IBattleDataStorage>(out var storage)) battleDataStorage = storage.Instance;
            if (node.IsBattle)
            {
                if (battlePanel == null || node.battleData == null || !node.battleData.IsValid) return;
                _pendingNode = node;
                battlePanel.Show(node.battleData, battleDataStorage.CurrentStageData.stageName);
                return;
            }
            OnNodeSelected?.Invoke(node);
            NodeSelected(node.info.type);
        }

        public void ConfirmBattle()
        {
            var node = _pendingNode;
            if (node == null || !_nodeMaker.CanEnter(node) || !battleDataStorage.TrySetBattle(node.battleData)) return;
            battleDataStorage.SetReturnPoint(_nodeMaker.SaveKey);
            _pendingNode = null;
            OnNodeSelected?.Invoke(node);
            battleDataStorage.Tp();
        }

        public void CancelBattle() => _pendingNode = null;

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
