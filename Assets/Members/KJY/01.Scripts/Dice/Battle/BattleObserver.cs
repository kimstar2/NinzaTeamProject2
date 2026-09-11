using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Command;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class BattleObserver : MonoModule
    {
        [SerializeField] private EventChannelSO eventChannel;
        
        [field:SerializeReference] public List<ICommand> CommandList { get; private set; } = new();
        public CancellationTokenSource BattleCts {get; private set;}
        private ICommand _currentCommand;

        public void StartBattle() // 배틀 시작
        {
            Debug.Log("배틀 시작");
            ExecuteAsync().Forget();
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnExecuteNextCommand>(HandleExecuteNextCommand);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnExecuteNextCommand>(HandleExecuteNextCommand);
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
            foreach (ICommand command in CommandList.ToList())
            {
                _currentCommand = command;
                _currentCommand.Execute();
                await _currentCommand.ExecuteAction(ct);
                CommandList.Remove(_currentCommand);
            }
            ClearCommands();
            eventChannel.RaiseEvent(new OnEndBattle());
        }

        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
                eventChannel.RaiseEvent(new OnExecuteNextCommand()); // 테스트 용도
        }

        private void HandleExecuteNextCommand(OnExecuteNextCommand garbage) => _currentCommand.MoveNext(); // 다음 행동 실행

        public void AddCommand(ICommand command) => CommandList.Add(command);
        public void RemoveCommand(ICommand command) => CommandList.Remove(command);
        
        private void KillTask()
        {
            if (BattleCts == null) return;
            BattleCts.Cancel();
            BattleCts.Dispose();
            BattleCts = null;
        }

        private void ClearCommands() => CommandList.Clear();

        private void OnDestroy()
        {
            KillTask();
        }
    }
}