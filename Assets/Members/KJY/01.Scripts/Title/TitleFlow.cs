using System;
using Members.KJY._01.Scripts.UI;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.KJY._01.Scripts.Service;
using UnityEngine;

namespace Members.KJY._01.Scripts.Title
{
    public class TitleFlow : MonoBehaviour
    {
        [Header("SoundClip")]
        [SerializeField] private SoundClipSO titleBgm;
        
        [Header("Settings")]
        [SerializeField] private TitleDice titleDice;

        private void Start()
        {
            ServiceLocator.Get<IAudioService>().Play(titleBgm);
        }

        public void StartGame()
        {
            if (titleDice != null && !titleDice.CanNavigate) return;
            if (ServiceLocator.TryGet<IBattleDataStorage>(out var storage)) storage.Instance.ResetRun();
            if (ServiceLocator.TryGet<Inventory>(out var inventory) && inventory is BattleInventory battleInventory)
                battleInventory.ResetRun();
            
            SceneTransition.Load("Assets/Members/CJY/Scene/CJY.unity");
        }
    }
}
