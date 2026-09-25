using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Player
{
    public class OnPlayerDataReceive : GameEvent
    {
        public PlayerDataSO PlayerDataData {get; private set;} 
        public OnPlayerDataReceive(PlayerDataSO playerData)
        {
            PlayerDataData = playerData;
        }
    }
}