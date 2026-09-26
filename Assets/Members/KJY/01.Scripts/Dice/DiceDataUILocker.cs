using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Events.Dice;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceDataUILocker : MonoBehaviour, IPointerEnterHandler , IPointerExitHandler
    {
        [SerializeField] private EventChannelSO eventChannel;
        public UnityEvent onEnter;
        public UnityEvent onExit;
        public UnityEvent onLock;
        public UnityEvent onUnlock;
        private bool _isLock;
        private bool _isBattle;
        private bool _isOpen;

        private void OnEnable()
        {
            eventChannel.AddListener<OnStartBattle>(HandleStartBattle);
            eventChannel.AddListener<OnEndBattle>(HandleEndBattle);
        }

        private void HandleStartBattle(OnStartBattle evt)
        {
            _isBattle = true;
            _isLock = false;
            onUnlock?.Invoke();
            Close();
        }

        private void HandleEndBattle(OnEndBattle evt)
        {
            _isBattle = false;
        }
        
        public void LockToggle()
        {
            if (_isBattle) return;
            _isLock = !_isLock;
            if (_isLock) 
                onLock?.Invoke();
            else 
                onUnlock?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isLock || _isBattle) return;
            _isOpen = true;
            onEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isLock || _isBattle) return;
            Close();
        }

        private void Close()
        {
            // 닫힌 창이나 이미 닫히는 창에는 닫기 모션을 다시 재생하지 않는다.
            if (!_isOpen) return;
            _isOpen = false;
            onExit?.Invoke();
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnStartBattle>(HandleStartBattle);
            eventChannel.RemoveListener<OnEndBattle>(HandleEndBattle);
            _isBattle = false;
            _isLock = false;
            _isOpen = false;
            onUnlock?.Invoke();
        }
    }
}
