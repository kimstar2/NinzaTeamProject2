using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Agent.Skill;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent
{
    /// <summary>
    /// 얘가 선택 관련도 하긴하는데
    /// 막상 그냥 컨드롤러라고 보면 됨..
    /// 나누기 귀찮아서 그런건 ㅈㅅ
    /// </summary>
    public abstract class AbstractSelector : ModuleOwner ,  IDamageable
    {
        [field:SerializeField] public bool IsSelect {get; protected set;} 
        [field: SerializeField] public Transform LineConnectTrm { get; private set; }
        [field: SerializeField] public Transform MyTransform { get; private set; }
        [SerializeField] protected EventChannelSO eventChannel;
        public UnityEvent onSelect;
        public UnityEvent onUnSelect;

        #region Modules

        public SkillExecutor SkillExecutor { get; private set; }
        public HealthModule HealthModule {get; private set;}
        public AbstractDiceInventory DiceInventory { get; private set; }

        #endregion
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            HealthModule = GetModule<HealthModule>();
            SkillExecutor = GetModule<SkillExecutor>();
            DiceInventory = GetModule<AbstractDiceInventory>();
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