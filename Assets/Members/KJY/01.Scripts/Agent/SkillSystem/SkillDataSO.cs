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
        [field: SerializeField] public SkillLogicExecutor SkillLogicExecutor { get; private set; }
        [field: SerializeField] public string SkillName {get; private set;}
        [field: SerializeField] public Sprite Icon {get; private set;}
        [field: SerializeField,TextArea] public string SkillDescription {get; private set;}
        [SerializeField] private List<SkillApplyStat> applyStats = new();

        public IReadOnlyList<SkillApplyStat> ApplyStats => applyStats;

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
