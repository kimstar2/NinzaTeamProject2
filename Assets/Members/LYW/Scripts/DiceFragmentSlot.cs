using System;
using JetBrains.Annotations;
using Members.KJY._01.Scripts.Dice.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Members.LYW.Scripts
{
    [RequireComponent(typeof(Image))]
    public class DiceFragmentSlot : MonoBehaviour
    {
        private Image diceFragmentSprite;
        [field : SerializeField] public Sprite defaultImage { get; private set; }
        public DiceDataSO _fragment {get; private set;}
        public bool isSetted { get; private set; } = false;
        public int index { get; private set; }

        public void SetFragment(DiceDataSO fragment)
        {
            if (isSetted) return;
            isSetted = true;
            _fragment = fragment;
            diceFragmentSprite.sprite = fragment.Icon;
        }

        public void ResetSlot()
        {
            index = 0;
            isSetted = false;
            _fragment = null;
            diceFragmentSprite.sprite = defaultImage;
        }
        
        public void RemoveFragment()
        {
            if (!isSetted) return;
            index = 0;
            isSetted = false;
            _fragment = null;
            diceFragmentSprite.sprite = defaultImage;
        }

        public void SetIndex(int num)
        {
            index = num;
        }
        
        private void Awake()
        {
            diceFragmentSprite = GetComponent<Image>();
            diceFragmentSprite.sprite = defaultImage;
        }
    }
}