using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice.Battle;
using UnityEngine;

namespace Members.KJY._01.Scripts.Events
{
    public class OnBattleResult : GameEvent
    {
        public BattleResult BattleResult {get; private set;}

        public OnBattleResult(BattleResult battleResult)
        {
            BattleResult = battleResult;
        }
    }
}