using Members.KJY._01.Scripts.Title;
using Members.KJY._01.Scripts.UI;
using TMPro;
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
        [SerializeField] private GetSettingWindow settings;
        [SerializeField] private DiceCatalogPanel catalog;
        [SerializeField] private TMP_Text menuLabel;
        public bool CanNavigate => (settings == null || !settings.IsOpen) && (catalog == null || !catalog.IsOpen);
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !(keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)) return;
            if (settings != null && settings.IsOpen) settings.Close();
            else if (catalog != null && catalog.IsOpen) catalog.Close();
            else currentMenu?.Set();
        }

        public void Set(TitleMenu menu)
        {
            currentMenu = menu;
            if (menuLabel != null) menuLabel.text = menu == null ? string.Empty : menu.name switch
            {
                "Start" => "시작", "Set" => "설정", "Quit" => "나가기", "DiceDict" => "주사위 도감", _ => menu.name
            };
        }
    }
}
