using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Player
{
    public class OnPlayerDiceDataBind : GameEvent
    {
        public DiceDataSO DiceData {get; private set;}
        public PlayerType PlayerType {get; private set;}
        
        public OnPlayerDiceDataBind(DiceDataSO bindData , PlayerType playerType)
        {
            DiceData = bindData;
            PlayerType = playerType;
        }
    }
}