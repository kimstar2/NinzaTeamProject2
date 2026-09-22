using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public struct EnemyDiceRollData
    {
        public readonly EnemyType enemyType;
        public readonly DiceFaceType diceFaceType;

        public EnemyDiceRollData(EnemyType enemyType, DiceFaceType diceFaceType)
        {
            this.enemyType = enemyType;
            this.diceFaceType = diceFaceType;
        }
    }
}