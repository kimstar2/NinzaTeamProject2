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
        [field: SerializeField] public StatusType Status { get; private set; }
        [field: SerializeField, Min(0f)] public float StatusDamage { get; private set; }
        [field: SerializeField, Min(0)] public int StatusTurns { get; private set; }
        [field: SerializeField] public StatusType BonusVsStatus { get; private set; }
        [field: SerializeField, Min(0f)] public float BonusDamage { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float MissChance { get; private set; }
        [field: SerializeField, Min(0)] public int MissTurns { get; private set; }
        [field: SerializeField, Min(0f)] public float HealthCost { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float ExecuteThreshold { get; private set; }
        [field: SerializeField, Min(0)] public int StunTurns { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float ReflectRatio { get; private set; }
        [field: SerializeField, Min(0)] public int ReflectTurns { get; private set; }
        [field: SerializeField] public bool CleanseDebuffs { get; private set; }
        [field: SerializeField] public bool IsArea { get; private set; }
        [field: SerializeField, Min(0)] public int InvulnerableTurns { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float HealMaxHealthRatio { get; private set; }
        [field: SerializeField, Min(0)] public int TauntTurns { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float ResistRatio { get; private set; }
        [field: SerializeField, Min(0)] public int ResistTurns { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float WeakenRatio { get; private set; }
        [field: SerializeField, Min(0)] public int WeakenTurns { get; private set; }
        [field: SerializeField, Range(0f, 2f)] public float PowerUpRatio { get; private set; }
        [field: SerializeField, Min(0)] public int PowerUpTurns { get; private set; }
        [field: SerializeField] public bool SelfDestruct { get; private set; }
        [field: SerializeField] public bool NoCastMotion { get; private set; } // 모션 없이 즉시 적용

        // 레벨당 +25%
        public float GetHealRatio(float level) => HealMaxHealthRatio * (1f + Mathf.Max(0f, level - 1f) * 0.25f);
        [field: SerializeField, Tooltip("디버프가 걸렸을 때 문구와 캐릭터 색. 투명(알파 0)이면 종류별 기본색")]
        public Color EffectColor { get; private set; } = Color.clear;
        [field: SerializeField] public TargetRule EnemyTargetRule { get; private set; }

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

        public string GetDescription(float level, bool usedByEnemy = false)
        {
            string target = usedByEnemy ? GetEnemyTargetLabel() :
                Target == TargetType.Self ? "[자신] " :
                Target == TargetType.Ally ? (IsArea ? "[아군 전체] " : "[아군 선택] ") :
                IsArea ? "[적 전체] " : "[적 선택] ";
            string synergy = Synergy switch
            {
                SynergyType.Mark => "표식: 이후 받는 공격 2회의 피해가 25% 증가합니다.",
                SynergyType.ExploitMark => "표식이 있으면 모두 소모하여 이번 타격의 피해가 50% 증가합니다.",
                SynergyType.Guard => "보호: 이후 받는 공격 2회의 피해를 30% 줄입니다.",
                SynergyType.Empower => "격려: 다음 공격·회복 스킬의 효과가 35% 증가합니다. 중첩되지 않습니다.",
                _ => string.Empty
            };
            return target + GetStatDescription(level) + (synergy.Length > 0 ? "\n" + synergy : "") + GetExtraDescription(level);
        }

        private string GetEnemyTargetLabel() => Target switch
        {
            TargetType.Self => "[자신] ",
            TargetType.Ally => IsArea ? "[아군 전체] " : "[아군 낮은 체력] ",
            TargetType.Enemy when IsArea => "[전체] ",
            _ => EnemyTargetRule switch
            {
                TargetRule.Random => "[무작위] ",
                TargetRule.LowestHealth => "[낮은 체력] ",
                _ => "[보복] "
            }
        };

        private static string StatusName(StatusType status) => CombatEffects.StatusName(status);

        private string GetExtraDescription(float level)
        {
            var text = new System.Text.StringBuilder();
            if (HealMaxHealthRatio > 0f)
                text.Append($"\n회복: 대상 최대 체력의 {GetHealRatio(level) * 100f:0.#}%를 회복합니다.");
            if (InvulnerableTurns > 0)
                text.Append($"\n무적: {InvulnerableTurns}턴 동안 받는 공격 피해를 모두 무시합니다. (지속 피해 제외)");
            if (TauntTurns > 0)
                text.Append($"\n도발: {TauntTurns}턴 동안 적의 단일 공격이 대상에게 향합니다.");
            if (ResistRatio > 0f && ResistTurns > 0)
                text.Append($"\n피해 감소: {ResistTurns}턴 동안 받는 공격 피해가 {ResistRatio * 100f:0}% 줄어듭니다.");
            if (WeakenRatio > 0f && WeakenTurns > 0)
                text.Append($"\n둔화: {WeakenTurns}턴 동안 대상이 주는 피해가 {WeakenRatio * 100f:0}% 줄어듭니다.");
            if (PowerUpRatio > 0f && PowerUpTurns > 0)
                text.Append($"\n공격력 증가: {PowerUpTurns}턴 동안 대상이 주는 피해가 {PowerUpRatio * 100f:0}% 늘어납니다.");
            if (SelfDestruct)
                text.Append("\n자폭: 스킬을 쓴 뒤 시전자가 쓰러집니다.");
            if (Status != StatusType.None && StatusDamage > 0f && StatusTurns > 0)
                text.Append($"\n{StatusName(Status)}: {StatusTurns}턴 동안 턴이 끝날 때마다 {StatusDamage:0.#} 피해를 줍니다.");
            if (BonusVsStatus != StatusType.None && BonusDamage > 0f)
                text.Append($"\n{StatusName(BonusVsStatus)} 상태인 대상에게 {BonusDamage:0.#} 추가 피해를 줍니다.");
            if (MissChance > 0f && MissTurns > 0)
                text.Append($"\n빗나감: {MissTurns}턴 동안 대상의 공격이 {MissChance * 100f:0}% 확률로 빗나갑니다.");
            if (ExecuteThreshold > 0f)
                text.Append($"\n처형: 공격 후 대상 체력이 {ExecuteThreshold * 100f:0}% 이하면 즉시 처치합니다.");
            if (StunTurns > 0)
                text.Append($"\n행동 불가: {StunTurns}턴 동안 대상이 공격할 수 없습니다. (보스는 턴마다 공격 1회 감소)");
            if (ReflectRatio > 0f && ReflectTurns > 0)
                text.Append($"\n반사: {ReflectTurns}턴 동안 받은 피해의 {ReflectRatio * 100f:0}%를 공격한 적에게 되돌립니다.");
            if (CleanseDebuffs)
                text.Append("\n정화: 대상에게 걸린 디버프(표식, 독·화상·낙인, 빗나감, 행동 불가)를 모두 제거합니다.");
            if (HealthCost > 0f)
                text.Append($"\n시전 시 자신의 체력을 {HealthCost:0.#} 소모합니다.");
            return text.ToString();
        }

        private string GetStatDescription(float level)
        {
            if (string.IsNullOrEmpty(SkillDescription) || applyStats.Count == 0) return SkillDescription;
            object[] values = new object[applyStats.Count];
            for (int i = 0; i < applyStats.Count; i++)
                values[i] = applyStats[i].GetScaledValue(level).ToString("0.#", CultureInfo.InvariantCulture);

            try { return string.Format(CultureInfo.InvariantCulture, SkillDescription, values); }
            catch (FormatException)
            {
                return SkillDescription;
            }
        }
    }
}
