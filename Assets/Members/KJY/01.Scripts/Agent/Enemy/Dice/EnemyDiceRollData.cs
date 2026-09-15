using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public struct EnemyDiceRollData
    {
        public readonly EnemyNumber enemyType;
        public readonly DiceFaceType diceFaceType;

        public EnemyDiceRollData(EnemyNumber enemyType, DiceFaceType diceFaceType)
        {
            this.enemyType = enemyType;
            this.diceFaceType = diceFaceType;
        }
    }
}