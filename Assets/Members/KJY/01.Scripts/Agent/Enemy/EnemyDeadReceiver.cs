using System;
using System.Diagnostics.Tracing;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public class EnemyDeadReceiver : MonoModule
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private EnemyType enemyType;
        public UnityEvent onDead;
        public UnityEvent onRevive;

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void HandleEnemyDead(OnEnemyDead evt)
        {
            if (evt.enemyType != enemyType) return;
            if (evt.isDead)
                Dead();
            else
                Revive();
        }

        private void Dead()
        {
            onDead?.Invoke();
        }

        private void Revive()
        {
            onRevive?.Invoke();
        }
    }
}