using System.Collections.Generic;
using Members.KJY._01.Scripts.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts.GameSystem
{
    [CreateAssetMenu(fileName = "Battle Data", menuName = "KJY/Battle Data", order = 0)]
    public class BattleDataSO : ScriptableObject
    {
        public List<EnemyDataSO> enemyDataList;
        public float maxRiskPenalty, riskLevelIncrease, riskLevelIncreaseMulti;
        [field: SerializeField, Min(0)] public int GoldReward { get; private set; } = 15;
        [field: SerializeField] public string StageName { get; private set; } = "전투";
        [field: SerializeField] public EnemyRank EncounterRank { get; private set; }
        public string EncounterLabel => EncounterRank switch
        {
            EnemyRank.Elite => "정예 전투",
            EnemyRank.Boss => "보스 전투",
            _ => "전투"
        };
        public void SetBattle(EnemyDataSO[] enemies, int gold, string stageName, EnemyRank rank)
        {
            enemyDataList = new List<EnemyDataSO>(enemies);
            GoldReward = Mathf.Max(0, gold);
            StageName = stageName;
            EncounterRank = rank;
        }
    }
}
