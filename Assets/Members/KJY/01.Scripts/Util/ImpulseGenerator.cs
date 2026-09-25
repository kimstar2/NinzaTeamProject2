using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Members.KJY._01.Scripts.Util
{
    [Serializable]
    public struct ImpulseSetting
    {
        [field: SerializeField] public Vector3 MinImpulseVel {get; private set;}
        [field: SerializeField] public Vector3 MaxImpulseVel {get; private set;}
    }
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class ImpulseGenerator : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float allGain = 1f;
        [SerializeField, Min(0.01f)] private float referenceForce = 24f;
        [SerializeField, Range(0f, 0.3f)] private float maxImpulse = 0.18f;
        [SerializeField] private List<ImpulseSetting> impulseSettings;
        public CinemachineImpulseSource Cis { get; private set; }

        private void Awake()
        {
            Cis = GetComponent<CinemachineImpulseSource>();
        }

        public void Generate()
        {
            GenerateWithForce(referenceForce);
        }

        // 피해가 커질수록 강해지지만 최대 진폭에 부드럽게 가까워짐.
        public float GetAmplitude(float force)
        {
            if (float.IsNaN(force) || float.IsInfinity(force) || force <= 0f) return 0f;
            float strength = 1f - 1f / (1f + force / Mathf.Max(0.01f, referenceForce));
            return Mathf.Clamp01(strength * Mathf.Max(0f, allGain)) * Mathf.Max(0f, maxImpulse);
        }

        public void GenerateWithForce(float force)
        {
            float amplitude = GetAmplitude(force);
            if (Cis == null || amplitude <= 0f) return;
            Cis.GenerateImpulseWithVelocity(GetDirection() * amplitude);
        }

        private Vector3 GetDirection()
        {
            if (impulseSettings == null || impulseSettings.Count == 0) return Vector3.right;
            int r = Random.Range(0, impulseSettings.Count);
            var setting = impulseSettings[r];
            float x = Random.Range(Mathf.Min(setting.MinImpulseVel.x, setting.MaxImpulseVel.x),
                Mathf.Max(setting.MinImpulseVel.x, setting.MaxImpulseVel.x));
            float y = Random.Range(Mathf.Min(setting.MinImpulseVel.y, setting.MaxImpulseVel.y),
                Mathf.Max(setting.MinImpulseVel.y, setting.MaxImpulseVel.y)) * 0.45f;
            Vector3 direction = new Vector3(x, y, 0f); // 2D 전투라 깊이 방향으로는 흔들지 않음
            return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.right;
        }
    }
}
