using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.Dice.Command
{
    [Serializable]
    public class AttackCommand : ICommand
    {
        [field:SerializeField] public float Damage {get; private set;}
        [field:SerializeField] public AbstractSelector Attacker { get; private set; }
        [field:SerializeField] public AbstractSelector TargetSelector { get; private set; }
        
        private UniTaskCompletionSource _nextSignal;
        
        
        public AttackCommand(AbstractSelector attacker , AbstractSelector targetSelector)
        {
            Attacker = attacker;
            Damage = 20; // 임시
            TargetSelector = targetSelector;
        }

        public void Execute()
        {
        }

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