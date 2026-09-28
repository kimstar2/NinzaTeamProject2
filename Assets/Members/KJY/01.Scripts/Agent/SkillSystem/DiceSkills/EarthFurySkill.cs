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
    public class EarthFurySkill : AbstractParticleSkill
    {
        [SerializeField] private PoolItemSO impactParticle;
        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.6f;
        [SerializeField, Min(0f)] private float jumpForward = 0.8f;
        [SerializeField, Min(0.05f)] private float jumpDuration = 0.42f;
        [Header("Earthquake")]
        [SerializeField, Min(0f)] private float slamAmplitude = 0.9f;
        [SerializeField, Min(0f)] private float rumbleAmplitude = 0.45f;
        [SerializeField, Min(1)] private int rumbleCount = 10;
        [SerializeField, Min(0.01f)] private float rumbleInterval = 0.06f;
        [SerializeField, Range(0f, 1f)] private float rumbleDecay = 0.8f;
        private bool _isApplied;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            var attackerTrm = Executor.Attacker.MyAgent.transform;
            float direction = Mathf.Sign(Executor.Target.MyAgent.transform.position.x - attackerTrm.position.x);
            Vector3 landing = attackerTrm.position + Vector3.right * (direction * jumpForward);

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

            ApplyStat();
            // ImpulseGenerator 상한(0.18) 우회
            var impulse = Executor.Attacker.MyAgent.GetComponentInChildren<ImpulseGenerator>();
            var source = impulse != null ? impulse.Cis : null;
            if (source == null) return;
            source.GenerateImpulseWithVelocity(Vector3.down * slamAmplitude);
            float amplitude = rumbleAmplitude;
            for (int i = 0; i < rumbleCount; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(rumbleInterval), cancellationToken: token);
                var dir = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f).normalized;
                source.GenerateImpulseWithVelocity(dir * amplitude);
                amplitude *= rumbleDecay;
            }
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return;
            _isApplied = true;
            PlaySkillSound();
            foreach (var target in Executor.GetTargets())
                if (target != null && !target.IsDead)
                    PlayParticle(impactParticle, target.MyAgent.transform.position + effectOffset);
            ApplyDamage();
        }
    }
}
