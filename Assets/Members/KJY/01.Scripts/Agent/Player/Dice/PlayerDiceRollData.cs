using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    public struct PlayerDiceRollData
    {
        public readonly PlayerType playerType;
        public readonly DiceFaceType diceFaceType;

        public PlayerDiceRollData(PlayerType playerType, DiceFaceType diceFaceType)
        {
            this.playerType = playerType;
            this.diceFaceType = diceFaceType;
        }
    }
}