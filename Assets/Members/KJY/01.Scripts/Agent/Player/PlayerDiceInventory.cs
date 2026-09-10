using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Player
{
    public class PlayerDiceInventory : AbstractDiceInventory
    {
        [SerializeField] private PlayerType playerType;
        private bool _isLocked;
        
        [SerializeField] private float receiveDelay;
        private CancellationTokenSource _cts;
        public UnityEvent onRollReceived;
        public UnityEvent onLockedReceived;
        
        private void Start() => DiceDataChanged();
        
        private void OnEnable() => eventChannel.AddListener<OnDiceLock>(HandleDiceLock);
        private void OnDisable() => eventChannel.RemoveListener<OnDiceLock>(HandleDiceLock);

        
        private void HandleDiceLock(OnDiceLock evt)
        {
            if (playerType != evt.PlayerType) return;
            _isLocked = evt.IsLock;
            Debug.Log("IsLocked: " + _isLocked);
        }

        public override void DiceDataChanged() => eventChannel.RaiseEvent(new OnDiceDataChanged(playerType, RunTimeDiceDataList));

        public void HandleRollEnd(DiceRollData diceRollData)
        {
            if (diceRollData.playerType != playerType) return;
            savedDiceData = RunTimeDiceDataList.GetDiceData(diceRollData.diceFaceType);
        }

        public override void Apply()
        {
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            ApplyDelay(token).Forget();
        }

        private async UniTask ApplyDelay(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(receiveDelay) , cancellationToken:token);
            eventChannel.RaiseEvent(new OnPlayerDiceDataBind(savedDiceData, playerType));

            if (_isLocked)
                onLockedReceived?.Invoke();
            else
                onRollReceived?.Invoke();
        }
    }
}