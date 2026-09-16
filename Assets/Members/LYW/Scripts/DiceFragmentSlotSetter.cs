using System.Linq;
using Members.LYW.Scripts.MySystem.Events;
using Members.LYW.Scripts.System;
using Members.LYW.Scripts.System.Events;
using UnityEngine;

namespace Members.LYW.Scripts
{
    public class DiceFragmentSlotSetter : MonoBehaviour
    {
        [field : SerializeField] private DiceFragmentSlot[] slots =  new DiceFragmentSlot[3];

        public bool CanUpgrade
        {
            get => slots.Count(s => s.isSetted) > 1;

            private set
            {
                
            }
        }
        private void Awake()
        {
            EventBus.Subscribe<RegisterFragmentEvent>(AddFragment);
            EventBus.Subscribe<UnRegisterFragmentEvent>(RemoveFragment);
        }

        private void OnDisable()
        {
            EventBus.UnSubscribe<RegisterFragmentEvent>(AddFragment);
            EventBus.UnSubscribe<UnRegisterFragmentEvent>(RemoveFragment);
        }
        
        private void AddFragment(RegisterFragmentEvent registerFragmentEvent)
        {
            Debug.Log("추가됨.");
            if (slots.All(s => s.isSetted)) return;
            Debug.Log("추가됨2");
            foreach (var slot in slots)
            {
                if (!slot.isSetted)
                {
                    slot.SetFragment(registerFragmentEvent.fragment._fragment);
                    slot.SetIndex(registerFragmentEvent.fragment.index);
                    return;
                }
            }
        }
        
        private void RemoveFragment(UnRegisterFragmentEvent unRegisterFragmentEvent)
        {
            Debug.Log("제거됨.");
            if (!slots.Any(s => s.isSetted)) return;
            Debug.Log("제거됨2");
            foreach (var slot in slots)
            {
                if (slot.index ==  unRegisterFragmentEvent.fragment.index)
                {
                    slot.RemoveFragment();
                    return;
                }
            }
        }
    }
}