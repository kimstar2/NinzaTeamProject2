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

        private void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
            _width = LineRenderer.widthMultiplier;
        }

        public void Connect(Transform from, Transform to, Gradient gradient, float fadeTime)
        {
            _fade?.Kill();
            _from = from;
            _to = to;
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
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                Vector3 pos = Vector3.Lerp(_from.position, _to.position, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
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
