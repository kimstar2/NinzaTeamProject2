using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Player;
using Members.KJY._01.Scripts.Services;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceRollReceiver : MonoModule
    {
        [SerializeField] private PlayerType playerType;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private float receiveDelay;

        [Header("Lock Setting")] 
        [SerializeField] private UIMonoImage diceImage;
        [SerializeField] private Color onLockColor;
        [SerializeField] private Color offLockColor;
        public bool IsLocked {get; private set;}
        
        private CancellationTokenSource _cts;
            
        public UnityEvent onRollReceived;

        private void OnEnable()
        {
            eventChannel.AddListener<OnRollEnd>(HandleRollEnd);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnRollEnd>(HandleRollEnd);
        }
        
        private void HandleRollEnd(OnRollEnd garbageEvt)
        {
            if (IsLocked) return;
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            ReceiveDelay(token).Forget();
        }

        public void LockToggle()
        {
            IsLocked = !IsLocked;
            eventChannel.RaiseEvent(new OnDiceLock(IsLocked,playerType));
            if (IsLocked)
                OnLock();
            else
                OffLock();
        }

        private void OnLock() => diceImage.SetColor(onLockColor);

        private void OffLock() => diceImage.SetColor(offLockColor);
        
        private async UniTask ReceiveDelay(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(receiveDelay) , cancellationToken:token);
            onRollReceived?.Invoke();
        }
    }
}