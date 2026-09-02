using DevLib.CoreLib.Runtime;
using Members.KJY.Scripts.Dice;

namespace Members.KJY.Scripts.Events
{
    public class OnDiceDataBind : GameEvent
    {
        public DiceDataSO DiceData {get; private set;}
        public OnDiceDataBind(DiceDataSO bindData)
        {
            DiceData  = bindData;
        }
    }
}