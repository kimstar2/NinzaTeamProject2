using System;
using System.Threading;
using _LumenLib.PoolingSystem.Runtime;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class HolyHealSkill : AbstractParticleSkill
    {
        [field:SerializeField] public PoolItemSO HealParticle {get; private set;}
        [SerializeField, Min(1)] private int pulseCount = 1;
        [SerializeField, Min(0f)] private float pulseInterval = 0.25f;
        [SerializeField] private bool emergencyHeal;
        private bool _isApplied;

        protected override bool CanHit => Executor != null && Executor.Attacker != null && !Executor.Attacker.IsDead;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            for (int i = 0; i < Mathf.Max(1, pulseCount) && CanHit; i++)
            {
                _isApplied = false;
                PlayParticle(HealParticle, Executor.Attacker.MyAgent.transform.position + effectOffset);
                ApplyStat();
                if (i + 1 < pulseCount && CanHit)
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, pulseInterval)), cancellationToken: token);
            }
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return;
            _isApplied = true;
            var health = Executor.Attacker.MyAgent.HealthModule;
            // 정수 비율로 비교해서 정확히 35%인 체력도 Mono의 소수 오차 없이 포함한다.
            bool isEmergency = health.CurrentHealth * 100f <= health.DefaultMaxHealth * 35f;
            float multiplier = emergencyHeal && isEmergency ? 2f : 1f;
            Executor.Attacker.ApplyStat(ApplyStatType.Heal, GetStat(ApplyStatType.Heal) * multiplier);
        }
    }
}
