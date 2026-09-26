using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    // 높이는 ContentSizeFitter가 결정하고, 이 컴포넌트는 화면 밖으로 나간 위치만 보정한다.
    [RequireComponent(typeof(RectTransform))]
    public class TooltipPanelBounds : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float screenPadding = 12f;
        [SerializeField] private Vector2 animationPadding = new(1.1f, 1.15f);
        private RectTransform _panel;
        private RectTransform _canvas;
        private Vector2 _originalPosition;

        private void Awake()
        {
            _panel = (RectTransform)transform;
            _canvas = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
            _originalPosition = _panel.anchoredPosition;
        }

        private void LateUpdate()
        {
            if (_panel.localScale.sqrMagnitude < 0.001f) return;
            _panel.anchoredPosition = _originalPosition;

            // 최대 확대 배율만큼 미리 비워 두면 튀어나오는 순간에도 잘리지 않고 위치가 흔들리지 않는다.
            Matrix4x4 toCanvas = _canvas.worldToLocalMatrix * _panel.parent.localToWorldMatrix *
                Matrix4x4.TRS(_panel.localPosition, _panel.localRotation,
                    new Vector3(Mathf.Max(1f, animationPadding.x), Mathf.Max(1f, animationPadding.y), 1f));
            Rect rect = _panel.rect;
            Vector2 min = new(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < 4; i++)
            {
                Vector2 point = toCanvas.MultiplyPoint3x4(new Vector3(
                    i < 2 ? rect.xMin : rect.xMax, i % 2 == 0 ? rect.yMin : rect.yMax));
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            Rect bounds = _canvas.rect;
            float x = Offset(min.x, max.x, bounds.xMin + screenPadding, bounds.xMax - screenPadding);
            float y = Offset(min.y, max.y, bounds.yMin + screenPadding, bounds.yMax - screenPadding);
            _panel.position += _canvas.TransformVector(new Vector3(x, y, 0f));
        }

        private static float Offset(float min, float max, float boundsMin, float boundsMax)
        {
            if (max - min > boundsMax - boundsMin) return boundsMax - max;
            return Mathf.Clamp(0f, boundsMin - min, boundsMax - max);
        }
    }
}
