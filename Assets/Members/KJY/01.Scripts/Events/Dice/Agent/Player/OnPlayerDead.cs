using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Player
{
    public class OnPlayerDead : GameEvent
    {
        public readonly PlayerType PlayerType;
        public readonly bool IsDead;
        public OnPlayerDead(PlayerType playerType, bool isDead)
        {
            PlayerType = playerType;
            IsDead = isDead;
        }
    }
}
