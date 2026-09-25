using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;
using UnityEngine.Events;
using ZLinq;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    public class PlayerDiceInventory : AbstractDiceInventory
    {
        [SerializeField] private PlayerType playerType;
        private bool _isLocked;
        private AbstractSelector _mySelector;
        
        [SerializeField] private float receiveDelay;
        private CancellationTokenSource _cts;
        public UnityEvent onRollReceived;
        public UnityEvent onLockedReceived;
        
        private void Start() => DiceDataChanged();

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _mySelector = owner as AbstractSelector;
        }
        
        private void OnEnable() => eventChannel.AddListener<OnDiceLock>(HandleDiceLock);
        private void OnDisable()
        {
            KillApply();
            eventChannel.RemoveListener<OnDiceLock>(HandleDiceLock);
        }

        
        private void HandleDiceLock(OnDiceLock evt)
        {
            if (playerType != evt.PlayerType) return;
            _isLocked = evt.IsLock;
        }

        public override void DiceDataChanged()
        {
            if (_mySelector == null || _mySelector.AgentData == null) return;
            eventChannel.RaiseEvent(new OnPlayerDiceDataChanged(playerType, RunTimeDiceDataList, _mySelector.AgentData.AttackType));
        }

        public void HandleRollEnd(PlayerDiceRollData playerDiceRollData)
        {
            if (playerDiceRollData.playerType != playerType) return;
            savedDiceData = RunTimeDiceDataList.GetDiceData(playerDiceRollData.diceFaceType);
        }

        public override void Apply()
        {
            if (!isActiveAndEnabled || _mySelector.IsDead) return;
            KillApply();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            ApplyDelay(token).Forget();
        }

        private async UniTask ApplyDelay(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(receiveDelay) , cancellationToken:token);
            if (_mySelector.IsDead) return;
            eventChannel.RaiseEvent(new OnPlayerDiceDataBind(savedDiceData, playerType, _mySelector.GetLevel()));

            if (_isLocked)
                onLockedReceived?.Invoke();
            else
                onRollReceived?.Invoke();
        }

        private void KillApply()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
