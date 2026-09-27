using DG.Tweening;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Mono
{
    public class MonoLineRenderer : MonoBehaviour
    {
        [SerializeField, Range(2, 64)] private int pointCount = 24;
        [SerializeField] private float arcHeight = 0.2f;
        public LineRenderer LineRenderer {get; private set;}
        private Transform _from, _to;
        private Tween _fade;
        private float _width;
        private bool _sameTeam;

        private void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
            _width = LineRenderer.widthMultiplier;
        }

        public void Connect(Transform from, Transform to, Gradient gradient, float fadeTime, bool sameTeam = false)
        {
            _fade?.Kill();
            _from = from;
            _to = to;
            _sameTeam = sameTeam;
            SetGradient(gradient); // 원본 말고 복제된 선에만 색 넣음
            LineRenderer.useWorldSpace = true;
            LineRenderer.positionCount = Mathf.Max(2, pointCount);
            LineRenderer.enabled = true;
            UpdatePositions();
            LineRenderer.widthMultiplier = 0f;
            _fade = DOTween.To(() => LineRenderer.widthMultiplier,
                    x => LineRenderer.widthMultiplier = x, _width, fadeTime)
                .SetEase(Ease.OutCubic).SetUpdate(true);
        }

        private void LateUpdate()
        {
            if (_from != null && _to != null) UpdatePositions(); // UI 움직여도 연결점 따라감
        }

        private void UpdatePositions()
        {
            int count = LineRenderer.positionCount;
            Vector3 start = _from.position, end = _to.position;
            float reach = Mathf.Max(_width * 1.5f, arcHeight * 2f);
            float side = Mathf.Max(start.x, end.x) + reach;
            bool self = _from == _to;
            if (self) end += Vector3.up * Mathf.Max(_width * 1.2f, .22f);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                Vector3 pos;
                if (_sameTeam)
                {
                    Vector3 cornerA = new(side, start.y, start.z), cornerB = new(side, end.y, end.z);
                    pos = t < 1f / 3f ? Vector3.Lerp(start, cornerA, t * 3f) :
                        t < 2f / 3f ? Vector3.Lerp(cornerA, cornerB, t * 3f - 1f) :
                        Vector3.Lerp(cornerB, end, t * 3f - 2f);
                }
                else
                {
                    pos = Vector3.Lerp(start, end, t);
                    pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
                }
                LineRenderer.SetPosition(i, pos); // 중간 점이 있어야 중간 그라데이션도 보임
            }
        }

        public void Disconnect(float fadeTime)
        {
            _fade?.Kill();
            _fade = DOTween.To(() => LineRenderer.widthMultiplier,
                    x => LineRenderer.widthMultiplier = x, 0f, fadeTime)
                .SetEase(Ease.InCubic).SetUpdate(true).OnComplete(() => Destroy(gameObject));
        }

        public void SetGradient(Gradient gradient) => LineRenderer.colorGradient = gradient;
        public void SetColor(Color color)
        {
            LineRenderer.startColor = color;
            LineRenderer.endColor = color;
        }
        public void SetColor(ColorSO color) => SetColor(color.GetColor());
        private void OnDestroy() => _fade?.Kill();
    }
}
