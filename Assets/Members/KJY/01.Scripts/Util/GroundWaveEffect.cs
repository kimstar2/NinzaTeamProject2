using UnityEngine;

namespace Members.KJY._01.Scripts.Util
{
    // 바닥에 퍼지는 타원 파동. 프리팹 없이 LineRenderer로 만들고 끝나면 스스로 사라진다.
    public class GroundWaveEffect : MonoBehaviour
    {
        private const int Segments = 48;

        private LineRenderer _line;
        private Color _color;
        private float _radius;
        private float _duration;
        private float _width;
        private float _flatten;
        private float _delay;
        private float _time;

        // reference: 머티리얼과 정렬 순서를 빌려올 렌더러 (보통 시전자 스프라이트)
        public static GroundWaveEffect Spawn(Vector3 center, Color color, float radius, float duration,
            float width, Renderer reference, float delay = 0f, float flatten = 0.35f)
        {
            var go = new GameObject("GroundWave");
            go.transform.position = center;
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = Segments;
            line.numCornerVertices = 2;
            line.enabled = false;
            if (reference != null)
            {
                line.sharedMaterial = reference.sharedMaterial;
                line.sortingLayerID = reference.sortingLayerID;
                line.sortingOrder = reference.sortingOrder - 1; // 캐릭터 발밑에 깔리게
            }

            var wave = go.AddComponent<GroundWaveEffect>();
            wave._line = line;
            wave._color = color;
            wave._radius = Mathf.Max(0.01f, radius);
            wave._duration = Mathf.Max(0.05f, duration);
            wave._width = Mathf.Max(0.01f, width);
            wave._flatten = Mathf.Clamp(flatten, 0.05f, 1f);
            wave._delay = Mathf.Max(0f, delay);
            return wave;
        }

        private void Update()
        {
            if (_delay > 0f)
            {
                _delay -= Time.deltaTime;
                return;
            }

            _time += Time.deltaTime;
            float t = Mathf.Clamp01(_time / _duration);
            float eased = 1f - (1f - t) * (1f - t) * (1f - t); // 처음엔 빠르게, 끝에서 느려짐
            float radius = Mathf.Lerp(0.2f, _radius, eased);
            for (int i = 0; i < Segments; i++)
            {
                float angle = i * Mathf.PI * 2f / Segments;
                _line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * _flatten, 0f));
            }

            var color = _color;
            color.a *= 1f - t;
            _line.startColor = _line.endColor = color;
            _line.widthMultiplier = _width * Mathf.Lerp(1f, 0.3f, t);
            _line.enabled = true;

            if (t >= 1f) Destroy(gameObject);
        }
    }
}
