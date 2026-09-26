using System;
using Members.KJY._01.Scripts.Title;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.Dice
{
    public enum Menu
    {
        Play,
        Options,
        Quit,
        DiceDictionary
    }
    public class TitleDice : MonoBehaviour
    {
        [SerializeField] private TitleMenu currentMenu;
        private void Update()
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                currentMenu.Set();
            }
        }

        public void Set(TitleMenu menu)
        {
            currentMenu = menu;
        }
    }
}