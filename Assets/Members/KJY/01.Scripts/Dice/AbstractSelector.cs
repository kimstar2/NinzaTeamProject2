using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Agent;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public abstract class AbstractSelector : ModuleOwner ,  IDamageable
    {
        [field:SerializeField] public bool IsSelect {get; protected set;} 
        [field: SerializeField] public Transform LineConnectTrm { get; private set; }
        [SerializeField] protected EventChannelSO eventChannel;
        public UnityEvent onSelect;
        public UnityEvent onUnSelect;

        #region Modules

        public HealthModule HealthModule {get; private set;}

        #endregion
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            HealthModule = GetModule<HealthModule>();
        }
        
        public void SelectToggle()
        {
            if (IsSelect)
                UnSelect();
            else
                Select();
        }

        protected abstract void Select();
        protected abstract void UnSelect();

        public abstract void ApplyDamage(float damage); // 추후 데이터 추가 예정
        public abstract void OnAttackCommand();
    }
}