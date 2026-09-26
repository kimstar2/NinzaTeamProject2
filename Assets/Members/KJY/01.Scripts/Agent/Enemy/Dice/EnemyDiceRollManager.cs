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
        [field:SerializeField] public bool IsDead {get; private set;}
        public bool DiceRollEnd { get; private set; } = true;
        public bool IsRolling { get; private set; }
        public bool DeadRollEnd {get; private set;} = true;
        public UnityEvent<EnemyDiceRollData> onRollEnd;
        
        public void OnRollEnd(DiceFaceType diceFaceType)
        {
            onRollEnd?.Invoke(new EnemyDiceRollData(EnemyType, diceFaceType));
            DiceRollEnd = true;
            IsRolling = false;
        }
        
        public void OnRoll()
        {
            DiceRollEnd = false;
            IsRolling = true;
        }
        
        public void OnDead()
        {
            if (IsDead) return;
            IsDead = true;
            IsRolling = false;
            DeadRollEnd = false;
        }

        public void OnDeadRollEnd() => DeadRollEnd = true;

        public void Init(bool hasUnit)
        {
            IsDead = !hasUnit;
            DeadRollEnd = true;
            DiceRollEnd = !hasUnit;
            IsRolling = false;
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
            eventChannel.AddListener<OnEnemyDeadRollEnd>(HandleDeadRollEnd);
            eventChannel.AddListener<OnEnemyDataChanged>(HandleDataChanged);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyRollRaise>(HandleRoll);
            eventChannel.RemoveListener<OnEnemyRollEnd>(HandleRollEnd);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
            eventChannel.RemoveListener<OnEnemyDeadRollEnd>(HandleDeadRollEnd);
            eventChannel.RemoveListener<OnEnemyDataChanged>(HandleDataChanged);
        }

        private void HandleRollEnd(OnEnemyRollEnd evt)
        {
            EnemyDiceRollCheck check = DiceRollCheckList.Find(x => x.EnemyType == evt.EnemyType);
            if (check == null || check.IsDead) return;
            check.OnRollEnd(evt.DiceFaceType);
            CheckAllRollEnd();
        }

        private void HandleDeadRollEnd(OnEnemyDeadRollEnd evt)
        {
            EnemyDiceRollCheck check = DiceRollCheckList.Find(x => x.EnemyType == evt.EnemyType);
            if (check == null || !check.IsDead || check.DeadRollEnd) return;
            check.OnDeadRollEnd();
            CheckAllRollEnd();
        }

        private void CheckAllRollEnd()
        {
            bool wasComplete = AllDiceRollEnd;
            AllDiceRollEnd = DiceRollCheckList.TrueForAll(x => x.IsDead ? x.DeadRollEnd : x.DiceRollEnd);
            if (AllDiceRollEnd && !wasComplete)
                onAllDiceRollEnd?.Invoke();
        }

        private void HandleRoll(OnEnemyRollRaise evt) => RollLogic(evt.OnlyPending);

        public void Roll() => RollLogic();

        protected override void RollLogic() => RollLogic(false);

        private void RollLogic(bool onlyPending)
        {
            AllDiceRollEnd = false;
            foreach (EnemyDiceRollCheck check in DiceRollCheckList)
            {
                if (check.IsDead) continue;
                if (check.IsRolling) continue;
                if (onlyPending && check.DiceRollEnd) continue;
                check.OnRoll();
                eventChannel.RaiseEvent(new OnEnemyRoll(check.EnemyType,check.IsDead , EnemyRollType.Roll));
            }
            CheckAllRollEnd();
        }

        private void HandleEnemyDead(OnEnemyDead evt)
        {
            EnemyDiceRollCheck check = DiceRollCheckList.Find(x => x.EnemyType == evt.enemyType);
            if (check == null) return;
            if (evt.isDead)
                check.OnDead();
            else
                check.Init(true);
            CheckAllRollEnd();
        }

        private void HandleDataChanged(OnEnemyDataChanged evt)
        {
            if (evt.EnemyData != null) return;
            DiceRollCheckList.Find(x => x.EnemyType == evt.EnemyType)?.Init(false);
            CheckAllRollEnd();
        }
    }
}
