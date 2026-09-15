using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyRollEnd : GameEvent
    {
        public DiceFaceType DiceFaceType {get; private set;}
        public EnemyNumber EnemyType {get; private set;}
        
        public OnEnemyRollEnd(DiceFaceType diceFaceType , EnemyNumber enemyType)
        {
            DiceFaceType = diceFaceType;
            EnemyType = enemyType;
        }
    }
}