using System;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.GameSystem;
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
        
        public BattleDataStorage Instance => this;

        private void Awake()
        {
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
            ServiceLocator.UnRegister<IBattleDataStorage>();
        }

        [ContextMenu("d")]
        public void Tp()
        {
            SceneManager.LoadScene(battleScene);
        }

        public BattleDataSO GetBattleData()
        {
            return battleData;
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