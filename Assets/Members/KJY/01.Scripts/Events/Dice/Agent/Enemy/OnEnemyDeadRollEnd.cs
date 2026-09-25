using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyDeadRollEnd : GameEvent
    {
        public DiceFaceType DiceFaceType {get; private set;}
        public EnemyType EnemyType {get; private set;}
        
        public OnEnemyDeadRollEnd(DiceFaceType diceFaceType , EnemyType enemyType)
        {
            DiceFaceType = diceFaceType;
            EnemyType = enemyType;
        }
    }
}