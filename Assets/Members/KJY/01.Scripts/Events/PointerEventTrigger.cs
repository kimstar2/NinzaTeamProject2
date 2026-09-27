using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.Events
{
    public class PointerEventTrigger : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler
    {
        public UnityEvent onEnter;
        public UnityEvent onExit;
        private Selectable _selectable;

        private void Awake() => TryGetComponent(out _selectable);
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable == null || _selectable.IsInteractable()) onEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    }
}
