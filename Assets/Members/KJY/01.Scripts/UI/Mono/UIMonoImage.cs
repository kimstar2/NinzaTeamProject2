using System;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI.Mono
{
    public class UIMonoImage : MonoBehaviour
    {
        public Image Image {get; private set;}

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
        
        public void SetColor(ColorSO color) => Image.color = color.GetColor();

        public void SetMaterial(Material material)
        {
            Image.material = material;
        }
        
        public void SetActive(bool value) => Image.enabled = value;
    }
}