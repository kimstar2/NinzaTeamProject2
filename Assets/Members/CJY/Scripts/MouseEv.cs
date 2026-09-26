using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Members.CJY.Scripts
{
    public class MouseEv : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("effect")] 
        [SerializeField] private float maxScale = 1.15f;
        [SerializeField] private float duration = 0.2f;
        private RectTransform rectTransform;
        private Vector3 startScale;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            startScale = rectTransform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            rectTransform.DOKill();
            rectTransform.DOScale(startScale * maxScale, duration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOKill();
            rectTransform.DOScale(startScale, duration);
        }
    }
}