using System;
using System.Threading;
using _TevLib.Extension.DoT;
using _LumenLib.PoolingSystem.Runtime;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class ArrowShotSkill : AbstractParticleSkill
    {
        [field:Header("Arrow Setting")]
        [field:SerializeField] public SpriteRenderer ArrowRenderer {get; private set;}
        [field:SerializeField] public TransformTweenSequencer ProjectileSeq {get; private set;}
        [SerializeField] private Transform targetPosition;
        [SerializeField] private PoolItemSO releaseParticle;
        [SerializeField] private PoolItemSO impactParticle;
        [SerializeField] private TrailRenderer arrowTrail;
        [SerializeField, Min(1)] private int shotCount = 1;
        [SerializeField, Min(0f)] private float shotInterval = 0.12f;
        private bool _isArrived;
        private bool _isApplied;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            for (int i = 0; i < Mathf.Max(1, shotCount) && CanHit; i++)
            {
                _isArrived = false;
                _isApplied = false;
                await ShootArrow(token);
                if (i + 1 < shotCount && CanHit)
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, shotInterval)), cancellationToken: token);
            }
        }

        private async UniTask ShootArrow(CancellationToken token)
        {
            Vector3 startPos = Executor.Attacker.MyAgent.transform.position + effectOffset;
            targetPosition.position = Executor.Target.MyAgent.transform.position + effectOffset;
            Transform arrowTrm = ArrowRenderer.transform;
            arrowTrm.position = startPos;
            Vector3 direction = targetPosition.position - startPos;
            arrowTrm.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            ArrowRenderer.enabled = true; // 활 놓는 프레임에 화살 나감. 적이면 방향도 반대로
            PlayParticle(releaseParticle, startPos);
            if (arrowTrail != null)
            {
                arrowTrail.Clear();
                arrowTrail.emitting = true;
            }
            ProjectileSeq.SetTargetTrm(arrowTrm);
            ProjectileSeq.Sequence();
            await UniTask.WaitUntil(() => !ProjectileSeq.HasTween, cancellationToken: token);
            ArrowRenderer.enabled = false;
            if (arrowTrail != null) arrowTrail.emitting = false;
            if (!CanHit) return;
            PlayParticle(impactParticle, targetPosition.position);
            _isArrived = true;
            ApplyStat(); // 도착한 뒤에 피해 적용
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || !_isArrived || _isApplied) return;
            _isApplied = true;
            ApplyDamage();
        }
    }
}
