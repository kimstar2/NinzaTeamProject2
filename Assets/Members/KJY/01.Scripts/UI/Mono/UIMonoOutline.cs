using System;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI.Mono
{
    public class UIMonoOutline : MonoBehaviour
    {
        public Outline Outline {get; private set;}
        private void Awake()
        {
            Outline = GetComponent<Outline>();
        }

        public void SetColor(Color color)
        {
            if (Outline == null) return;
            Outline.effectColor = color;
        }   
        public void SetColor(ColorSO color)
        {
            if (Outline == null) return;
            Outline.effectColor = color.GetColor();
        }

        public void SetDistance(Vector2 distance)
        {
            Outline.effectDistance = distance;
        }
    }
}