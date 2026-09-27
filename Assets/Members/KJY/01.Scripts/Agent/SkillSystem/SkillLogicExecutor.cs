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
        private readonly HashSet<AbstractSelector> _effectApplied = new();
        private readonly List<AbstractSelector> _targets = new();
        private bool _missShown;
        
        public void SkillFinished()
        {
            // 자폭: 공격이 모두 끝난 뒤 시전자가 쓰러진다
            if (_executed && SkillData != null && SkillData.SelfDestruct && Attacker != null && !Attacker.IsDead)
            {
                Attacker.Effects.ShowPopup("자폭!", new Color(1f, 0.45f, 0.3f));
                Attacker.ApplyDamage(Attacker.MyAgent.HealthModule.CurrentHealth);
            }
            _executed = false;
            OnSkillFinished?.Invoke();
        }

        private bool _executed; // 대상이 없어 바로 끝난 경우에는 자폭하지 않음
        public void SkillExecute(AbstractSelector attacker, AbstractSelector target, AgentType agentType, SkillDataSO skillData)
        {
            // 광역 공격은 연출이 향할 대상을 상대 진영 가운데로 정한다 (피해는 전체)
            if (skillData.IsArea && skillData.Target == SkillDataSO.TargetType.Enemy)
                target = FindAreaCenter(attacker, skillData) ?? target;
            else if (skillData.Target == SkillDataSO.TargetType.Enemy)
                target = FindTaunter(attacker, skillData) ?? target; // 도발 중인 상대가 있으면 단일 공격은 그쪽으로
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
                skillData.GetScaledStat(ApplyStatType.Heal, 1f) > 0f || skillData.HealMaxHealthRatio > 0f;
            PowerMultiplier = hasPower ? Attacker.Effects.UseEmpower() : 1f;
            IsMissed = skillData.GetScaledStat(ApplyStatType.Damage, 1f) > 0f && Attacker.Effects.RollMiss();
            _effectApplied.Clear();
            _missShown = false;
            _executed = true;
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

        private static AbstractSelector FindTaunter(AbstractSelector attacker, SkillDataSO skillData)
        {
            foreach (var selector in AbstractSelector.InBattle)
                if (selector != null && !selector.IsDead && selector.Effects.TauntTurns > 0 && skillData.CanTarget(attacker, selector))
                    return selector;
            return null;
        }

        // 살아 있는 상대를 위에서부터 줄 세웠을 때 가운데. 짝수면 가운데 두 명 중 위쪽.
        private static AbstractSelector FindAreaCenter(AbstractSelector attacker, SkillDataSO skillData)
        {
            var candidates = new List<AbstractSelector>();
            foreach (var selector in AbstractSelector.InBattle)
                if (selector != null && !selector.IsDead && skillData.CanTarget(attacker, selector)) candidates.Add(selector);
            if (candidates.Count == 0) return null;
            candidates.Sort((a, b) =>
            {
                int byHeight = b.DefaultPosition.position.y.CompareTo(a.DefaultPosition.position.y);
                return byHeight != 0 ? byHeight : a.DefaultPosition.position.x.CompareTo(b.DefaultPosition.position.x);
            });
            return candidates[(candidates.Count - 1) / 2];
        }

        // 광역이면 대상 진영 전체, 아니면 지정한 대상 한 명
        public List<AbstractSelector> GetTargets()
        {
            _targets.Clear();
            if (SkillData == null || !SkillData.IsArea)
            {
                if (Target != null) _targets.Add(Target);
                return _targets;
            }
            foreach (var selector in AbstractSelector.InBattle)
                if (selector != null && !selector.IsDead && SkillData.CanTarget(Attacker, selector)) _targets.Add(selector);
            if (_targets.Count == 0 && Target != null) _targets.Add(Target);
            return _targets;
        }

        // 빗나감 문구는 한 번 시전에 한 번만 띄운다
        public void ShowMissOnce(AbstractSelector target)
        {
            if (_missShown || target == null) return;
            _missShown = true;
            target.Effects.ShowPopup("빗나감", new Color(0.8f, 0.8f, 0.8f));
        }

        public void ApplySynergy() => ApplySynergy(Target);

        // 대상마다 한 번씩 부가효과(정화, 반사, 상태이상, 빗나감, 행동 불가, 시너지)를 건다
        public void ApplySynergy(AbstractSelector target)
        {
            if (target == null || target.IsDead || IsMissed || !_effectApplied.Add(target)) return;
            var effects = target.Effects;
            Color custom = SkillData.EffectColor;
            Color ColorOf(CombatEffects.DebuffKind kind) => custom.a > 0f ? custom : CombatEffects.DefaultColor(kind);

            if (SkillData.CleanseDebuffs)
            {
                effects.ClearDebuffs(); // 새 효과를 걸기 전에 먼저 정화
                effects.ShowPopup("정화!", new Color(0.6f, 1f, 1f));
            }
            if (SkillData.PowerUpRatio > 0f && SkillData.PowerUpTurns > 0)
            {
                effects.AddPowerUp(SkillData.PowerUpRatio, SkillData.PowerUpTurns);
                effects.ShowPopup("공격력 증가!", new Color(1f, 0.8f, 0.35f));
            }
            if (SkillData.TauntTurns > 0)
            {
                effects.AddTaunt(SkillData.TauntTurns);
                effects.ShowPopup("도발!", new Color(1f, 0.7f, 0.3f));
            }
            if (SkillData.ResistRatio > 0f && SkillData.ResistTurns > 0)
            {
                effects.AddResist(SkillData.ResistRatio, SkillData.ResistTurns);
                effects.ShowPopup("피해 감소!", new Color(0.6f, 0.85f, 1f));
            }
            if (SkillData.WeakenRatio > 0f && SkillData.WeakenTurns > 0)
            {
                effects.AddWeaken(SkillData.WeakenRatio, SkillData.WeakenTurns);
                effects.RememberDebuff(CombatEffects.DebuffKind.Weaken, ColorOf(CombatEffects.DebuffKind.Weaken));
                effects.ShowPopup("둔화!", ColorOf(CombatEffects.DebuffKind.Weaken));
            }
            if (SkillData.InvulnerableTurns > 0)
            {
                effects.AddInvulnerable(SkillData.InvulnerableTurns);
                effects.ShowPopup("무적!", new Color(0.85f, 0.9f, 1f));
            }
            if (SkillData.ReflectRatio > 0f && SkillData.ReflectTurns > 0)
            {
                effects.AddReflect(SkillData.ReflectRatio, SkillData.ReflectTurns);
                effects.ShowPopup("반사!", new Color(0.85f, 0.9f, 1f));
            }
            if (SkillData.Status != StatusType.None && SkillData.StatusDamage > 0f && SkillData.StatusTurns > 0)
            {
                var kind = CombatEffects.ToKind(SkillData.Status);
                effects.AddStatus(SkillData.Status, SkillData.StatusDamage, SkillData.StatusTurns);
                effects.RememberDebuff(kind, ColorOf(kind));
                effects.ShowPopup(CombatEffects.StatusName(SkillData.Status) + "!", ColorOf(kind));
            }
            if (SkillData.MissChance > 0f && SkillData.MissTurns > 0)
            {
                effects.AddMiss(SkillData.MissChance, SkillData.MissTurns);
                effects.RememberDebuff(CombatEffects.DebuffKind.Miss, ColorOf(CombatEffects.DebuffKind.Miss));
                effects.ShowPopup("명중률 감소!", ColorOf(CombatEffects.DebuffKind.Miss));
            }
            if (SkillData.StunTurns > 0)
            {
                effects.AddStun(SkillData.StunTurns);
                effects.RememberDebuff(CombatEffects.DebuffKind.Stun, ColorOf(CombatEffects.DebuffKind.Stun));
                effects.ShowPopup("행동 불가!", ColorOf(CombatEffects.DebuffKind.Stun));
            }
            switch (SkillData.Synergy)
            {
                case SkillDataSO.SynergyType.Mark:
                    effects.Mark();
                    effects.RememberDebuff(CombatEffects.DebuffKind.Mark, ColorOf(CombatEffects.DebuffKind.Mark));
                    effects.ShowPopup("표식!", ColorOf(CombatEffects.DebuffKind.Mark));
                    break;
                case SkillDataSO.SynergyType.Guard:
                    effects.Guard();
                    effects.ShowPopup("보호!", new Color(0.6f, 0.85f, 1f));
                    break;
                case SkillDataSO.SynergyType.Empower:
                    effects.Empower();
                    effects.ShowPopup("격려!", new Color(1f, 0.85f, 0.4f));
                    break;
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
