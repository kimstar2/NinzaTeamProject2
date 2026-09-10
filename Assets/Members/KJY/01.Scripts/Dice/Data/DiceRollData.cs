using Members.KJY._01.Scripts.Agent.Player;

namespace Members.KJY._01.Scripts.Dice.Data
{
    public struct DiceRollData
    {
        public readonly PlayerType playerType;
        public readonly DiceFaceType diceFaceType;

        public DiceRollData(PlayerType playerType, DiceFaceType diceFaceType)
        {
            this.playerType = playerType;
            this.diceFaceType = diceFaceType;
        }
    }
}