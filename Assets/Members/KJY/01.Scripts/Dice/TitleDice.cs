using Members.KJY._01.Scripts.Title;
using Members.KJY._01.Scripts.UI;
using TMPro;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
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
        [SerializeField] private SoundClipSO clickSound;
        public bool CanNavigate => (settings == null || !settings.IsOpen) && (catalog == null || !catalog.IsOpen);
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !(keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)) return;
            if (settings != null && settings.IsOpen)
            {
                ServiceLocator.Get<IAudioService>().Play(clickSound);
                settings.Close();
            }
            else if (catalog != null && catalog.IsOpen)
            {
                ServiceLocator.Get<IAudioService>().Play(clickSound);
                catalog.Close();
            }
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
