using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent;
using UnityEngine;

namespace Members.KJY._01.Scripts.Command
{
    [Serializable]
    public class ActionCommand : ICommand
    {
        // 직렬화 시켜놓는 이유는 인스펙터에서 배틀 옵저버 커맨드 확인 용임
        [field:SerializeField] public AbstractSelector Attacker { get; private set; }
        [field:SerializeField] public AbstractSelector TargetSelector { get; private set; }
        
        private UniTaskCompletionSource _nextSignal;
        
        
        public ActionCommand(AbstractSelector attacker , AbstractSelector targetSelector)
        {
            Attacker = attacker;
            TargetSelector = targetSelector;
        }

        public void SetNextSignal(CancellationToken token)
        {
            _nextSignal = new UniTaskCompletionSource();
            _nextSignal.Task.ToCancellationToken(token);
        }

        public async UniTask ExecuteAction()
        {
            if (TryExecuteSkill()) return;
            
            await _nextSignal.Task;
        }

        private bool TryExecuteSkill()
        {
            if (Attacker.IsDead || TargetSelector.IsDead)
            {
                MoveNext();
                return true;
            }

            Attacker.SkillExecutor.TryExecuteSkill(Attacker, TargetSelector);
            return false;
        }

        public void MoveNext()
        {
            _nextSignal?.TrySetResult();
            _nextSignal = null;
        }
    }
}