using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY.Scripts.Events;
using Members.KJY.Scripts.Module.Util;
using UnityEngine;

namespace Members.KJY.Scripts.Dice
{
    public class DiceRollRaiser : MonoModule
    {
        private EventChannelSO _eventChannel;
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _eventChannel = Owner.GetModule<GetEventChannel>().EventChannel;
        }

        public void Roll()
        {
            _eventChannel.RaiseEvent(new OnRoll());
        }
    }
}