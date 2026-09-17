using Members.LYW.Scripts.MySystem.Events;
using Members.LYW.Scripts.System;
using Members.LYW.Scripts.System.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Members.LYW.Scripts
{
    [RequireComponent(typeof(Image))]
    public class DiceFragment : MonoBehaviour, IPointerClickHandler
    {
        private Image _fragmentImage;
        public DiceFragmentSO _fragment { get; private set; }
        public bool isSelected { get; private set; } = false;
        public int index {get; private set;}
        public static int SelectedValue { get; private set; } = 0;
        public static void ResetSelectedValue() => SelectedValue = 0;
        public void Init(DiceFragmentSO fragment)
        {
            _fragmentImage = GetComponent<Image>();
            _fragment = fragment;
        }

        public void SetIndex(int num)
        {
            index = num;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isSelected)
            {
                if (SelectedValue >= 3) return;
                isSelected = true;
                SelectedValue++;
                _fragmentImage.color = new Color(0.7f, 0.7f, 0.7f, 1);
                EventBus.Publish(new RegisterFragmentEvent()
                {
                    fragment = this,
                });
            }
            else
            {
                isSelected = false;
                SelectedValue--;
                _fragmentImage.color = Color.white;
                EventBus.Publish(new UnRegisterFragmentEvent()
                {
                    fragment = this,
                });
            }
        }
    }
}