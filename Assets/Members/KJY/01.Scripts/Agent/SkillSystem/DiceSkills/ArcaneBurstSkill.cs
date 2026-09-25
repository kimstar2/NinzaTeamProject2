using System.Threading;
using _LumenLib.PoolingSystem.Runtime;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;
using UnityEngine.Serialization;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class ArcaneBurstSkill : AbstractParticleSkill
    {
        [field:Header("Particle Setting")]
        [field:SerializeField, FormerlySerializedAs("castParticle")] public PoolItemSO CastParticle {get; private set;}
        [field:SerializeField, FormerlySerializedAs("impactParticle")] public PoolItemSO ImpactParticle {get; private set;}
        private bool _isApplied;

        public void PlayCastParticle() // onCast에 연결
            => PlayParticle(CastParticle, Executor.Attacker.MyAgent.transform.position + effectOffset);

        protected override UniTask AttackAsync(CancellationToken token)
        {
            PlayParticle(ImpactParticle, Executor.Target.MyAgent.transform.position + effectOffset);
            ApplyStat(); // 얘는 폭발 뜨는 순간 바로 때림
            return UniTask.CompletedTask;
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return; // 두 번 들어와도 한 번만 적용
            _isApplied = true;
            Executor.Target.ApplyStat(ApplyStatType.Damage, GetDamage());
        }
    }
}
