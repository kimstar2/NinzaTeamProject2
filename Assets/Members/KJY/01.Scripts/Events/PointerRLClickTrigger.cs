using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Members.KJY._01.Scripts.Events
{
    public class PointerRlClickTrigger : MonoBehaviour , IPointerClickHandler
    {
        public UnityEvent onLeftClick;
        public UnityEvent onRightClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLeftClick?.Invoke();
                    break;
                case PointerEventData.InputButton.Right:
                    onRightClick?.Invoke();
                    break;
            }
        }
    }
}