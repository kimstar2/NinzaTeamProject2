using System.Collections.Generic;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.GameSystem
{
    [CreateAssetMenu(fileName = "Battle Data", menuName = "KJY/Battle Data", order = 0)]
    public class BattleDataSO : ScriptableObject
    {
        public List<EnemyDataSO> enemyDataList;
        public float maxRiskPenalty, riskLevelIncrease, riskLevelIncreaseMulti;
        [field: SerializeField, Min(0)] public int GoldReward { get; private set; } = 15;
        public string StageName { get; private set; } = "전투";
        public EnemyRank EncounterRank { get; private set; }
        public string EncounterLabel => EncounterRank switch
        {
            EnemyRank.Elite => "정예 전투",
            EnemyRank.Boss => "보스 전투",
            _ => "전투"
        };

        public void SetEncounter(string stageName, EnemyRank rank, int goldReward, List<EnemyDataSO> enemies)
        {
            StageName = stageName;
            EncounterRank = rank;
            GoldReward = Mathf.Max(0, goldReward);
            enemyDataList = enemies;
        }
    }
}
