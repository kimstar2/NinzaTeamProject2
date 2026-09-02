using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY.Scripts.Events;
using Members.KJY.Scripts.Module.Util;

namespace Members.KJY.Scripts.Dice
{
    public class DiceDataRaiser : MonoModule
    {
        private EventChannelSO _eventChannel;
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _eventChannel = Owner.GetModule<GetEventChannel>().EventChannel;
        }

        public void RaiseDiceData()
        {
            _eventChannel.RaiseEvent(new OnDiceDataBind(new DiceDataSO()));
        }
    }
}