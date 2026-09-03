using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Members.LYW.Scripts.Event
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SetButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float moveDistance = 80f;
        [SerializeField] private float duration = 0.3f;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        private Vector2 originalPosition;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();

            originalPosition = rectTransform.anchoredPosition;
        }

        public void Set(int order)
        {
            float delay = order * 0.18f;

            canvasGroup.DOKill();
            rectTransform.DOKill();
            
            canvasGroup.alpha = 0f;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            rectTransform.anchoredPosition =
                originalPosition + Vector2.left * moveDistance;
            
            canvasGroup
                .DOFade(1f, duration)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);
            
            rectTransform
                .DOAnchorPos(originalPosition, duration)
                .SetDelay(delay)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }

        public void Hide()
        {
            canvasGroup.DOKill();
            rectTransform.DOKill();
            
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup
                .DOFade(0f, duration)
                .SetEase(Ease.InQuad);

            rectTransform
                .DOAnchorPos(
                    originalPosition + Vector2.left * moveDistance,
                    duration
                )
                .SetEase(Ease.InCubic);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            var outline = GetComponent<Outline>();
            var color = outline.effectColor;
            color.a = 1f;
            outline.effectColor = color;
            
            transform.DOKill();

            transform
                .DOScale(1.05f, 0.15f)
                .SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var outline = GetComponent<Outline>();
            var color = outline.effectColor;
            color.a = 0f;
            outline.effectColor = color;
            
            transform.DOKill();

            transform
                .DOScale(1f, 0.15f)
                .SetEase(Ease.OutQuad);
        }
    }
}