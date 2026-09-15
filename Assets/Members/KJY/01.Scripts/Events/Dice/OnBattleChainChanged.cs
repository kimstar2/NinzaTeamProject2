using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;

namespace Members.KJY._01.Scripts.Events.Dice
{
    public class OnBattleChainChanged : GameEvent
    {
        public readonly PlayerDataSO playerData;
        public readonly int chainCount;
        public readonly bool isAdd;

        public OnBattleChainChanged(PlayerDataSO playerData,int chainCount , bool isAdd)
        {
            this.playerData = playerData;
            this.isAdd = isAdd;
            this.chainCount = chainCount;
        }
    }
}