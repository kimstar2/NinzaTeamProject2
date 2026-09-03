using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY.Scripts.Events;
using Members.KJY.Scripts.Module.Util;

namespace Members.KJY.Scripts.Dice
{
    public class DiceDataRaiser : MonoEvent
    {
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
        }

        public void RaiseDiceData()
        {
            EventChannel.RaiseEvent(new OnDiceDataBind(new DiceDataSO()));
        }
    }
}