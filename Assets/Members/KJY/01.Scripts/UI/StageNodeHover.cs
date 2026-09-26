using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Members.KJY._01.Scripts.UI
{
    public class StageNodeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("effect")] 
        [SerializeField] private float maxScale = 1.15f;
        [SerializeField] private float speed = 5f;
        private RectTransform rectTransform;
        private Vector3 startScale;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            startScale = rectTransform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (rectTransform == null || !isActiveAndEnabled) return;
            rectTransform.DOKill();
            rectTransform.DOScale(startScale * maxScale, speed);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (rectTransform == null || !isActiveAndEnabled) return;
            rectTransform.DOKill();
            rectTransform.DOScale(startScale, speed);
        }

        private void OnDisable()
        {
            if (rectTransform == null) return;
            // 노드가 풀로 돌아가거나 씬이 바뀌면 진행 중인 확대도 끝낸다.
            rectTransform.DOKill();
            rectTransform.localScale = startScale;
        }
    }
}
