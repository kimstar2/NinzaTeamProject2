using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Player;

namespace Members.KJY._01.Scripts.Events.Dice
{
    public class OnDiceLock : GameEvent
    {
        public PlayerType PlayerType {get; private set;}
        public bool IsLock {get; private set;}
        public OnDiceLock(bool isLock, PlayerType playerType)
        {
            PlayerType = playerType;
            IsLock = isLock;
        }
    }
}