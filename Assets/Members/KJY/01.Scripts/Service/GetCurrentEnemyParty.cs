using System;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.GameSystem.EnemyParty;
using UnityEngine;
using ZLinq;
using Random = UnityEngine.Random;

namespace Members.KJY._01.Scripts.Service
{
    public class GetCurrentEnemyParty : MonoBehaviour , IGetCurrentEnemyParty
    {
        [field:SerializeField] public EnemyPartyListSO EnemyPartyList { get; private set; }
        private void Awake()
        {
            ServiceLocator.Register<IGetCurrentEnemyParty>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IGetCurrentEnemyParty>();
        }

        public EnemyDataSO GetData(EnemyPartyType crtType)
        {
            int l = EnemyPartyList.EnemyPartyList.
                AsValueEnumerable().
                Where(x => x.EnemyPartyType == crtType).
                Select(x=>x.EnemyDataList.Length).FirstOrDefault();
            
            int r = Random.Range(0, l);

            EnemyDataSO enemyData =EnemyPartyList.EnemyPartyList.
                AsValueEnumerable().
                Where(x => x.EnemyPartyType == crtType).
                Select(x=>x.EnemyDataList[r]).FirstOrDefault();
            
            return enemyData;
        }
    }
}