using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{
    [Serializable]
    public struct SkillApplyStat
    {
        [field: SerializeField] public ApplyStatType ApplyStatType { get; private set; }
        [field: SerializeField] public float Value { get; private set; }

        public float GetScaledValue(float level) => Value * level;
    }

    [CreateAssetMenu(fileName = "Skill data", menuName = "KJY/Skill/Skill data", order = 0)]
    public class SkillDataSO : ScriptableObject
    {
        public enum TargetType { Enemy, Ally, Self }
        public enum SynergyType { None, Mark, ExploitMark, Guard, Empower }
        public enum DamageCondition
        {
            None,
            WoundedTarget,
            WoundedCaster,
            HealthyTarget
        }

        [field: SerializeField] public SkillLogicExecutor SkillLogicExecutor { get; private set; }
        [field: SerializeField] public string SkillName {get; private set;}
        [field: SerializeField] public Sprite Icon {get; private set;}
        [field: SerializeField,TextArea] public string SkillDescription {get; private set;}
        [SerializeField] private List<SkillApplyStat> applyStats = new();
        [SerializeField] private DamageCondition damageCondition;
        [field: SerializeField, Range(0f, 1f)] public float LifeStealFraction { get; private set; }
        [field: SerializeField] public TargetType Target { get; private set; }
        [field: SerializeField] public SynergyType Synergy { get; private set; }
        // 플레이어에게 보이지 않는 확률 보정용 수치. 행운이 오를수록 높은 스킬이 잘 나온다.
        [field: SerializeField, Min(0)] public int Strength { get; private set; }

        public IReadOnlyList<SkillApplyStat> ApplyStats => applyStats;

        public bool CanTarget(AbstractSelector attacker, AbstractSelector target)
        {
            if (attacker == null || target == null || attacker.IsDead || target.IsDead ||
                attacker.AgentData == null || target.AgentData == null) return false;
            bool sameTeam = (attacker.AgentData is Player.PlayerDataSO) == (target.AgentData is Player.PlayerDataSO);
            return Target switch
            {
                TargetType.Ally => sameTeam,
                TargetType.Self => attacker == target,
                _ => !sameTeam
            };
        }

        // 조건 수치는 설명과 함께 고정한다. 기본 피해량은 기존 Apply Stats에서 조절한다.
        public float GetDamageMultiplier(float casterHealthRatio, float targetHealthRatio)
        {
            return damageCondition switch
            {
                DamageCondition.WoundedTarget when targetHealthRatio <= 0.35f => 1.6f,
                DamageCondition.WoundedCaster when casterHealthRatio <= 0.5f => 1.5f,
                DamageCondition.HealthyTarget when targetHealthRatio >= 0.8f => 1.5f,
                _ => 1f
            };
        }

        public float GetScaledStat(ApplyStatType statType, float level)
        {
            foreach (SkillApplyStat stat in applyStats)
                if (stat.ApplyStatType == statType) return stat.GetScaledValue(level);
            return 0f;
        }

        public string GetDescription(float level)
        {
            string target = Target == TargetType.Ally ? "[아군 선택] " : Target == TargetType.Self ? "[자신] " : "[적 선택] ";
            string synergy = Synergy switch
            {
                SynergyType.Mark => "표식: 이후 받는 공격 2회의 피해가 25% 증가합니다.",
                SynergyType.ExploitMark => "표식이 있으면 모두 소모하여 이번 타격의 피해가 50% 증가합니다.",
                SynergyType.Guard => "보호: 이후 받는 공격 2회의 피해를 30% 줄입니다.",
                SynergyType.Empower => "격려: 다음 공격·회복 스킬의 효과가 35% 증가합니다. 중첩되지 않습니다.",
                _ => string.Empty
            };
            return target + GetStatDescription(level) + (synergy.Length > 0 ? "\n" + synergy : "");
        }

        private string GetStatDescription(float level)
        {
            if (string.IsNullOrEmpty(SkillDescription) || applyStats.Count == 0) return SkillDescription;
            object[] values = new object[applyStats.Count];
            for (int i = 0; i < applyStats.Count; i++)
                values[i] = applyStats[i].GetScaledValue(level).ToString("F1", CultureInfo.InvariantCulture);

            try { return string.Format(CultureInfo.InvariantCulture, SkillDescription, values); }
            catch (FormatException)
            {
                return SkillDescription;
            }
        }
    }
}
