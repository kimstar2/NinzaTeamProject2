    using System;
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
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public class EnemySelector : AbstractSelector
    {
        [SerializeField] private EnemyDataSO defaultEnemyData;
        [SerializeField] private EnemyDataSO runtimeEnemyData;
        [SerializeField] private EnemyType enemyType;
        private IGetCurrentEnemyParty _getEnemyData;
            
        protected override void InitializeModules()
        {
            base.InitializeModules();
            runtimeEnemyData = Instantiate(defaultEnemyData);
        }

        protected override void Start()
        {
            base.Start();
            _getEnemyData = ServiceLocator.Get<IGetCurrentEnemyParty>();
            EnemyDataChanged(_getEnemyData.GetData(0)); // ㅌㅅㅌ
        }

        private void Update()
        {
            // if (Keyboard.current.eKey.wasPressedThisFrame) // ㅌㅅㅌ
                // EnemyDataChanged(_getEnemyData.GetData(0));
        }

        protected override void HandleDead()
        {
            base.HandleDead();
            eventChannel.RaiseEvent(new OnEnemyDead(enemyType, true));
        }

        protected override void Select()
        {
            if (IsDead) return;
            eventChannel.RaiseEvent(new OnEnemySelect(this));
            onSelect?.Invoke();
        }
        
        protected override void UnSelect()
        {
            onUnSelect?.Invoke();
        }

        private void ChangeEnemyData(EnemyDataSO newEnemyData)
        {
            runtimeEnemyData = newEnemyData;
            AgentData = newEnemyData;
            ValidateData();
        }

        public void ValidateData()
        {
            if (runtimeEnemyData == null) return;
            IconImage.SetImage(runtimeEnemyData.EnemyImage);
            IconImage.SetColor(runtimeEnemyData.ImageColor);
            Debug.Log("DDDD");
            MyAgent.AgentRenderer.SetSprite(runtimeEnemyData.EnemyImage);
            MyAgent.AgentRenderer.SetColor(runtimeEnemyData.ImageColor);
            MyAgent.AnimCompo.SetController(runtimeEnemyData.EnemyAc);
        }

        
        public override void OnAttackCommand() { }
        public override void ApplyDamage(float damage)
        {
            MyAgent.HealthModule.TakeDamage(damage);
        }

        public override void ApplyHeal(float heal)
        { MyAgent.HealthModule.Heal(heal); }

        #region Handle

        private void EnemyDataChanged(EnemyDataSO newEnemyData)
        {
            if (newEnemyData == null) return;
            if (newEnemyData == runtimeEnemyData) return;
            eventChannel.RaiseEvent(new OnEnemyDataChanged(newEnemyData,enemyType));
            ChangeEnemyData(newEnemyData);
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
