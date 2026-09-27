using System;
using System.Collections.Generic;
using DevLib.HashDataSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using Members.KJY._01.Scripts.Flags;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{
    public class SkillLogicExecutor : MonoBehaviour , ISkillLogicExecutor , IRequirePooling
    {
        [field:SerializeField] public HashDataSO IdleAnimHash {get; private set;}
        [field:SerializeField] public HashDataSO SkillAnimHash {get; private set;}
        [SerializeField] private List<AbstractSkillLogic> skills;
        public AgentType AgentType { get; private set; }
        public event Action OnSkillFinished;
        public event Action OnSkillExecute;
        public UnityEvent onAnimFinished;
        
        public AbstractSelector Attacker { get; private set; }
        public AbstractSelector Target { get; private set; }
        public SkillDataSO SkillData { get; private set; }
        public float PowerMultiplier { get; private set; } = 1f;
        public bool IsMissed { get; private set; } // 빗나감 상태면 이번 시전의 피해가 전부 빗나감
        private bool _synergyApplied;
        
        public void SkillFinished() => OnSkillFinished?.Invoke();
        public void SkillExecute(AbstractSelector attacker, AbstractSelector target, AgentType agentType, SkillDataSO skillData)
        {
            Target = target;
            Attacker = attacker;
            AgentType = agentType;
            SkillData = skillData;
            
            if (!skillData.CanTarget(attacker, target))
            {
                SkillFinished();
                Remove();
                return;
            }
            bool hasPower = skillData.GetScaledStat(ApplyStatType.Damage, 1f) > 0f ||
                skillData.GetScaledStat(ApplyStatType.Heal, 1f) > 0f;
            PowerMultiplier = hasPower ? Attacker.Effects.UseEmpower() : 1f;
            IsMissed = skillData.GetScaledStat(ApplyStatType.Damage, 1f) > 0f && Attacker.Effects.RollMiss();
            _synergyApplied = false;
            PayHealthCost();
            
            Attacker.MyAgent.AnimTrigger.OnAnimFinished -= HandleAnimFinished;
            Attacker.MyAgent.AnimTrigger.OnAnimFinished += HandleAnimFinished;        
            
            Attacker.MyAgent.AnimTrigger.OnAttack -= HandleAttack;
            Attacker.MyAgent.AnimTrigger.OnAttack += HandleAttack;

            foreach (AbstractSkillLogic skillLogic in skills)
            {
                skillLogic.Init(this,attacker.GetLevel());
                skillLogic.Execute();
            }
            OnSkillExecute?.Invoke();
        }

        private void PayHealthCost()
        {
            if (SkillData.HealthCost <= 0f) return;
            var health = Attacker.MyAgent.HealthModule;
            float cost = Mathf.Min(SkillData.HealthCost, health.CurrentHealth - 1f); // 소모로 죽지는 않게 1은 남김
            if (cost > 0f) Attacker.ApplyDamage(cost);
        }

        private void HandleAttack()
        {
            Attacker.MyAgent.AnimTrigger.OnAttack -= HandleAttack;
           
            foreach (AbstractSkillLogic skillLogic in skills)
                skillLogic.Attack();
        }

        public void ApplySynergy()
        {
            if (_synergyApplied || Target == null || Target.IsDead || IsMissed) return;
            _synergyApplied = true;
            Target.Effects.AddStatus(SkillData.Status, SkillData.StatusDamage, SkillData.StatusTurns);
            Target.Effects.AddMiss(SkillData.MissChance, SkillData.MissTurns);
            Target.Effects.AddStun(SkillData.StunTurns);
            switch (SkillData.Synergy)
            {
                case SkillDataSO.SynergyType.Mark: Target.Effects.Mark(); break;
                case SkillDataSO.SynergyType.Guard: Target.Effects.Guard(); break;
                case SkillDataSO.SynergyType.Empower: Target.Effects.Empower(); break;
            }
        }

        private void HandleAnimFinished()
        {
            Attacker.MyAgent.AnimTrigger.OnAnimFinished -= HandleAnimFinished;
            onAnimFinished?.Invoke();
            
            foreach (AbstractSkillLogic skillLogic in skills)
                skillLogic.AnimEnd();
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Attacker == null || Attacker.MyAgent == null) return;
            var trigger = Attacker.MyAgent.AnimTrigger;
            if (trigger == null) return;
            trigger.OnAnimFinished -= HandleAnimFinished;
            trigger.OnAttack -= HandleAttack;
        }

        public void PlayAnim()
        {
            if (SkillAnimHash != null && SkillAnimHash.HashValue != NoneHash.Value)
                Attacker.MyAgent.AnimCompo.RenderClipIfNotPlaying(SkillAnimHash.HashValue);
        }
        
        public void PlayIdleAnim()
        {
            if (IdleAnimHash != null && IdleAnimHash.HashValue != NoneHash.Value)
                Attacker.MyAgent.AnimCompo.RenderClipIfNotPlaying(IdleAnimHash.HashValue);
        }
    }
}
