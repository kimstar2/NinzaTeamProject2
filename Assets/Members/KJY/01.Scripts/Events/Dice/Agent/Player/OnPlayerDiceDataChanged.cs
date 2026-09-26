using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Player
{
    public class OnPlayerDiceDataChanged : GameEvent
    {
        public PlayerType PlayerType {get; private set;}
        public DiceDataListSO DiceDataList {get; private set;}
        public AgentAttackType AttackType {get; private set;}
        
        public OnPlayerDiceDataChanged(PlayerType playerType, DiceDataListSO diceDataList, AgentAttackType attackType)
        {
            PlayerType = playerType;
            DiceDataList = diceDataList;
            AttackType = attackType;
        }
    }
}
