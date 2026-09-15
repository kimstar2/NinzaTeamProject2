using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDiceInventory : AbstractDiceInventory
    {
        [SerializeField] private EnemyNumber enemyType;
        [SerializeField] private float receiveDelay;
        private CancellationTokenSource _cts;
        public UnityEvent onRollReceived;

        private void Start() => DiceDataChanged();

        public override void DiceDataChanged()
        {
            eventChannel.RaiseEvent(new OnEnemyDiceDataChanged(enemyType,RunTimeDiceDataList));
        }

        public void HandleRollEnd(EnemyDiceRollData enemyDiceRollData)
        {
            if (enemyDiceRollData.enemyType != enemyType) return;
            savedDiceData = RunTimeDiceDataList.GetDiceData(enemyDiceRollData.diceFaceType); // 테스트
        }
        
        public override void Apply()
        {
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            ApplyDelay(token).Forget();
        }
        
        private async UniTask ApplyDelay(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(receiveDelay), cancellationToken: token);
            eventChannel.RaiseEvent(new OnEnemyDiceDataBind(savedDiceData, enemyType));
            onRollReceived?.Invoke();
        }
        
        private void OnValidate()
        {
            gameObject.name = $"{nameof(EnemyDiceInventory)} ({enemyType})";
        }
    }
}