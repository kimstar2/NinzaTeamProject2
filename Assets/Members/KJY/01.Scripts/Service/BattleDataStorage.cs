using Members.KJY._01.Scripts.UI;
using System.Collections.Generic;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.GameSystem;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Members.KJY._01.Scripts.Service
{
    [DefaultExecutionOrder(-500)]
    public class BattleDataStorage : MonoBehaviour , IBattleDataStorage
    {
        [field:SerializeField] public List<StageDataSO> StageDataList { get; private set; }
        [SerializeField] private PlayerDataSO runTimeTanker,runTimeDealer,runTimeHealer, runTimeMage ;
        [SerializeField] private BattleDataSO battleData;
        [Scene]
        [SerializeField] private int battleScene;
        [field: SerializeField, Min(0)] public int CurrentStage { get; set; }
        private BattleDataSO _runtimeBattle;
        private bool _isOwner;
        private string _returnScene, _mapKey, _mapCheckpoint;
        public bool IsRunComplete { get; private set; }

        public BattleDataStorage Instance => this;
        public StageDataSO CurrentStageData => StageDataList != null && CurrentStage >= 0 &&
            CurrentStage < StageDataList.Count ? StageDataList[CurrentStage] : null;

        private void Awake()
        {
            if (ServiceLocator.TryGet<IBattleDataStorage>(out var existing) && existing.Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _isOwner = true;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<IBattleDataStorage>(this);
            runTimeTanker = Instantiate(runTimeTanker);
            runTimeDealer = Instantiate(runTimeDealer);
            runTimeHealer = Instantiate(runTimeHealer);
            runTimeMage = Instantiate(runTimeMage);
            
            runTimeTanker.Init();
            runTimeDealer.Init();
            runTimeHealer.Init();
            runTimeMage.Init();
        }

        private void OnDestroy()
        {
            if (!_isOwner) return;
            ServiceLocator.UnRegister<IBattleDataStorage>();
            ReleaseBattle();
            foreach (PlayerDataSO player in GetRunTimePlayerData())
                if (player != null) Destroy(player);
        }

        [ContextMenu("d")]
        public void Tp()
        {
            SceneTransition.Load(battleScene);
        }

        public void SetReturnPoint(string mapKey)
        {
            _returnScene = SceneManager.GetActiveScene().path;
            _mapKey = mapKey;
            _mapCheckpoint = PlayerPrefs.GetString(mapKey, string.Empty);
        }

        public void FinishBattle(bool victory)
        {
            if (!victory)
            {
                if (!string.IsNullOrEmpty(_mapKey)) PlayerPrefs.SetString(_mapKey, _mapCheckpoint);
                foreach (var player in GetRunTimePlayerData()) player.RecoverAfterDefeat();
            }
            else if (GetBattleData().EncounterRank == EnemyRank.Boss)
            {
                if (CurrentStage + 1 < StageDataList.Count) CurrentStage++;
                else IsRunComplete = true;
            }
            PlayerPrefs.Save();
        }

        public void ReturnToMap()
        {
            if (!string.IsNullOrEmpty(_returnScene)) SceneTransition.Load(_returnScene);
        }

        public void ResetRun()
        {
            ReleaseBattle();
            CurrentStage = 0;
            IsRunComplete = false;
            foreach (var player in GetRunTimePlayerData()) player.Init();
            for (int i = 0; i < StageDataList.Count; i++) PlayerPrefs.DeleteKey($"MapSaveData.Stage{i}");
        }

        public bool TrySetBattle(BattleData data)
        {
            if (battleData == null || data == null || !data.IsValid) return false;
            ReleaseBattle();
            var enemies = new EnemyDataSO[data.enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyRank rank = i == 0 ? data.rank : EnemyRank.Normal;
                enemies[i] = data.enemies[i].CreateBattleCopy(data.healthMultiplier, data.dicePower, rank);
            }
            _runtimeBattle = Instantiate(battleData);
            _runtimeBattle.hideFlags = HideFlags.DontSave;
            _runtimeBattle.SetBattle(enemies, data.gold, CurrentStageData.stageName, data.rank);
            return true;
        }

        private void ReleaseBattle()
        {
            if (_runtimeBattle == null) return;
            foreach (var enemy in _runtimeBattle.enemyDataList) if (enemy != null) Destroy(enemy);
            Destroy(_runtimeBattle);
            _runtimeBattle = null;
        }

        public BattleDataSO GetBattleData()
        {
            return _runtimeBattle != null ? _runtimeBattle : battleData;
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
