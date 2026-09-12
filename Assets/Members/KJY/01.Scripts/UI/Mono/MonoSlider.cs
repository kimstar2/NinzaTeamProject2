using System;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI.Mono
{
    public class MonoSlider : MonoBehaviour
    {
        public Slider Slider {get; private set;}

        private void Awake()
        {
            Slider = GetComponent<Slider>();
        }

        public void SetFillAmount(float fillAmount)
        {
            Slider.value = fillAmount;
        }
        
        public void SetColorBlock(ColorBlock colorBlock) => Slider.colors = colorBlock;
    }
}