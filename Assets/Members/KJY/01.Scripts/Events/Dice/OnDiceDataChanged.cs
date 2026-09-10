using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice
{
    public class OnDiceDataChanged : GameEvent
    {
        public PlayerType PlayerType {get; private set;}
        public DiceDataListSO DiceDataList {get; private set;}
        
        public OnDiceDataChanged(PlayerType playerType , DiceDataListSO diceDataList)
        {
            PlayerType = playerType;
            DiceDataList = diceDataList;
        }

        public PlayerType GetPlayerTypeUsingInteger(int d)
        {
            return PlayerType;
        }
    }
}