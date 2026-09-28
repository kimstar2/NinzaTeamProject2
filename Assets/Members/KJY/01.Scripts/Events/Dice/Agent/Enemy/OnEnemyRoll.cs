using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public enum EnemyRollType
    {
        DeadRoll,
        Roll,
        Refresh // 수치만 갱신
    }
    public class OnEnemyRoll : GameEvent
    {
        public readonly EnemyType enemyType;
        public readonly bool isDead;
        public readonly EnemyRollType rollType;

        public OnEnemyRoll(EnemyType enemyType, bool isDead , EnemyRollType rollType)
        {
            this.enemyType = enemyType;
            this.isDead = isDead;
            this.rollType = rollType;
        }
        
    }
}