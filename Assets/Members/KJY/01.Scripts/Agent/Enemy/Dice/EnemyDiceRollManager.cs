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
        [field:SerializeField] public EnemyType EnemyType {get; private set;}
        [field:SerializeField] public bool DiceRollEnd {get; private set;}
        [field:SerializeField] public bool IsDead {get; private set;}
        public UnityEvent<EnemyDiceRollData> onRollEnd;
        
        public void OnRollEnd(DiceFaceType diceFaceType)
        {
            onRollEnd?.Invoke(new EnemyDiceRollData(EnemyType, diceFaceType));
            DiceRollEnd = true;
        }
        
        public void OnRoll()
        {
            DiceRollEnd = false;
        }
        
        public void OnDead()
        {
            IsDead = true;
        }
    }
    public class EnemyDiceRollManager : AbstractDiceRollManager
    {
        [field:SerializeField] public List<EnemyDiceRollCheck> DiceRollCheckList { get; private set; }


        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyRollRaise>(HandleRoll);
            eventChannel.AddListener<OnEnemyRollEnd>(HandleRollEnd);
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyRollRaise>(HandleRoll);
            eventChannel.RemoveListener<OnEnemyRollEnd>(HandleRollEnd);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void HandleRollEnd(OnEnemyRollEnd evt)
        {
            EnemyDiceRollCheck check = DiceRollCheckList.Find(x => x.EnemyType == evt.EnemyType);
            check.OnRollEnd(evt.DiceFaceType);

            AllDiceRollEnd = DiceRollCheckList.TrueForAll(x => x.DiceRollEnd || x.IsDead);
            
            if (AllDiceRollEnd)
                onAllDiceRollEnd?.Invoke();
        }

        private void HandleRoll(OnEnemyRollRaise garbage)
        {
            RollLogic();
        }

        protected override void RollLogic()
        {
            if (!AllDiceRollEnd) return;
            foreach (EnemyDiceRollCheck check in DiceRollCheckList)
            {
                check.OnRoll();
                eventChannel.RaiseEvent(new OnEnemyRoll(check.EnemyType,check.IsDead));
            }
            
            AllDiceRollEnd = false;
        }

        private void HandleEnemyDead(OnEnemyDead evt)
        {
            EnemyDiceRollCheck check = DiceRollCheckList.Find(x => x.EnemyType == evt.enemyType);
            Debug.Log("Real Dead");
            check.OnDead();
        }
    }
}