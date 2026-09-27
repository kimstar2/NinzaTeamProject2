using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using _LumenLib.PoolingSystem.Runtime;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    // 대지의 분노: 뛰어올랐다 내려찍으면 화면이 지진처럼 흔들리며 적 전체에 피해를 준다.
    public class EarthFurySkill : AbstractParticleSkill
    {
        [SerializeField] private PoolItemSO impactParticle;
        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.6f;
        [SerializeField, Min(0f)] private float jumpForward = 0.8f;
        [SerializeField, Min(0.05f)] private float jumpDuration = 0.42f;
        [Header("Earthquake")]
        [SerializeField, Min(0f)] private float shakeForce = 120f; // 첫 흔들림 세기, 이후 점점 약해짐
        [SerializeField, Min(1)] private int shakeCount = 6;
        [SerializeField, Min(0.01f)] private float shakeInterval = 0.07f;
        [SerializeField, Range(0f, 1f)] private float shakeDecay = 0.75f;
        private bool _isApplied;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            var attackerTrm = Executor.Attacker.MyAgent.transform;
            float direction = Mathf.Sign(Executor.Target.MyAgent.transform.position.x - attackerTrm.position.x);
            Vector3 landing = attackerTrm.position + Vector3.right * (direction * jumpForward);

            // 뛰어올랐다가 내리꽂기
            Tween jump = attackerTrm.DOJump(landing, jumpHeight, 1, jumpDuration).SetEase(Ease.InQuad);
            try
            {
                await UniTask.WaitWhile(() => jump.IsActive() && jump.IsPlaying(), cancellationToken: token);
            }
            finally
            {
                jump.Kill();
            }
            if (!CanHit) return;

            // 착지하는 순간 피해, 그 뒤로 지진처럼 여러 번 흔들리며 잦아든다
            ApplyStat();
            var impulse = Executor.Attacker.MyAgent.GetComponentInChildren<ImpulseGenerator>();
            if (impulse == null) return;
            float force = shakeForce;
            for (int i = 0; i < shakeCount; i++)
            {
                impulse.GenerateWithForce(force);
                force *= shakeDecay;
                await UniTask.Delay(TimeSpan.FromSeconds(shakeInterval), cancellationToken: token);
            }
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return;
            _isApplied = true;
            foreach (var target in Executor.GetTargets())
                if (target != null && !target.IsDead)
                    PlayParticle(impactParticle, target.MyAgent.transform.position + effectOffset);
            ApplyDamage();
        }
    }
}
