using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class DiceBattleRaiser : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;
        
        public void BattleStart()
        {
            eventChannel.RaiseEvent(new OnStartBattle());
        }
    }
}