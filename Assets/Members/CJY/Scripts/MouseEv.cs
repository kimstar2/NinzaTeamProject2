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
        [SerializeField] private float speed = 5f;
        private RectTransform rectTransform;
        private Button button;
        private Vector3 startScale;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            startScale = rectTransform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            rectTransform.DOKill();
            rectTransform.DOScale(startScale * maxScale, speed);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOKill();
            rectTransform.DOScale(startScale, speed);
        }
    }
}