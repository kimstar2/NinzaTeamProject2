using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public abstract class AbstractDiceInventory : MonoModule
    {
        [field:SerializeField] public DiceDataListSO RunTimeDiceDataList {get; private set;}
        [SerializeField] protected DiceDataListSO defaultDiceDataList;
        [SerializeField] protected EventChannelSO eventChannel;
        protected DiceDataSO savedDiceData;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            RunTimeDiceDataList = defaultDiceDataList.GetRuntimeList();
            
        }
        
        public abstract void DiceDataChanged();
        public abstract void Apply();
        public DiceDataSO GetDiceData() => savedDiceData;
    }
}