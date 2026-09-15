using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Members.KJY._01.Scripts.Events
{
    public class PointerEventTrigger : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler
    {
        public UnityEvent onEnter;
        public UnityEvent onExit;
        
        public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();

        public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    }
}