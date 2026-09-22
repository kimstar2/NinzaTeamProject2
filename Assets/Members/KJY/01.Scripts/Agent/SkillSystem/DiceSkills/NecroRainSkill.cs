using System;
using System.Threading;
using _LumenLib.PoolingSystem.Runtime;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class NecroRainSkill : AbstractParticleSkill
    {
        [field:Header("Particle Setting")]
        [field:SerializeField] public PoolItemSO CastParticle {get; private set;}
        [field:SerializeField] public PoolItemSO ImpactParticle {get; private set;}
        [SerializeField, Min(1)] private int hitCount = 3;
        [SerializeField, Min(0f)] private float damage = 8f;
        [SerializeField, Min(0f)] private float hitDelay = 0.3f;
        [SerializeField, Min(0f)] private float hitInterval = 0.15f;
        private int _hitCount;
        private int _applyCount;

        public void PlayCastParticle() // 대상 밑에 마법진 먼저 깔아줌
            => PlayParticle(CastParticle, Executor.Target.MyAgent.transform.position);

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            for (int i = 0; i < Mathf.Max(1, hitCount); i++)
            {
                if (!CanHit) break; // 중간에 죽었으면 남은 타격은 안 함
                PlayParticle(ImpactParticle, Executor.Target.MyAgent.transform.position + effectOffset);
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, hitDelay)), cancellationToken: token);
                if (!CanHit) break;
                _hitCount++;
                ApplyStat(); // 낙하 이펙트가 닿는 타이밍
                if (i + 1 < hitCount)
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, hitInterval)), cancellationToken: token);
            }
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _applyCount >= _hitCount) return;
            _applyCount++; // 지금 들어온 타격은 적용했음
            Executor.Target.ApplyStat(ApplyStatType.Damage, GetDamage(damage));
        }
    }
}
