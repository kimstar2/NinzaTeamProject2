using System;
using System.Threading;
using _LumenLib.PoolingSystem.Runtime;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class HolyStrikeSkill : AbstractParticleSkill
    {
        [field:Header("Particle Setting")]
        [field:SerializeField] public PoolItemSO CastParticle {get; private set;}
        [field:SerializeField] public PoolItemSO ImpactParticle {get; private set;}
        [SerializeField] private PoolItemSO healParticle;
        [SerializeField, Min(0f)] private float hitDelay = 0.24f;
        private bool _isHit;
        private bool _isApplied;

        public void PlayCastParticle()
            => PlayParticle(CastParticle, Executor.Attacker.MyAgent.transform.position + effectOffset);

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            PlayParticle(ImpactParticle, Executor.Target.MyAgent.transform.position + effectOffset);
            await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, hitDelay)), cancellationToken: token);
            _isHit = true;
            ApplyStat(); // 빛 기둥 내려온 다음에 한 방 세게 때림
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || !_isHit || _isApplied) return;
            _isApplied = true;
            Executor.Target.ApplyStat(ApplyStatType.Damage, GetDamage());
            float heal = GetStat(ApplyStatType.Heal);
            if (heal > 0f && !Executor.Attacker.IsDead)
            {
                PlayParticle(healParticle, Executor.Attacker.MyAgent.transform.position + effectOffset);
                Executor.Attacker.ApplyStat(ApplyStatType.Heal, heal);
            }
        }
    }
}
