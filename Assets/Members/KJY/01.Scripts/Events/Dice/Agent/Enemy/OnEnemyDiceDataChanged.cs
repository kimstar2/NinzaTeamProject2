using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.KJY._01.Scripts.Events.Dice.Agent.Enemy
{
    public class OnEnemyDiceDataChanged : GameEvent
    {
        public EnemyType EnemyType {get; private set;}
        public DiceDataListSO DiceDataList {get; private set;}
        
        public OnEnemyDiceDataChanged(EnemyType enemyType , DiceDataListSO diceDataList)
        {
            EnemyType = enemyType;
            DiceDataList = diceDataList;
        }
    }
}