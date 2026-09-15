using System;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.Mono
{
    public class MonoSprite : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer { get; private set; }

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetSprite(Sprite sprite) => SpriteRenderer.sprite = sprite;
        public void SetSprite(Image image) => SpriteRenderer.sprite = image.sprite;
        public void SetColor(Color color) => SpriteRenderer.color = color;
        public void SetColor(ColorSO color) => SpriteRenderer.color = color.GetColor();
    }
}