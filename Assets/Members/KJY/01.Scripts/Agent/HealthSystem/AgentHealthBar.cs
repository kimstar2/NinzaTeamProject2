using System;
using DG.Tweening;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.HealthSystem
{
    public class AgentHealthBar : AbstractHealthBar
    {
        [SerializeField] private HealthModule healthModule;
        [SerializeField] private Transform healthBar;

        private void OnEnable()
        {
            healthModule.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            healthModule.OnHealthChanged -= HandleHealthChanged;
        }

        public override void SetHealthBar(float value)
        {
            healthBar.DOScaleX(value, tweenStep.Duration).SetEase(tweenStep.EaseType);
        }
    }
}