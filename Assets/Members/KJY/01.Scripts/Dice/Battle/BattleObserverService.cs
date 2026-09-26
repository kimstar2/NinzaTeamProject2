using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Command;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using ZLinq;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class BattleObserverService : MonoModule , IBattleObserverService
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private float commandDelay;
        [field:SerializeReference] public List<ICommand> BattleCommandList { get; private set; } = new();
        public CancellationTokenSource BattleCts {get; private set;}
        public bool IsBattle {get; private set;}
        public bool HasBattleResult {get; private set;}
        private ICommand _currentCommand;

        public void StartBattle() // 배틀 시작
        {
            IsBattle = true;
            ExecuteAsync().Forget();
        }

        private void Awake()
        {
            ServiceLocator.Register<IBattleObserverService>(this);
        }
        
        private void OnDestroy()
        {
            KillTask();
            ServiceLocator.UnRegister<IBattleObserverService>();
        }
        
        private void OnEnable()
        {
            eventChannel.AddListener<OnExecuteNextCommand>(HandleExecuteNextCommand);
            eventChannel.AddListener<OnBattleResult>(HandleBattleResult);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnExecuteNextCommand>(HandleExecuteNextCommand);
            eventChannel.RemoveListener<OnBattleResult>(HandleBattleResult);
        }

        /// <summary>
        /// 비동기 행동 시퀀스
        /// ICommand 라는 명령객체로 행동
        /// 커맨드들을 순차적으로 실행함
        /// </summary>
        public async UniTask ExecuteAsync() 
        {
            KillTask();
            BattleCts = new CancellationTokenSource();
            CancellationToken ct = BattleCts.Token;
            
            foreach (ICommand command in BattleCommandList.AsValueEnumerable().ToList())
            {
                _currentCommand = command;
                
                _currentCommand.SetNextSignal(ct);
                await _currentCommand.ExecuteAction();
                RemoveCommand(_currentCommand);
                if (HasBattleResult) break;
                await UniTask.Delay(TimeSpan.FromSeconds(commandDelay), cancellationToken:ct);
            }
            ClearCommands();
            IsBattle = false;
            eventChannel.RaiseEvent(new OnEndBattle());
            if (HasBattleResult) return;
            eventChannel.RaiseEvent(new OnEnemyRollRaise());
            eventChannel.RaiseEvent(new OnPlayerRoll(PlayerType.All));
        }

        private void HandleExecuteNextCommand(OnExecuteNextCommand garbage)
        {
            _currentCommand?.MoveNext();
        }

        private void HandleBattleResult(OnBattleResult evt) => HasBattleResult = true;

        public void AddCommand(ICommand command) => BattleCommandList.Add(command);
        public void RemoveCommand(ICommand command) => BattleCommandList.Remove(command);
        
        private void KillTask()
        {
            if (BattleCts == null) return;
            BattleCts.Cancel();
            BattleCts.Dispose();
            BattleCts = null;
        }

        private void ClearCommands() => BattleCommandList.Clear();
    }
}
