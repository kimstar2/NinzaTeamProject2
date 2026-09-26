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
            SceneManager.LoadScene(battleScene);
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
            _runtimeBattle.SetBattle(enemies, data.gold, $"스테이지 {CurrentStage + 1}", data.rank);
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
