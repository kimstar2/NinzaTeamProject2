using DG.Tweening;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.HealthSystem
{
    // HealthModule.onTakeDamaged에서 호출. 공격 모션이나 위치를 바꾸지 않고 피격만 표현함.
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private ParticleSystem hitParticle;
        [SerializeField] private Color flashColor = new(1f, 0.35f, 0.25f, 1f);
        [SerializeField, Min(0.01f)] private float flashDuration = 0.12f;
        [SerializeField, Min(1f)] private float strongHitDamage = 40f;
        private Tween _flashTween;
        private Color _originalColor;

        public void Play(float damage)
        {
            if (damage <= 0f || float.IsNaN(damage) || float.IsInfinity(damage)) return;
            float strength = Mathf.Clamp01(damage / strongHitDamage);
            if (targetRenderer != null)
            {
                if (_flashTween == null || !_flashTween.IsActive()) _originalColor = targetRenderer.color;
                _flashTween?.Kill();
                targetRenderer.color = Color.Lerp(_originalColor, flashColor, 0.7f + strength * 0.3f);
                _flashTween = targetRenderer.DOColor(_originalColor, flashDuration).SetEase(Ease.OutQuad);
            }
            if (hitParticle != null)
            {
                if (targetRenderer != null) hitParticle.transform.position = targetRenderer.bounds.center;
                hitParticle.Play();
                hitParticle.Emit(Mathf.RoundToInt(Mathf.Lerp(5f, 12f, strength)));
            }
        }

        private void OnDisable()
        {
            if (_flashTween != null && _flashTween.IsActive())
            {
                _flashTween.Kill();
                if (targetRenderer != null) targetRenderer.color = _originalColor;
            }
            _flashTween = null;
            if (hitParticle != null) hitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
