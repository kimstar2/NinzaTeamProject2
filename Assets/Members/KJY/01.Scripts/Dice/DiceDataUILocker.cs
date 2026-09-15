using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceDataUILocker : MonoBehaviour, IPointerEnterHandler , IPointerExitHandler
    {
        public UnityEvent onEnter;
        public UnityEvent onExit;
        public UnityEvent onLock;
        public UnityEvent onUnlock;
        private bool _isLock;
        
        public void LockToggle()
        {
            _isLock = !_isLock;
            if (_isLock) 
                onLock?.Invoke();
            else 
                onUnlock?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isLock) return;
            onEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isLock) return;
            onExit?.Invoke();
        }
    }
}