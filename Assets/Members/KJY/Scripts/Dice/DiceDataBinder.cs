using System;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY.Scripts.Events;
using Members.KJY.Scripts.Module.Util;
using TMPro;
using UnityEngine;

namespace Members.KJY.Scripts.Dice
{
    public class DiceDataBinder : MonoModule , IAfterInitModule
    {
        [SerializeField] private Transform titleContainer;
        [SerializeField] private Transform descContainer;
        private EventChannelSO _eventChannel;
        private TextMeshProUGUI _titleTMP;
        private TextMeshProUGUI _descTMP;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _eventChannel = Owner.GetModule<GetEventChannel>().EventChannel;
            _titleTMP = titleContainer.GetComponentInChildren<TextMeshProUGUI>();
            _descTMP = descContainer.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        public void AfterInit()
        {
            _eventChannel.AddListener<OnDiceDataBind>(HandleDiceDataBind);
        }


        private void OnDestroy()
        {
            _eventChannel.RemoveListener<OnDiceDataBind>(HandleDiceDataBind);
        }

        private void HandleDiceDataBind(OnDiceDataBind obj)
        {
        }
    }
}