using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY.Scripts.Events;
using Members.KJY.Scripts.Module.Util;

namespace Members.KJY.Scripts.Dice
{
    public class DiceRerollRaiser : MonoModule
    {
        private EventChannelSO _eventChannel;
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _eventChannel = Owner.GetModule<GetEventChannel>().EventChannel;
        }

        public void Reroll()
        {
            _eventChannel.RaiseEvent(new OnReroll());
        }
    }
}