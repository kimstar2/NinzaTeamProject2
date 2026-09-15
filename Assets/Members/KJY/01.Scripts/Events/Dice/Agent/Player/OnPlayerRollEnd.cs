using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Player
{
    public class OnPlayerRollEnd : GameEvent
    {
        public PlayerType PlayerType {get; private set;}
        public DiceFaceType DiceFaceType {get; private set;}
        
        public OnPlayerRollEnd(DiceFaceType diceFaceFaceType, PlayerType playerType)
        {
            DiceFaceType = diceFaceFaceType;
            PlayerType = playerType;
        }
    }
}