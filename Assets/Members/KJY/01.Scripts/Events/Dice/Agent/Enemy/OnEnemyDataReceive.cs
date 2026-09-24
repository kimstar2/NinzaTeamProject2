using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyDataReceive : GameEvent
    {
        public EnemyDataSO EnemyDataData {get; private set;} 
        public EnemyType EnemyType {get; private set;} 
        
        public OnEnemyDataReceive(EnemyDataSO enemyData , EnemyType enemyType)
        {
            EnemyDataData = enemyData;
            EnemyType = enemyType;
        }
    }
}