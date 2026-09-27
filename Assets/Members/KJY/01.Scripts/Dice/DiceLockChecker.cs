using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Agent.Player.Dice;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceLockChecker : MonoModule
    {
        [SerializeField] private PlayerType playerType;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private PlayerDiceRollManager rollManager;

        [Header("Lock Setting")] 
        [SerializeField] private UIMonoImage diceImage;
        [SerializeField] private Color onLockColor;
        [SerializeField] private Color offLockColor;
        public bool IsLocked {get; private set;}

        private void OnEnable()
        {
            eventChannel.AddListener<OnStartBattle>(HandleStartBattle);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnStartBattle>(HandleStartBattle);
        }

        private void HandleStartBattle(OnStartBattle obj) => OffLock();


        public void LockToggle()
        {
            if (rollManager == null || !rollManager.AllDiceRollEnd) return;
            if (!IsLocked)
                OnLock();
            else
                OffLock();
        }

        public void OnLock()
        {
            if (rollManager == null || !rollManager.AllDiceRollEnd) return;
            IsLocked = true;
            diceImage.SetColor(onLockColor);
            eventChannel.RaiseEvent(new OnDiceLock(IsLocked,playerType));
        }

        
        public void OffLock()
        {
            IsLocked = false;
            diceImage.SetColor(offLockColor);
            eventChannel.RaiseEvent(new OnDiceLock(IsLocked,playerType));
        }

        public void RefreshColor() => diceImage.SetColor(IsLocked ? onLockColor : offLockColor);
        
        
        private void OnValidate()
        {
            gameObject.name = $"{nameof(DiceLockChecker)} ({playerType})";
        }
    }
}
