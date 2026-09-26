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

        public IReadOnlyList<SkillApplyStat> ApplyStats => applyStats;

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
