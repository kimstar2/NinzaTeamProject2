using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyDataChanged : GameEvent
    {
        public EnemyDataSO EnemyData { get; private set; }
        public EnemyNumber EnemyType { get; private set; }

        public OnEnemyDataChanged(EnemyDataSO enemyData, EnemyNumber enemyType)
        {
            EnemyData = enemyData;
            EnemyType = enemyType;
        }
    }
}