    using System;
    using System.Collections.Generic;
    using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using Members.KJY._01.Scripts.Service;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public class EnemySelector : AbstractSelector
    {
        [SerializeField] private EnemyType enemyType;
        private IGetCurrentEnemyParty _getEnemyData;
        private bool _isDeadRolled;
        
        [field:SerializeField] public EnemyDataSO RuntimeEnemyData { get; private set; }
        public bool IsNoneData => RuntimeEnemyData == null;
        public UnityEvent onInit;

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDeadRoll>(HandleDeadRoll);
            eventChannel.AddListener<OnEnemyDataReceive>(HandleDataReceive);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDeadRoll>(HandleDeadRoll);
            eventChannel.RemoveListener<OnEnemyDataReceive>(HandleDataReceive);
        }

        // protected override void Start()
        // {
        //     base.Start();
        //     _getEnemyData = ServiceLocator.Get<IGetCurrentEnemyParty>();
        //     EnemyDataChanged(_getEnemyData.GetData(0)); // ㅌㅅㅌ
        // }

        protected override void HandleDead()
        {
            if (IsDead || IsNoneData) return;
            base.HandleDead();
            RuntimeEnemyData.Dead();
            eventChannel.RaiseEvent(new OnEnemyDead(enemyType, true));
        }

        private void HandleDeadRoll(OnEnemyDeadRoll grb)
        {
            if (_isDeadRolled) return;
            if (!IsDead) return;
            _isDeadRolled = true;
            eventChannel.RaiseEvent(new OnEnemyRoll(enemyType, true, EnemyRollType.DeadRoll));
        }

        public void Init()
        {
            if (IsNoneData) return;
            EnterBattle();
            IsDead = false;
            IsSelect = false;
            _isDeadRolled = false;
            ValidateData();
            MyAgent.HealthModule.InitHealth(RuntimeEnemyData.MaxHealth);
            onUnSelect?.Invoke();
            eventChannel.RaiseEvent(new OnEnemyDead(enemyType, false));
            eventChannel.RaiseEvent(new OnEnemyDataChanged(RuntimeEnemyData, enemyType));
            DiceInventory.DiceDataChanged();
            onInit?.Invoke();
            // eventChannel.RaiseEvent(new OnEnemyRoll(enemyType, false, EnemyRollType.Roll));
        }

        public void ClearData()
        {
            RuntimeEnemyData = null;
            AgentData = null;
            IsDead = true;
            IsSelect = false;
            eventChannel.RaiseEvent(new OnEnemyDataChanged(null, enemyType));
        }
        
        protected override void Select()
        {
            if (IsDead || IsNoneData) return;
            eventChannel.RaiseEvent(new OnEnemySelect(this));
            onSelect?.Invoke();
        }
        
        protected override void UnSelect()
        {
            onUnSelect?.Invoke();
        }

        private void ChangeEnemyData(EnemyDataSO newEnemyData)
        {
            RuntimeEnemyData = newEnemyData;
            AgentData = newEnemyData;
            if (isActiveAndEnabled) ValidateData();
        }
        
        private void HandleDataReceive(OnEnemyDataReceive evt)
        {
            if (evt.EnemyType != enemyType) return;
            ChangeEnemyData(evt.EnemyDataData);
        }

        public void ValidateData()
        {
            if (RuntimeEnemyData == null) return;
            IconImage.SetImage(RuntimeEnemyData.EnemyImage);
            IconImage.SetColor(RuntimeEnemyData.ImageColor);
            MyAgent.AgentRenderer.SetSprite(RuntimeEnemyData.EnemyImage);
            MyAgent.AgentRenderer.SetColor(RuntimeEnemyData.ImageColor);
            MyAgent.AnimCompo.SetController(RuntimeEnemyData.EnemyAc);
        }

        
        public override void OnAttackCommand() { }
        public override void ApplyDamage(float damage)
        {
            MyAgent.HealthModule.TakeDamage(damage);
        }

        public override void ApplyHeal(float heal)
        { MyAgent.HealthModule.Heal(heal); }

        public override float GetLevel()
        {
            return ServiceLocator.Get<IGetRiskPenalty>().GetRiskPenalty();
        }

        #region Handle

        public void EnemyDataChanged(EnemyDataSO newEnemyData)
        {
            if (newEnemyData == null) return;
            ChangeEnemyData(newEnemyData);
            if (isActiveAndEnabled)
                eventChannel.RaiseEvent(new OnEnemyDataChanged(newEnemyData,enemyType));
        }
        

        #endregion
        
        #if UNITY_EDITOR
        
        private void OnValidate()
        {
            gameObject.name = $"{nameof(EnemySelector)} ({enemyType})";
        }
        
        #endif
    }
}
