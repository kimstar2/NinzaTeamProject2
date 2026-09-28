using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using TMPro;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public class CombatStatusVisual : MonoBehaviour
    {
        public static TMP_FontAsset PopupFont { get; set; }

        private const float PopupInterval = 0.18f;
        private const float PopupDuration = 0.9f;
        private const float TintStrength = 0.5f;

        private AbstractSelector _selector;
        private readonly Queue<(string text, Color color)> _queue = new();
        private Coroutine _popupRoutine;
        private Color? _appliedTint;
        private Tween _reapplyTween;

        public void Bind(AbstractSelector selector)
        {
            _selector = selector;
            _selector.Effects.Popup += Enqueue;
            _selector.Effects.Changed += RefreshTint;
            _selector.Effects.Cleared += ForgetTint;
        }

        private void OnDestroy()
        {
            _reapplyTween?.Kill();
            if (_selector == null) return;
            _selector.Effects.Popup -= Enqueue;
            _selector.Effects.Changed -= RefreshTint;
            _selector.Effects.Cleared -= ForgetTint;
        }

        private SpriteRenderer AgentRenderer =>
            _selector != null && _selector.MyAgent != null && _selector.MyAgent.AgentRenderer != null
                ? _selector.MyAgent.AgentRenderer.SpriteRenderer : null;

        #region 문구

        private void Enqueue(string text, Color color)
        {
            _queue.Enqueue((text, color));
            if (_popupRoutine == null && isActiveAndEnabled) _popupRoutine = StartCoroutine(PopupRoutine());
        }

        private IEnumerator PopupRoutine()
        {
            while (_queue.Count > 0)
            {
                var (text, color) = _queue.Dequeue();
                Spawn(text, color);
                yield return new WaitForSeconds(PopupInterval);
            }
            _popupRoutine = null;
        }

        // 최상단 오버레이
        private static Canvas _popupCanvas;

        private static Canvas PopupCanvas
        {
            get
            {
                if (_popupCanvas != null) return _popupCanvas;
                var go = new GameObject("CombatPopupCanvas");
                _popupCanvas = go.AddComponent<Canvas>();
                _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _popupCanvas.sortingOrder = 999;
                return _popupCanvas;
            }
        }

        private void Spawn(string text, Color color)
        {
            var body = AgentRenderer;
            var cam = Camera.main;
            if (body == null || cam == null || !body.gameObject.activeInHierarchy) return;
            Bounds bounds = body.bounds;
            Vector3 screen = cam.WorldToScreenPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
            if (screen.z < 0f) return;
            float scale = Screen.height / 1080f;

            var go = new GameObject("CombatPopup", typeof(RectTransform));
            go.transform.SetParent(PopupCanvas.transform, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(400f, 60f) * scale;
            rect.position = screen + new Vector3(0f, 20f * scale, 0f);
            var label = go.AddComponent<TextMeshProUGUI>();
            if (PopupFont != null) label.font = PopupFont;
            label.raycastTarget = false;
            label.text = text;
            label.fontSize = 34f * scale;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.color = new Color(color.r, color.g, color.b, 1f);
            label.outlineWidth = 0.2f;
            label.outlineColor = new Color32(0, 0, 0, 200);

            go.transform.DOMoveY(rect.position.y + 70f * scale, PopupDuration).SetEase(Ease.OutCubic);
            DOTween.To(() => label.alpha, a => label.alpha = a, 0f, PopupDuration * 0.5f)
                .SetDelay(PopupDuration * 0.5f)
                .OnComplete(() => Destroy(go));
        }

        #endregion

        #region 디버프 색

        private Color BaseColor
        {
            get
            {
                var colorSO = _selector.AgentData switch
                {
                    PlayerDataSO player => player.ImageColor,
                    EnemyDataSO enemy => enemy.ImageColor,
                    _ => null
                };
                return colorSO != null ? colorSO.GetColor() : Color.white;
            }
        }

        private void RefreshTint()
        {
            if (_selector == null || _selector.AgentData == null) return;
            Color? tint = _selector.Effects.CurrentTint;
            if (tint == _appliedTint) return;
            _appliedTint = tint;
            ApplyTint();
            // 피격 깜빡임 후 재적용
            _reapplyTween?.Kill();
            _reapplyTween = DOVirtual.DelayedCall(0.25f, ApplyTint);
        }

        private void ApplyTint()
        {
            var body = AgentRenderer;
            if (body == null || _selector.AgentData == null) return;
            Color baseColor = BaseColor;
            Color color = _appliedTint.HasValue ? Color.Lerp(baseColor, _appliedTint.Value, TintStrength) : baseColor;
            color.a = baseColor.a;
            body.color = color;
        }

        private void ForgetTint()
        {
            _reapplyTween?.Kill();
            _appliedTint = null;
        }

        #endregion
    }
}
