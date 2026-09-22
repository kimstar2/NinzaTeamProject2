using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Members.LYW.Scripts.Event
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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