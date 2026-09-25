using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public class EnemyDeadRollRaiser : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;

        public void OnDeadRoll()
        {
            eventChannel.RaiseEvent(new OnEnemyDeadRoll());
        }
    }
}