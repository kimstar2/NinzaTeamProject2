using System;
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

        public void SetMaterial(Material material)
        {
            Image.material = material;
        }
    }
}