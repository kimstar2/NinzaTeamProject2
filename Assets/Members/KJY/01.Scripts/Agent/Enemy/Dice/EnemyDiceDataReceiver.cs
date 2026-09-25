using DG.Tweening;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDiceDataReceiver : AbstractDiceDataReceiver
    {
        [SerializeField] private EnemyType enemyType;
        public UnityEvent onEnemyDead;

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDiceDataChanged>(HandleDiceDataChanged);
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDiceDataChanged>(HandleDiceDataChanged);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
        }
        
        private void HandleEnemyDead(OnEnemyDead obj)
        {
            if (obj.enemyType != enemyType || !obj.isDead) return;
            onEnemyDead?.Invoke();
        }

        private void HandleDiceDataChanged(OnEnemyDiceDataChanged evt)
        {
            if (evt.EnemyType != enemyType) return;
            ApplyDiceData(evt.DiceDataList, evt.AttackType);
        }
    }
}
