using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Members.KJY.T.TestScripts
{
    public class DiceListUI : MonoBehaviour
    {
        [SerializeField] private GameObject diceUI;
        [SerializeField] private Image d1, d2, d3, d4, d5, d6;
        [SerializeField] private bool isUiLock;
        
        public void SetView(DiceData data)
        { // 현재는 여섯개라고 가정
            d1.sprite = data.diceSurface[0].diceSprite;
            d2.sprite = data.diceSurface[1].diceSprite;
            d3.sprite = data.diceSurface[2].diceSprite;
            d4.sprite = data.diceSurface[3].diceSprite;
            d5.sprite = data.diceSurface[4].diceSprite;
            d6.sprite = data.diceSurface[5].diceSprite;
        }

        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
                isUiLock = !isUiLock;
        }

        public void OnView()
        {
            diceUI.gameObject.SetActive(true);
        }   
        
        public void OffView()
        {
            if (isUiLock) return;
            diceUI.gameObject.SetActive(false);
        }
    }
}