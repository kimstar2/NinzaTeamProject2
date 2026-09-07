using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Player;

namespace Members.KJY._01.Scripts.Events.Dice
{
    public class OnRollEnd : GameEvent
    {
        public PlayerType PlayerType {get; private set;}
        public DiceFaceType DiceFaceType {get; private set;}
        
        public OnRollEnd(DiceFaceType diceFaceFaceType, PlayerType playerType)
        {
            DiceFaceType = diceFaceFaceType;
            PlayerType = playerType;
        }
    }
}