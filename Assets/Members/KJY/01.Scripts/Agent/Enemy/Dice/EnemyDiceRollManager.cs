using System;
using System.Collections.Generic;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    [Serializable]
    public class EnemyDiceRollCheck
    {
        [field:SerializeField] public EnemyNumber EnemyType {get; private set;}
        [field:SerializeField] public bool DiceRollEnd {get; private set;}
        public UnityEvent<EnemyDiceRollData> onRollEnd;
        public UnityEvent<EnemyNumber> onRoll;
        
        public void OnRollEnd(DiceFaceType diceFaceType)
        {
            onRollEnd?.Invoke(new EnemyDiceRollData(EnemyType, diceFaceType));
            DiceRollEnd = true;
        }
        
        public void OnRoll()
        {
            onRoll?.Invoke(EnemyType);
            DiceRollEnd = false;
        }
    }
    public class EnemyDiceRollManager : AbstractDiceRollManager
    {
        [field:SerializeField] public List<EnemyDiceRollCheck> DiceRollCheckList { get; private set; }


        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyRoll>(HandleRoll);
            eventChannel.AddListener<OnEnemyRollEnd>(HandleRollEnd);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyRoll>(HandleRoll);
            eventChannel.RemoveListener<OnEnemyRollEnd>(HandleRollEnd);
        }

        private void HandleRollEnd(OnEnemyRollEnd obj)
        {
            EnemyDiceRollCheck check = DiceRollCheckList.Find(x => x.EnemyType == obj.EnemyType);
            check.OnRollEnd(obj.DiceFaceType);

            AllDiceRollEnd = DiceRollCheckList.TrueForAll(x => x.DiceRollEnd);
            
            if (AllDiceRollEnd)
                onAllDiceRollEnd?.Invoke();
        }

        private void HandleRoll(OnEnemyRoll obj)
        {
            AllDiceRollEnd = false;
            foreach (EnemyDiceRollCheck check in DiceRollCheckList)
                check.OnRoll();
        }
    }
}