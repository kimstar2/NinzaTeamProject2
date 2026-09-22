using DG.Tweening;
using Members.KJY._01.Scripts.Agent.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI.Mono
{
    // 선택 기다릴 땐 숨 쉬듯이, 연결되면 은은하게 켜둠. 게임 판정은 셀렉터가 함
    public class UISelectionEffect : MonoBehaviour
    {
        [SerializeField] private PlayerSelector selector;
        [SerializeField] private Outline targetOutline;
        [SerializeField] private Image lightImage;
        [SerializeField] private Color idleColor = new(0.22f, 0.28f, 0.36f, 0.7f);
        [SerializeField, Min(0.01f)] private float fadeTime = 0.14f;
        [SerializeField, Min(0.1f)] private float pulseTime = 0.65f;
        [SerializeField, Range(0f, 1f)] private float lightAlpha = 0.28f;
        [SerializeField, Range(0f, 1f)] private float connectedStrength = 0.55f;
        [SerializeField] private Vector2 outlineWidth = new(1f, 3f);
        private Tween _fade;
        private Tween _pulse;
        private Color _color;
        private float _strength;
        private Vector3 _lightScale;

        private void Awake() => _lightScale = lightImage.rectTransform.localScale;
        private void Start() => SetStrength(0f);

        public void ShowSelected() // onSelect 연결
        {
            KillTween();
            _color = selector.PlayerColor.GetColor();
            _fade = DOTween.To(() => _strength, SetStrength, 1f, fadeTime)
                .SetUpdate(true).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    _pulse = DOTween.To(() => _strength, SetStrength, 0.65f, pulseTime)
                        .SetUpdate(true).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                });
        }

        public void ShowConnected() // onSetTarget 연결. 한 번 밝아졌다가 연결 표시만 남김
        {
            KillTween();
            _color = selector.PlayerColor.GetColor();
            SetStrength(1f);
            _fade = DOTween.To(() => _strength, SetStrength, connectedStrength, fadeTime * 2f)
                .SetUpdate(true).SetEase(Ease.OutCubic);
        }

        public void Hide() // 선택 취소랑 배틀 시작 때 꺼줌
        {
            KillTween();
            _fade = DOTween.To(() => _strength, SetStrength, 0f, fadeTime)
                .SetUpdate(true).SetEase(Ease.OutCubic);
        }

        private void SetStrength(float strength)
        {
            _strength = strength;
            targetOutline.effectColor = Color.Lerp(idleColor, _color, strength);
            float width = Mathf.Lerp(outlineWidth.x, outlineWidth.y, strength);
            targetOutline.effectDistance = new Vector2(width, -width);
            Color lightColor = _color;
            lightColor.a = lightAlpha * strength;
            lightImage.color = lightColor;
            lightImage.rectTransform.localScale = _lightScale * (1f + strength * 0.04f);
        }

        private void KillTween()
        {
            _fade?.Kill();
            _pulse?.Kill();
            _fade = null;
            _pulse = null;
        }

        private void OnDisable()
        {
            KillTween();
            SetStrength(0f);
        }
    }
}
