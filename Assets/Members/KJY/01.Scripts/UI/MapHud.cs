using DevLib.ServiceLocator;
using Members.CJY.Scripts;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.KJY._01.Scripts.Service;
using TMPro;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    [DefaultExecutionOrder(100)]
    public partial class MapHud : MonoBehaviour
    {
        [SerializeField] private NodeMaker nodeMaker;
        [SerializeField] private NodeEvent nodeEvent;
        [SerializeField] private TMP_Text stageTitle, progress, gold;
        [SerializeField] private GameObject completeNotice;
        private BattleDataStorage _storage;
        private BattleInventory _inventory;

        private void Start()
        {
            _storage = ServiceLocator.Get<IBattleDataStorage>().Instance;
            ServiceLocator.TryGet<Inventory>(out var inventory);
            _inventory = inventory as BattleInventory;
            nodeEvent.OnNodeSelected += Refresh;
            Refresh(null);
        }

        private void Refresh(NodeConnect node)
        {
            stageTitle.text = $"{_storage.CurrentStage + 1:00}  /  {_storage.CurrentStageData.stageName}";
            progress.text = $"진행 {nodeMaker.CurrentColumn} / {nodeMaker.LastColumn}   ·   다음 경로를 선택하세요";
            gold.text = $"{(_inventory != null ? _inventory.Gold : 0)} G";
            completeNotice.SetActive(_storage.IsRunComplete);
        }

        public void ReturnToTitle() => SceneTransition.Load("Assets/Members/KJY/02.RealScene/UIScene.unity");
        private void OnDestroy() { if (nodeEvent != null) nodeEvent.OnNodeSelected -= Refresh; }
    }
}
