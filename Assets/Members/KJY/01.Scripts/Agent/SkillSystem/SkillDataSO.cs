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

    public enum TargetRule { Default, Random, LowestHealth }

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
        [Tooltip("이 스킬 면을 장착할 수 있는 직업. 비우면 모든 직업 가능")]
        [SerializeField] private List<AgentAttackType> suitableTypes = new();
        public IReadOnlyList<AgentAttackType> SuitableTypes => suitableTypes;
        public bool IsSuitable(AgentAttackType type) => suitableTypes.Count == 0 || suitableTypes.Contains(type);

        public static string RoleName(AgentAttackType type) => type switch
        {
            AgentAttackType.Archer => "궁수",
            AgentAttackType.Melee => "전사",
            AgentAttackType.Magic => "마법사",
            AgentAttackType.Healer => "힐러",
            AgentAttackType.Tank => "탱커",
            _ => type.ToString()
        };

        public string SuitableDescription => suitableTypes.Count == 0 ? "모든 직업" :
            string.Join(", ", suitableTypes.ConvertAll(RoleName));

        [field: Header("Status")]
        [field: SerializeField] public StatusType Status { get; private set; } // 맞은 대상에게 거는 지속 피해
        [field: SerializeField, Min(0f)] public float StatusDamage { get; private set; }
        [field: SerializeField, Min(0)] public int StatusTurns { get; private set; }
        [field: SerializeField] public StatusType BonusVsStatus { get; private set; } // 이 상태인 대상에게 추가 피해
        [field: SerializeField, Min(0f)] public float BonusDamage { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float MissChance { get; private set; } // 맞은 대상의 공격이 빗나갈 확률
        [field: SerializeField, Min(0)] public int MissTurns { get; private set; }
        [field: SerializeField, Min(0f)] public float HealthCost { get; private set; } // 시전자가 소모하는 체력 (죽지는 않음)
        [field: SerializeField, Range(0f, 1f)] public float ExecuteThreshold { get; private set; } // 타격 후 이 비율 이하면 즉시 처치
        [field: SerializeField, Min(0)] public int StunTurns { get; private set; } // 맞은 대상이 공격하지 못하는 턴
        [field: SerializeField] public TargetRule EnemyTargetRule { get; private set; } // 적(몬스터)이 쓸 때 대상 고르는 방식

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
            return target + GetStatDescription(level) + (synergy.Length > 0 ? "\n" + synergy : "") + GetExtraDescription();
        }

        private static string StatusName(StatusType status) => CombatEffects.StatusName(status);

        private string GetExtraDescription()
        {
            var text = new System.Text.StringBuilder();
            if (Status != StatusType.None && StatusDamage > 0f && StatusTurns > 0)
                text.Append($"\n{StatusName(Status)}: {StatusTurns}턴 동안 턴이 끝날 때마다 {StatusDamage:0.#} 피해를 줍니다.");
            if (BonusVsStatus != StatusType.None && BonusDamage > 0f)
                text.Append($"\n{StatusName(BonusVsStatus)} 상태인 대상에게 {BonusDamage:0.#} 추가 피해를 줍니다.");
            if (MissChance > 0f && MissTurns > 0)
                text.Append($"\n빗나감: {MissTurns}턴 동안 대상의 공격이 {MissChance * 100f:0}% 확률로 빗나갑니다.");
            if (ExecuteThreshold > 0f)
                text.Append($"\n처형: 공격 후 대상 체력이 {ExecuteThreshold * 100f:0}% 이하면 즉시 처치합니다.");
            if (StunTurns > 0)
                text.Append($"\n행동 불가: {StunTurns}턴 동안 대상이 공격할 수 없습니다.");
            if (Target == TargetType.Enemy && EnemyTargetRule != TargetRule.Default)
                text.Append(EnemyTargetRule == TargetRule.Random ? "\n(몬스터 사용 시 무작위 대상)" : "\n(몬스터 사용 시 체력이 가장 낮은 대상)");
            if (HealthCost > 0f)
                text.Append($"\n시전 시 자신의 체력을 {HealthCost:0.#} 소모합니다.");
            return text.ToString();
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
