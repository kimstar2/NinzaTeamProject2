using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
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
            SetDiceList(defaultDiceDataList);
        }

        public void SetDiceList(DiceDataListSO data)
        {
            DiceDataListSO source = data != null ? data : defaultDiceDataList;
            if (source == null) return;
            if (RunTimeDiceDataList != null) Destroy(RunTimeDiceDataList);
            RunTimeDiceDataList = source.GetRuntimeList();
            savedDiceData = RunTimeDiceDataList.Front;
        }

        private void OnDestroy()
        {
            if (RunTimeDiceDataList != null) Destroy(RunTimeDiceDataList);
        }
        
        public abstract void DiceDataChanged();
        public abstract void Apply();
        public DiceDataSO GetDiceData() => savedDiceData;
    }
}
