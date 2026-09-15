using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Members.KJY._01.Scripts.Events
{
    public class PointerClickTrigger : MonoBehaviour , IPointerClickHandler
    {
        public UnityEvent onClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }
    }
}