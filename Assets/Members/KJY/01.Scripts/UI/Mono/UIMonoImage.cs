using System;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI.Mono
{
    public class UIMonoImage : MonoBehaviour
    {
        protected Image Image;

        protected virtual void Awake()
        {
            Image = GetComponent<Image>();
        }

        public void SetImage(Sprite sprite)
        {
            Image.sprite = sprite;
        }
        
        public void SetColor(Color color)
        {
            Image.color = color;
        }
    }
}