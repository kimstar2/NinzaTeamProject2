using System;
using DevLib.CoreLib.Runtime;
using DevLib.HashDataSystem;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.UI;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.Agent
{
    /// <summary>
    /// 얘가 선택 관련도 하긴하는데
    /// 막상 그냥 컨드롤러라고 보면 됨..
    /// 나누기 귀찮아서 그런건 ㅈㅅ
    /// </summary>
    public abstract class AbstractSelector : ModuleOwner ,  IStatApply
    {
        [field:Header("Object Setting")]
        [field: SerializeField] public Transform LineConnectTrm { get; private set; }
        [field:SerializeField] public Transform DefaultPosition { get; private set; }
        [field:SerializeField] public AbstractAgent MyAgent { get; private set; }
        [field:Header("Game Setting")]
        [field:SerializeField] public bool IsSelect {get; protected set;} 
        [field:SerializeField] public UIMonoImage IconImage { get; private set; }
        [SerializeField] protected EventChannelSO eventChannel;
        [SerializeField] private HashDataSO deathHash; 
        public AgentDataSO AgentData { get; protected set; }
        public bool IsDead {get; protected set;}
        public UnityEvent onSelect;
        public UnityEvent onUnSelect;
        public UnityEvent<float> onHealthChanged;
        public TweenLayoutGroup TweenLayoutGroup { get; private set; }
        [field: SerializeField] public TweenLayoutGroup DiceLayoutGroup { get; private set; }
        [field: SerializeField] public Transform DiceLayoutTarget { get; private set; }
        protected bool isFirstInit = true;
        
        #region Modules

        public SkillExecutor SkillExecutor { get; private set; }
        public AbstractDiceInventory DiceInventory { get; private set; }

        #endregion
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            SkillExecutor = GetModule<SkillExecutor>();
            DiceInventory = GetModule<AbstractDiceInventory>();
            TweenLayoutGroup = GetComponentInParent<TweenLayoutGroup>();
        }

        public void HideFromBattle()
        {
            TweenLayoutGroup.Hide(transform);
            DiceLayoutGroup.Hide(DiceLayoutTarget);
            MyAgent.gameObject.SetActive(false);
        }

        protected void EnterBattle()
        {
            MyAgent.gameObject.SetActive(true);
            MyAgent.transform.position = DefaultPosition.position;
            DiceLayoutGroup.Add(DiceLayoutTarget);
            TweenLayoutGroup.Add(transform);
        }
        
        protected virtual void HandleDead()
        {
            MyAgent.AnimCompo.RenderClip(deathHash.HashValue);
            IsDead = true;
        }
        
        protected void HandleHealthChanged(float health, float maxHealth)
        {
            onHealthChanged?.Invoke(health / maxHealth);
        }

        protected virtual void Start()
        {
            MyAgent.HealthModule.OnDead += HandleDead;
            MyAgent.HealthModule.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(MyAgent.HealthModule.CurrentHealth, MyAgent.HealthModule.DefaultMaxHealth);
        }

        protected virtual void OnDestroy()
        {
            MyAgent.HealthModule.OnDead -= HandleDead;
            MyAgent.HealthModule.OnHealthChanged -= HandleHealthChanged;
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

        public abstract void OnAttackCommand();
        
        public void ApplyStat(ApplyStatType statType, float value)
        {
            switch (statType)
            {
                case ApplyStatType.Damage:
                    ApplyDamage(value);
                    break;
                case ApplyStatType.Heal:
                    ApplyHeal(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }
        public abstract void ApplyDamage(float damage);
        public abstract void ApplyHeal(float heal);
        public abstract float GetLevel();

    }
}
