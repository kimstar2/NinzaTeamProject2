using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Player
{
    public class OnPlayerRoll : GameEvent
    {
        public readonly PlayerType playerType;

        public OnPlayerRoll(PlayerType playerType)
        {
            this.playerType = playerType;
        }
    }
}