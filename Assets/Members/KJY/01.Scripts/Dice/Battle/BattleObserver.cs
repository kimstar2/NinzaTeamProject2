using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Dice.Command;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class BattleObserver : MonoModule
    {
        [SerializeField] private EventChannelSO eventChannel;
        
        [field:SerializeReference] public List<ICommand> commandList { get; private set; } = new();
        public CancellationTokenSource BattleCts {get; private set;}

        public void StartBattle() // 배틀 시작
        {
            Debug.Log("배틀 시작");
            ExecuteAsync().Forget();
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
            foreach (ICommand command in commandList)
            {
                command.Execute();
                await command.ExecuteAction(ct);
                Debug.Log("d");
            }
            ClearCommands();
            eventChannel.RaiseEvent(new OnEndBattle());
        }

        public void AddCommand(ICommand command) => commandList.Add(command);
        public void RemoveCommand(ICommand command) => commandList.Remove(command);
        
        private void KillTask()
        {
            if (BattleCts == null) return;
            BattleCts.Cancel();
            BattleCts.Dispose();
            BattleCts = null;
        }

        private void ClearCommands() => commandList.Clear();

        private void OnDestroy()
        {
            KillTask();
        }
    }
}