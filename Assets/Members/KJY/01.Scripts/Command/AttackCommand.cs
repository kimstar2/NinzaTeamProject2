using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Command
{
    [Serializable]
    public class AttackCommand : ICommand
    {
        // 직렬화 시켜놓는 이유는 인스펙터에서 배틀 옵저버 커맨드 확인 용임
        [field:SerializeField] public float Damage {get; private set;}
        [field:SerializeField] public AbstractSelector Attacker { get; private set; }
        [field:SerializeField] public AbstractSelector TargetSelector { get; private set; }
        
        private UniTaskCompletionSource _nextSignal;
        
        
        public AttackCommand(AbstractSelector attacker , AbstractSelector targetSelector)
        {
            Attacker = attacker;
            TargetSelector = targetSelector;
        }

        public void Execute() => Attacker.SkillExecutor.TryExecuteSkill(Attacker.DiceInventory.GetDiceData().SkillData);

        public async UniTask ExecuteAction(CancellationToken token)
        {
            _nextSignal = new UniTaskCompletionSource();
            _nextSignal.Task.ToCancellationToken(token);
            await _nextSignal.Task;
            TargetSelector.ApplyDamage(Damage);
        }

        public void MoveNext()
        {
            _nextSignal?.TrySetResult();
            _nextSignal = null;
        }
    }
}