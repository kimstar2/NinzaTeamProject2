using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class HealingWaveSkill : HolyHealSkill
    {
        [Header("Wave")]
        [SerializeField] private Color waveColor = new(0.45f, 1f, 0.55f, 0.9f);
        [SerializeField, Min(0.1f)] private float waveRadius = 6f;
        [SerializeField, Min(0.05f)] private float waveDuration = 0.7f;
        [SerializeField, Min(0.01f)] private float waveWidth = 0.18f;
        [SerializeField, Min(1)] private int waveCount = 3;
        [SerializeField, Min(0f)] private float waveGap = 0.15f;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            var caster = Executor.Attacker.MyAgent;
            var reference = caster.GetComponentInChildren<SpriteRenderer>();
            for (int i = 0; i < waveCount; i++)
                GroundWaveEffect.Spawn(caster.transform.position, waveColor, waveRadius, waveDuration, waveWidth,
                    reference, i * waveGap);

            await UniTask.Delay(TimeSpan.FromSeconds(waveDuration * 0.4f), cancellationToken: token);
            await base.AttackAsync(token);
        }
    }
}
