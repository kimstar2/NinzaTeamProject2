using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Events.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDiceInventory : AbstractDiceInventory
    {
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private float receiveDelay;
        private CancellationTokenSource _cts;
        private AbstractSelector _mySelector;
        private bool _deadRollReceived;
        public UnityEvent onRollReceived;
        public UnityEvent onDeadRollReceived;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _mySelector = owner as AbstractSelector;
            _deadRollReceived = false;
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDeadRollEnd>(HandleEnemyDeadRollEnd);
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
            eventChannel.AddListener<OnRiskPenaltyChanged>(HandleRiskPenaltyChanged);
        }
        

        private void OnDisable()
        {
            KillApply();
            eventChannel.RemoveListener<OnEnemyDeadRollEnd>(HandleEnemyDeadRollEnd);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
            eventChannel.RemoveListener<OnRiskPenaltyChanged>(HandleRiskPenaltyChanged);

        }

        private void HandleEnemyDead(OnEnemyDead evt)
        {
            if (evt.enemyType != enemyType || evt.isDead) return;
            KillApply();
            _deadRollReceived = false;
            savedDiceData = RunTimeDiceDataList.GetDiceData(DiceFaceType.Front);
        }

        private void HandleEnemyDeadRollEnd(OnEnemyDeadRollEnd evt)
        {
            if (evt.EnemyType != enemyType) return;
            if (_deadRollReceived || !_mySelector.IsDead) return;
            savedDiceData = RunTimeDiceDataList.GetDiceData(evt.DiceFaceType);
            if (savedDiceData == null) return;

            _deadRollReceived = true;
            KillApply();
            eventChannel.RaiseEvent(new OnEnemyDiceDataBind(savedDiceData, enemyType, EnemyRollType.DeadRoll, _mySelector.GetLevel()));
            onDeadRollReceived?.Invoke();
        }

        private void Start() => DiceDataChanged();

        public override void DiceDataChanged()
        {
            eventChannel.RaiseEvent(new OnEnemyDiceDataChanged(enemyType,RunTimeDiceDataList));
        }

        public void HandleRollEnd(EnemyDiceRollData enemyDiceRollData)
        {
            if (enemyDiceRollData.enemyType != enemyType) return;
            savedDiceData = RunTimeDiceDataList.GetDiceData(enemyDiceRollData.diceFaceType);
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
            await UniTask.Delay(TimeSpan.FromSeconds(receiveDelay), cancellationToken: token);
            RealApply();
        }

        public void RealApply()
        {
            if (_mySelector.IsDead) return;
            eventChannel.RaiseEvent(new OnEnemyDiceDataBind(savedDiceData, enemyType, EnemyRollType.Roll, _mySelector.GetLevel()));
            onRollReceived?.Invoke();
        }
        
        private void HandleRiskPenaltyChanged(OnRiskPenaltyChanged obj)
        {
            if (_mySelector.IsDead) return;
            eventChannel.RaiseEvent(new OnEnemyDiceDataBind(savedDiceData, enemyType, EnemyRollType.Roll, obj.PenaltyValue));
            onRollReceived?.Invoke();
        }

        private void KillApply()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        
        private void OnValidate()
        {
            gameObject.name = $"{nameof(EnemyDiceInventory)} ({enemyType})";
        }
    }
}
