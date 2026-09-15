using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;

namespace Members.KJY._01.Scripts.Events.Dice.Selector
{
    public class OnEnemySelect : GameEvent
    {
        public EnemySelector EnemySelector {get; private set;}
        public OnEnemySelect(EnemySelector enemySelector)
        {
            EnemySelector = enemySelector;
        }
    }
}