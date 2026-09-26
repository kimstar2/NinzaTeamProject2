using System;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.GameSystem;
using Members.KJY._01.Scripts.GameSystem.EnemyParty;
using Members.KJY._01.Scripts.Agent.Enemy;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Members.KJY._01.Scripts.Service
{
    public class BattleDataStorage : MonoBehaviour , IBattleDataStorage
    {
        [SerializeField] private PlayerDataSO runTimeTanker,runTimeDealer,runTimeHealer, runTimeMage ;
        [SerializeField] private BattleDataSO battleData;
        [Scene]
        [SerializeField] private int battleScene;
        [SerializeField] private EnemyPartyListSO stages;
        [Scene, SerializeField] private int mapScene = 1;
        private BattleDataSO _encounter;
        private bool _isOwner, _returning, _isNodeEncounter;
        private int _runSeed;
        public int StageIndex { get; private set; }
        public bool RunCompleted { get; private set; }
        public bool IsFinalEncounter => _isNodeEncounter && _encounter != null &&
            _encounter.EncounterRank == EnemyRank.Boss && StageIndex == stages.StageCount - 1;
        public string CurrentStageName => stages != null && StageIndex < stages.StageCount
            ? stages.EnemyPartyList[StageIndex].StageName : "전투";
        
        public BattleDataStorage Instance => this;

        private void Awake()
        {
            if (ServiceLocator.TryGet<IBattleDataStorage>(out var existing) && existing.Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _isOwner = true;
            ServiceLocator.Register<IBattleDataStorage>(this);
            runTimeTanker = Instantiate(runTimeTanker);
            runTimeDealer = Instantiate(runTimeDealer);
            runTimeHealer = Instantiate(runTimeHealer);
            runTimeMage = Instantiate(runTimeMage);
            
            runTimeTanker.Init();
            runTimeDealer.Init();
            runTimeHealer.Init();
            runTimeMage.Init();
            _runSeed = UnityEngine.Random.Range(1, int.MaxValue);
            ClearSavedMaps();
        }

        private void OnDestroy()
        {
            if (!_isOwner) return;
            ServiceLocator.UnRegister<IBattleDataStorage>();
            ReleaseEncounter();
            foreach (PlayerDataSO player in GetRunTimePlayerData())
                if (player != null) Destroy(player);
        }

        [ContextMenu("d")]
        public void Tp()
        {
            _returning = false;
            SceneManager.LoadScene(battleScene);
        }

        public void EnterEncounter(int column, int lastColumn, EnemyRank rank)
        {
            if (RunCompleted || stages == null || _isNodeEncounter) return;
            ReleaseEncounter();
            float progress = Mathf.InverseLerp(1, Mathf.Max(2, lastColumn), column);
            _encounter = stages.CreateEncounter(battleData, StageIndex, progress, rank,
                _runSeed ^ (StageIndex * 7919) ^ (column * 397));
            if (_encounter == null)
            {
                Debug.LogError("스테이지의 적 목록과 보스 연결을 확인하세요.", this);
                return;
            }
            _isNodeEncounter = true;
            Tp();
        }

        public void CompleteEncounter(bool victory)
        {
            if (_returning) return;
            _returning = true;
            if (victory && _isNodeEncounter)
            {
                bool bossClear = _encounter.EncounterRank == EnemyRank.Boss;
                foreach (PlayerDataSO player in GetRunTimePlayerData())
                {
                    if (bossClear) player.Init();
                    else player.Heal(player.MaxHealth * 0.15f);
                }
                if (bossClear)
                {
                    if (StageIndex + 1 < stages.StageCount) StageIndex++;
                    else RunCompleted = true;
                }
            }
            else if (!victory) RestartRun();
            _isNodeEncounter = false;
            SceneManager.LoadScene(mapScene);
        }

        public void RestartRun()
        {
            StageIndex = 0;
            RunCompleted = false;
            _isNodeEncounter = false;
            _runSeed = UnityEngine.Random.Range(1, int.MaxValue);
            foreach (PlayerDataSO player in GetRunTimePlayerData()) player.Init();
            ClearSavedMaps();
        }

        public void Rest()
        {
            foreach (PlayerDataSO player in GetRunTimePlayerData()) player.Heal(player.MaxHealth * 0.3f);
        }

        private void ClearSavedMaps()
        {
            if (stages == null) return;
            for (int i = 0; i < stages.StageCount; i++) PlayerPrefs.DeleteKey(StageNodeMaker.GetSaveKey(i));
        }

        private void ReleaseEncounter()
        {
            if (_encounter == null) return;
            foreach (EnemyDataSO enemy in _encounter.enemyDataList)
                if (enemy != null) Destroy(enemy);
            Destroy(_encounter);
            _encounter = null;
        }

        public BattleDataSO GetBattleData()
        {
            return _encounter != null ? _encounter : battleData;
        }
        
        public PlayerDataSO GetRunTimePlayerData(PlayerType pT)
        {
            return pT switch
            {
                PlayerType.Tanker => runTimeTanker,
                PlayerType.Dealer => runTimeDealer,
                PlayerType.Healer => runTimeHealer,
                PlayerType.Mage => runTimeMage,
                _ => null
            };
        }

        public PlayerDataSO[] GetRunTimePlayerData()
        {
            return new[] { runTimeTanker, runTimeDealer, runTimeHealer, runTimeMage };
        }
    }
}
