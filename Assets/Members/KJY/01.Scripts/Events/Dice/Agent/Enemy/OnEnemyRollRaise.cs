using DevLib.CoreLib.Runtime;
using UnityEngine;

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
