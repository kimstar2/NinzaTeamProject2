using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Player;

namespace Members.KJY._01.Scripts.Events.Dice
{
    public class OnDiceDataBind : GameEvent
    {
        public DiceDataSO DiceData {get; private set;}
        public PlayerType PlayerType {get; private set;}
        
        public OnDiceDataBind(DiceDataSO bindData , PlayerType playerType)
        {
            DiceData = bindData;
            PlayerType = playerType;
        }
    }
}