using System;
using System.Diagnostics.Tracing;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public class Enemy : AbstractAgent
    {
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private EventChannelSO eventChannel;
        public UnityEvent onDeadRollEnd;
        public UnityEvent onInit;
            
        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDeadRollEnd>(HandleEnemyDeadRollEnd);
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDeadRollEnd>(HandleEnemyDeadRollEnd);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void HandleEnemyDead(OnEnemyDead evt)
        {
            if (evt.enemyType != enemyType || evt.isDead) return;
            onInit?.Invoke();
        }

        private void HandleEnemyDeadRollEnd(OnEnemyDeadRollEnd evt)
        {
            if (evt.EnemyType != enemyType) return;
            onDeadRollEnd?.Invoke();
        }
        
    }
}
