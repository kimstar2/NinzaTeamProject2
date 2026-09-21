using System.Threading;
using _LumenLib.PoolingSystem.Runtime;
using _TevLib.Extension.DoT;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class FireballSkill : AbstractParticleSkill
    {
        [field:Header("Projectile Setting")]
        [field:SerializeField] public ParticleSystem ProjectileParticle {get; private set;}
        [field:SerializeField] public TransformTweenSequencer ProjectileSeq {get; private set;}
        [SerializeField] private Transform targetPosition;
        [field:SerializeField] public PoolItemSO ImpactParticle {get; private set;}
        [SerializeField, Min(0f)] private float damage = 24f;
        private bool _isArrived;
        private bool _isApplied;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            Vector3 startPos = Executor.Attacker.MyAgent.transform.position + effectOffset;
            targetPosition.position = Executor.Target.MyAgent.transform.position + effectOffset;
            ProjectileParticle.transform.position = startPos;
            ParticleSystem.MainModule main = ProjectileParticle.main;
            main.startRotation = -Mathf.Atan2(targetPosition.position.y - startPos.y,
                targetPosition.position.x - startPos.x); // 적이 쏘면 불덩이 방향도 반대로
            ProjectileParticle.Play();
            ProjectileSeq.SetTargetTrm(ProjectileParticle.transform);
            ProjectileSeq.Sequence();
            await UniTask.WaitUntil(() => !ProjectileSeq.HasTween, cancellationToken: token);
            ProjectileParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (!CanHit) return;
            _isArrived = true;
            PlayParticle(ImpactParticle, Executor.Target.MyAgent.transform.position + effectOffset);
            ApplyStat(); // 발사할 때 말고 도착했을 때 피해 적용
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || !_isArrived || _isApplied) return;
            _isApplied = true;
            Executor.Target.ApplyStat(ApplyStatType.Damage, GetDamage(damage));
        }
    }
}
