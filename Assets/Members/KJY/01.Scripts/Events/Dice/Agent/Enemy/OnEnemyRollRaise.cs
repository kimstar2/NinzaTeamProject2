using DevLib.CoreLib.Runtime;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyRollRaise : GameEvent
    {
        public readonly bool OnlyPending;

        public OnEnemyRollRaise(bool onlyPending = false)
        {
            OnlyPending = onlyPending;
        }
    }
}
