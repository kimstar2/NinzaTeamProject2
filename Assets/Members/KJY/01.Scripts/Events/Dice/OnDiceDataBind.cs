using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice
{
    public class OnDiceDataBind : GameEvent
    {
        public DiceDataSO DiceData { get; private set; }
        
        public OnDiceDataBind(DiceDataSO diceData)
        {
            DiceData = diceData;
        }
    }
}