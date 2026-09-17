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
            EventBus.Subscribe<UpgradeFragmentEvent>(RefreshFragments);
        }

        private void OnDisable()
        {
            EventBus.UnSubscribe<RegisterFragmentEvent>(AddFragment);
            EventBus.UnSubscribe<UnRegisterFragmentEvent>(RemoveFragment);
            EventBus.UnSubscribe<UpgradeFragmentEvent>(RefreshFragments);
        }

        private void RefreshFragments(UpgradeFragmentEvent upgradeFragmentEvent)
        {
            foreach (var slot in slots)
            {
                slot.ResetSlot();
            }
        }
        
        private void AddFragment(RegisterFragmentEvent registerFragmentEvent)
        {
            if (slots.All(s => s.isSetted)) return;
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
            if (!slots.Any(s => s.isSetted)) return;
            foreach (var slot in slots)
            {
                if (slot.index == unRegisterFragmentEvent.fragment.index)
                {
                    slot.RemoveFragment();
                    return;
                }
            }
        }
    }
}