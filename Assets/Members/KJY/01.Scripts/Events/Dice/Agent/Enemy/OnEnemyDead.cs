using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyDead : GameEvent
    {
        public readonly EnemyType enemyType;
        public readonly bool isDead;
        
        public OnEnemyDead(EnemyType enemyType, bool isDead)
        {
            this.isDead = isDead;;
            this.enemyType = enemyType;
        }
    }
}