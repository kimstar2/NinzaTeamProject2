using System.Threading;
using _TevLib.Extension.DoT;
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
        private bool _isArrived;
        private bool _isApplied;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            Vector3 startPos = Executor.Attacker.MyAgent.transform.position + effectOffset;
            targetPosition.position = Executor.Target.MyAgent.transform.position + effectOffset;
            Transform arrowTrm = ArrowRenderer.transform;
            arrowTrm.position = startPos;
            Vector3 direction = targetPosition.position - startPos;
            arrowTrm.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            ArrowRenderer.enabled = true; // 활 놓는 프레임에 화살 나감. 적이면 방향도 반대로
            ProjectileSeq.SetTargetTrm(arrowTrm);
            ProjectileSeq.Sequence();
            await UniTask.WaitUntil(() => !ProjectileSeq.HasTween, cancellationToken: token);
            ArrowRenderer.enabled = false;
            _isArrived = true;
            ApplyStat(); // 도착한 뒤에 피해 적용
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || !_isArrived || _isApplied) return;
            _isApplied = true;
            Executor.Target.ApplyStat(ApplyStatType.Damage, GetDamage());
        }
    }
}
