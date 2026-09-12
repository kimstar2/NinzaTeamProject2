using System;
using _TevLib.Extension.DoT;
using DG.Tweening;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public class AgentHealthBar : MonoBehaviour
    {
        [SerializeField] private HealthModule healthModule;
        [SerializeField] private MonoSlider healthBar;
        [SerializeField] private TweenStep tweenStep;

        private void OnEnable() => healthModule.OnHealthChanged += HandleHealthChanged;

        private void OnDisable() => healthModule.OnHealthChanged -= HandleHealthChanged;

        private void HandleHealthChanged(float health, float maxHealth) => SetHealthBar(health/ maxHealth);
        public void SetHealthBar(float value)=> healthBar.Slider.DOValue(value, tweenStep.Duration).SetEase(tweenStep.EaseType);
    }
}