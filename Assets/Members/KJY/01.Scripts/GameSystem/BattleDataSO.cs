using System.Collections.Generic;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.GameSystem
{
    [CreateAssetMenu(fileName = "Battle Data", menuName = "KJY/Battle Data", order = 0)]
    public class BattleDataSO : ScriptableObject
    {
        public List<PlayerDataSO> playerDataList;
        public List<EnemyDataSO> enemyDataList;
        public float maxRiskPenalty, riskLevelIncrease, riskLevelIncreaseMulti;
    }
}