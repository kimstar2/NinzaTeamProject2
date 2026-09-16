using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyRoll : GameEvent
    {
        public readonly EnemyType enemyType;
        public readonly bool isDead;

        public OnEnemyRoll(EnemyType enemyType, bool isDead)
        {
            this.enemyType = enemyType;
            this.isDead = isDead;
        }
        
    }
}