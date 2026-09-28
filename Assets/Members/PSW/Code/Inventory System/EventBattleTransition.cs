using System;
using System.Reflection;
using DevLib.ServiceLocator;
using Members.CJY.Scripts;
using Members.KJY._01.Scripts;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Service;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Members.PSW.Code.InventorySystem
{
    // Captures the event's map origin and prepares a normal encounter. Kept on the
    // persistent PSW reward prefab so no map/event scene files need to be changed.
    public sealed class EventBattleTransition : MonoBehaviour
    {
        [SerializeField] private string eventScenePath;
        [SerializeField] private string battleScenePath;
        private static readonly FieldInfo ReturnSceneField = typeof(CompleteUI)
            .GetField("returnScene", BindingFlags.Instance | BindingFlags.NonPublic);

        private NodeMaker _map;
        private NodeEvent _nodeEvent;
        private BattleDataStorage _storage;
        private StageDataSO _stage;
        private int _column, _lastColumn, _seed;
        private bool _hasContext;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            DetachMap();
            _hasContext = false;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.path != eventScenePath) _hasContext = false;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var map in root.GetComponentsInChildren<NodeMaker>())
            {
                var nodeEvent = map.GetComponent<NodeEvent>();
                if (nodeEvent == null) continue;
                DetachMap();
                _map = map;
                _nodeEvent = nodeEvent;
                // NodeMaker subscribed in Awake and saves the selected node first.
                _nodeEvent.OnNodeSelected += HandleNodeSelected;
                return;
            }
        }

        private void HandleNodeSelected(NodeConnect node)
        {
            _hasContext = false;
            if (node?.info == null || node.info.type != NodeType.Event || _map == null ||
                !ServiceLocator.TryGet<IBattleDataStorage>(out var service) || service.Instance == null)
                return;
            var storage = service.Instance;
            if (storage.CurrentStageData == null || !storage.CurrentStageData.IsValid) return;
            _storage = storage;
            _stage = storage.CurrentStageData;
            _column = node.column;
            _lastColumn = _map.LastColumn;
            _seed = Random.Range(1, int.MaxValue);
            // Capture while the MAP is active, never from inside EventScene.
            // A defeat returns to the consumed event node instead of replaying its rewards.
            storage.SetReturnPoint(_map.SaveKey);
            _hasContext = true;
        }

        public bool TryPrepare(Scene eventScene, out string failure)
        {
            failure = "전투를 준비하지 못했습니다. 맵으로 돌아갑니다.";
            if (!_hasContext || eventScene.path != eventScenePath || _storage == null || _stage == null ||
                !ServiceLocator.TryGet<IBattleDataStorage>(out var service) || service.Instance != _storage ||
                _storage.CurrentStageData != _stage)
            {
                failure = "이벤트의 출발 지점이 없습니다. 맵에서 이벤트에 진입해 주세요.";
                return false;
            }
            if (!Array.Exists(_storage.GetRunTimePlayerData(), player => player != null && !player.IsDead))
            {
                failure = "전투 가능한 아군이 없어 전투를 시작할 수 없습니다.";
                return false;
            }
            if (ReturnSceneField == null || string.IsNullOrEmpty(battleScenePath) ||
                !Application.CanStreamedLevelBeLoaded(battleScenePath))
                return false;

            CompleteUI completion = null;
            foreach (var root in eventScene.GetRootGameObjects())
            foreach (var candidate in root.GetComponentsInChildren<CompleteUI>(true))
            {
                if (completion != null) return false;
                completion = candidate;
            }
            if (completion == null) return false;

            var battle = _stage.CreateBattle(_column, _lastColumn, _seed, EnemyRank.Normal);
            if (battle == null || !battle.IsValid || !_storage.TrySetBattle(battle)) return false;

            // Reuse CompleteUI's existing timed result and fade. Its destination is
            // instance state, so non-combat choices still return to the map normally.
            ReturnSceneField.SetValue(completion, battleScenePath);
            _hasContext = false;
            failure = null;
            return true;
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            if (_map == null || _map.gameObject.scene == scene) DetachMap();
        }

        private void DetachMap()
        {
            if (_nodeEvent != null) _nodeEvent.OnNodeSelected -= HandleNodeSelected;
            _nodeEvent = null;
            _map = null;
        }
    }
}
