using System;
using Members.KJY._01.Scripts.Util;
using Unity.VisualScripting;
using UnityEngine;

namespace Members.KJY._01.Scripts.Mono
{
    public class MonoLineRenderer : MonoBehaviour
    {
        public LineRenderer LineRenderer {get; private set;}

        private void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
        }

        public void SetGradient(Gradient gradient)
        {
            LineRenderer.colorGradient = gradient;
        }

        public void SetColor(Color color)
        {
            LineRenderer.startColor = color;
        }
        
        public void SetColor(ColorSO color)
        {
            SetColor(color.GetColor());
        }
    }
}