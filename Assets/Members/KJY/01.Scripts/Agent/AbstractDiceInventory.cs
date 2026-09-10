using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public abstract class AbstractDiceInventory : MonoBehaviour
    {
        [field:SerializeField] public DiceDataListSO RunTimeDiceDataList {get; private set;}
        [SerializeField] protected DiceDataListSO defaultDiceDataList;
        [SerializeField] protected EventChannelSO eventChannel;
        protected DiceDataSO savedDiceData;
        
        protected virtual void Awake()
        {
            RunTimeDiceDataList = defaultDiceDataList.GetRuntimeList();
        }
        public abstract void DiceDataChanged();
        public abstract void Apply();
    }
}