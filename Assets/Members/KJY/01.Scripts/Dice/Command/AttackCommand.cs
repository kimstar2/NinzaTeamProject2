using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Command
{
    [Serializable]
    public class AttackCommand : ICommand
    {
        [field:SerializeField] public float Damage {get; private set;}
        [field:SerializeField] public AbstractSelector Attacker { get; private set; }
        [field:SerializeField] public AbstractSelector TargetSelector { get; private set; }
        [field:SerializeField] private float _testDelay = 1f;
        
        private CancellationTokenSource _cts;
        
        
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
            await UniTask.Delay(TimeSpan.FromSeconds(_testDelay), cancellationToken: token);
            TargetSelector.ApplyDamage(Damage);
        }
    }
}