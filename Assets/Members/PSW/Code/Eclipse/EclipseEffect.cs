using DG.Tweening;
using UnityEngine;

namespace Members.PSW.Code.Eclipse
{
    /// <summary>Local visual timeline only. All geometry and particle systems are authored in the prefab.</summary>
    [DisallowMultipleComponent]
    public sealed class EclipseEffect : MonoBehaviour
    {
        public const float Duration = 5.2f;
        public const float CrackTime = 2.55f;
        public const float ShatterTime = 3.35f;

        [Header("Playback")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool loopPreview = true;
        [SerializeField, Min(0.1f)] private float playbackSpeed = 1f;
        [SerializeField, Min(0f)] private float repeatDelay = 1f;
        [Header("Authored objects")]
        [SerializeField] private Transform sun;
        [SerializeField] private Transform moon;
        [SerializeField] private Renderer sunSurface;
        [SerializeField] private Renderer moonSurface;
        [SerializeField] private Renderer eclipseSurface;
        [SerializeField] private Renderer corona;
        [SerializeField] private Renderer atmosphere;
        [SerializeField] private Renderer shockRing;
        [SerializeField] private LineRenderer[] cracks;
        [SerializeField] private Renderer[] fragments;
        [SerializeField] private Vector3[] fragmentOrigins;
        [SerializeField] private ParticleSystem solarMotes;
        [SerializeField] private ParticleSystem lunarMotes;
        [SerializeField] private ParticleSystem fractureDust;
        [SerializeField] private ParticleSystem fallingGlints;

        private static readonly int Opacity = Shader.PropertyToID("_Opacity");
        private static readonly int Phase = Shader.PropertyToID("_Phase");
        private static readonly int Reveal = Shader.PropertyToID("_Reveal");
        private MaterialPropertyBlock _properties;
        private Sequence _sequence;
        private float _clock;
        private float _previousTime = -1f;

        private void OnEnable()
        {
            if (Application.isPlaying && playOnEnable) Play();
        }

        private void OnDisable() => Stop();
        private void OnDestroy() => _sequence?.Kill();

        [ContextMenu("Play Eclipse")]
        public void Play()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            Stop();
            _sequence = DOTween.Sequence();
            _sequence.AppendCallback(() =>
            {
                ClearParticles();
                _previousTime = -1f;
                _clock = 0f;
                Sample(0f);
            });
            _sequence.Append(DOTween.To(() => _clock, value =>
            {
                _clock = value;
                Sample(value);
            }, Duration, Duration).SetEase(Ease.Linear));
            _sequence.AppendInterval(repeatDelay);
            _sequence.SetLoops(loopPreview ? -1 : 1);
            _sequence.timeScale = Mathf.Max(0.1f, playbackSpeed);
            _sequence.SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        [ContextMenu("Stop Eclipse")]
        public void Stop()
        {
            _sequence?.Kill();
            _sequence = null;
            ClearParticles();
            Sample(0f);
            _previousTime = -1f;
        }

        /// <summary>Deterministic sampling also supports the editor scrubber and render validation.</summary>
        public void Sample(float seconds, bool simulateParticles = false)
        {
            if (!sun || !moon || !eclipseSurface) return;
            float t = Mathf.Clamp(seconds, 0f, Duration);
            float appear = Smooth(0.12f, 0.8f, t);
            float merge = Smooth(1.05f, 2.15f, t);
            float merged = Smooth(2.02f, 2.22f, t);
            float after = Mathf.Max(0f, t - ShatterTime);
            bool broken = t >= ShatterTime;
            Vector3 center = new Vector3(0f, 0.35f, 0f);

            // Opposed arcs end at precisely the same center; no slash or chain phase.
            sun.localPosition = center + new Vector3(-2.15f * (1f - merge),
                0.48f * (1f - merge) + Mathf.Sin(merge * Mathf.PI) * 0.65f, 0.15f);
            moon.localPosition = center + new Vector3(2.15f * (1f - merge),
                -0.48f * (1f - merge) - Mathf.Sin(merge * Mathf.PI) * 0.65f, -0.1f);
            sun.localScale = Vector3.one * Mathf.Lerp(0.04f, 1f, appear);
            moon.localScale = Vector3.one * Mathf.Lerp(0.04f, 1f, appear);
            SetSurface(sunSurface, appear * (1f - merged), t);
            SetSurface(moonSurface, appear * (1f - merged), t);
            SetSurface(eclipseSurface, broken ? 0f : merged, t);
            float pressure = Smooth(CrackTime, ShatterTime, t);
            SetSurface(corona, merged * (1f - Smooth(ShatterTime, 4.25f, t)), t);
            corona.transform.localScale = Vector3.one * (1f + pressure * 0.07f + after * 0.28f);
            SetSurface(atmosphere, Smooth(0f, 0.6f, t) * (1f - Smooth(4.25f, Duration, t)), t);
            SetSurface(shockRing, broken ? (1f - Smooth(0f, 0.7f, after)) * 0.8f : 0f, t);
            shockRing.transform.localScale = Vector3.one * (1f + after * 2.6f);

            for (int i = 0; i < cracks.Length; i++)
            {
                float delay = (i % 9) * 0.026f;
                float reveal = Smooth(CrackTime + delay, ShatterTime - 0.06f, t);
                SetSurface(cracks[i], broken ? 0f : reveal, t, reveal);
                cracks[i].widthMultiplier = 0.009f + pressure * 0.022f;
            }
            for (int i = 0; i < fragments.Length; i++)
            {
                Transform piece = fragments[i].transform;
                Vector3 origin = fragmentOrigins[i];
                Vector3 outward = new Vector3(origin.x, origin.y, 0f).normalized;
                float stagger = (i % 7) * 0.018f;
                float age = Mathf.Max(0f, after - stagger);
                float speed = 0.9f + (i % 5) * 0.28f;
                piece.localPosition = origin + outward * (age * speed + age * age * 0.25f)
                    + new Vector3(0f, -age * age * 0.85f, -(i % 3) * age * 0.18f);
                piece.localRotation = Quaternion.Euler(age * ((i % 3) - 1) * 65f,
                    age * ((i % 5) - 2) * 35f, age * ((i % 2 == 0) ? 34f : -28f));
                piece.localScale = Vector3.one * (1f - Smooth(0.8f, 1.85f, age) * 0.6f);
                SetSurface(fragments[i], broken ? 1f - Smooth(0.65f, 1.75f, age) : 0f, t);
            }

            if (simulateParticles)
            {
                SimulateMotes(solarMotes, t);
                SimulateMotes(lunarMotes, t);
                SimulateBurst(fractureDust, t - ShatterTime);
                SimulateBurst(fallingGlints, t - ShatterTime);
            }
            else if (Application.isPlaying)
            {
                if (_previousTime < 0.18f && t >= 0.18f)
                {
                    solarMotes.Play();
                    lunarMotes.Play();
                }
                if (_previousTime < 2.2f && t >= 2.2f)
                {
                    solarMotes.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    lunarMotes.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                if (_previousTime < ShatterTime && broken)
                {
                    fractureDust.Play();
                    fallingGlints.Play();
                }
            }
            _previousTime = t;
        }

        private void SetSurface(Renderer surface, float alpha, float time, float reveal = 1f)
        {
            if (!surface) return;
            surface.enabled = alpha > 0.001f;
            if (!surface.enabled) return;
            if (_properties == null) _properties = new MaterialPropertyBlock();
            _properties.Clear();
            _properties.SetFloat(Opacity, alpha);
            _properties.SetFloat(Phase, time);
            _properties.SetFloat(Reveal, reveal);
            surface.SetPropertyBlock(_properties);
        }

        private static float Smooth(float from, float to, float time) =>
            Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, time));

        private void ClearParticles()
        {
            if (solarMotes) solarMotes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (lunarMotes) lunarMotes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (fractureDust) fractureDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (fallingGlints) fallingGlints.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static void SimulateMotes(ParticleSystem particles, float time)
        {
            particles.Simulate(Mathf.Max(0f, Mathf.Min(time, 2.2f) - 0.18f), true, true);
            if (time > 2.2f)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                particles.Simulate(time - 2.2f, true, false);
            }
        }

        private static void SimulateBurst(ParticleSystem particles, float age)
        {
            if (age < 0f) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else particles.Simulate(age, true, true);
        }
    }
}
