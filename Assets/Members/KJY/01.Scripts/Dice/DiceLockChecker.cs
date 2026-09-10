using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Services;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceLockChecker : MonoModule
    {
        [SerializeField] private PlayerType playerType;
        [SerializeField] private EventChannelSO eventChannel;

        [Header("Lock Setting")] 
        [SerializeField] private UIMonoImage diceImage;
        [SerializeField] private Color onLockColor;
        [SerializeField] private Color offLockColor;
        public bool IsLocked {get; private set;}
        
        public void LockToggle()
        {
            if (!IsLocked)
                OnLock();
            else
                OffLock();
            eventChannel.RaiseEvent(new OnDiceLock(IsLocked,playerType));
        }

        private void OnLock()
        {
            IsLocked = true;
            diceImage.SetColor(onLockColor);
        }

        
        private void OffLock()
        {
            IsLocked = false;
            diceImage.SetColor(offLockColor);
        }
        
        private void OnValidate()
        {
            gameObject.name = $"{nameof(DiceLockChecker)} ({playerType})";
        }
    }
}