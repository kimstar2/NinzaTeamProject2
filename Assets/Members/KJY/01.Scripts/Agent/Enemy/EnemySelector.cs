using System;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public class EnemySelector : AbstractSelector
    {
        [SerializeField] private EnemyDataSO defaultEnemyData;
        [SerializeField] private EnemyDataSO runtimeEnemyData;
        [SerializeField] private EnemyNumber enemyType;

        protected override void InitializeModules()
        {
            base.InitializeModules();
            runtimeEnemyData = Instantiate(defaultEnemyData);
        }

        private void Start()
        {
            ValidateData();
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
        }

        protected override void Select()
        {
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
            ValidateData();
        }

        private void ValidateData()
        {
            IconImage.SetImage(runtimeEnemyData.EnemyImage);
            IconImage.SetColor(runtimeEnemyData.ImageColor);
            MyAgent.AgentRenderer.SetSprite(runtimeEnemyData.EnemyImage);
            MyAgent.AgentRenderer.SetColor(runtimeEnemyData.ImageColor);
        }

        
        public override void OnAttackCommand() { }
        public override void ApplyDamage(float damage)
        {
            HealthModule.TakeDamage(damage);
        }

        public override void ApplyHeal(float heal)
        { }

        #region Handle

        private void HandleEnemyDataChanged(OnEnemyDataChanged evt)
        {
            if (evt.EnemyType != enemyType) return;
            ChangeEnemyData(evt.EnemyData);
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