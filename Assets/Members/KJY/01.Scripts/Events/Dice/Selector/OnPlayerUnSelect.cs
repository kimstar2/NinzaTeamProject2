using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;

namespace Members.KJY._01.Scripts.Events.Dice.Selector
{
    public class OnPlayerUnSelect : GameEvent
    {
        public PlayerType PlayerType { get; private set; }
        public OnPlayerUnSelect(PlayerType playerType)
        {
            PlayerType = playerType;
        }
    }
}