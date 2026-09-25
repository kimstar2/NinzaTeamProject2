using System;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.GameSystem;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Members.KJY._01.Scripts.Service
{
    public class BattleDataStorage : MonoBehaviour , IBattleDataStorage
    {
        [SerializeField] private BattleDataSO battleData;
        [Scene]
        [SerializeField] private int battleScene;
        
        public BattleDataStorage Instance => this;

        private void Awake()
        {
            ServiceLocator.Register<IBattleDataStorage>(this);
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
    }
}