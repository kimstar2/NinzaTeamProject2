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

        protected override bool CanHit => Executor != null && Executor.SkillData.CanTarget(Executor.Attacker, Executor.Target);

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            for (int i = 0; i < Mathf.Max(1, pulseCount) && CanHit; i++)
            {
                _isApplied = false;
                PlayParticle(HealParticle, Executor.Target.MyAgent.transform.position + effectOffset);
                ApplyStat();
                if (i + 1 < pulseCount && CanHit)
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, pulseInterval)), cancellationToken: token);
            }
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return;
            _isApplied = true;
            PlaySkillSound();
            foreach (var target in Executor.GetTargets())
            {
                if (target == null || target.IsDead) continue;
                if (target != Executor.Target)
                    PlayParticle(HealParticle, target.MyAgent.transform.position + effectOffset);
                var health = target.MyAgent.HealthModule;
                // 35% 경계 오차 방지
                bool isEmergency = health.CurrentHealth * 100f <= health.DefaultMaxHealth * 35f;
                float multiplier = emergencyHeal && isEmergency ? 2f : 1f;
                float heal = GetStat(ApplyStatType.Heal) * multiplier +
                             health.DefaultMaxHealth * Executor.SkillData.GetHealRatio(BaseLevel) * Executor.PowerMultiplier;
                if (heal > 0f) target.ApplyStat(ApplyStatType.Heal, heal);
                Executor.ApplySynergy(target);
            }
        }
    }
}
