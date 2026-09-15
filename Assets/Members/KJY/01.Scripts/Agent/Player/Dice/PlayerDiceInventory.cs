using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
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

        public override void DiceDataChanged() => eventChannel.RaiseEvent(new OnPlayerDiceDataChanged(playerType, RunTimeDiceDataList));

        public void HandleRollEnd(PlayerDiceRollData playerDiceRollData)
        {
            if (playerDiceRollData.playerType != playerType) return;
            savedDiceData = RunTimeDiceDataList.GetDiceData(playerDiceRollData.diceFaceType);
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