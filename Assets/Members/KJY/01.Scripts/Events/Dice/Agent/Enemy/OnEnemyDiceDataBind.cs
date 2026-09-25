using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyDiceDataBind : GameEvent
    {
        public EnemyRollType RollType {get; private set;}
        public DiceDataSO DiceData { get; private set; }
        public EnemyType EnemyType { get; private set; }
        public float Level { get; private set; }
        public OnEnemyDiceDataBind(DiceDataSO diceData, EnemyType enemyType, EnemyRollType rollType, float level)
        {
            DiceData = diceData;
            EnemyType = enemyType;
            RollType = rollType;
            Level = level;
        }
    }
}
