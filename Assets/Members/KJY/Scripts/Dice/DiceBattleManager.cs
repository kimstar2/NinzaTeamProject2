using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY.Scripts.Events;
using Members.KJY.Scripts.Module.Util;
using UnityEngine;

namespace Members.KJY.Scripts.Dice
{
    public class DiceBattleManager : ModuleOwner
    {
        private EventChannelSO _eventChannelSO;
        private DiceRerollRaiser _raiser;
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            _eventChannelSO = GetModule<GetEventChannel>().EventChannel;
        }
    }
}