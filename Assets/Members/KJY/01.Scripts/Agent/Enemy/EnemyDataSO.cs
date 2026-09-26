using System;
using Members.KJY._01.Scripts.Util;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public enum EnemyRank { Normal, Elite, Boss }

    [CreateAssetMenu(fileName = "Enemy data", menuName = "KJY/Agent/Enemy data", order = 0)]
    public class EnemyDataSO : AgentDataSO
    {
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public AnimatorOverrideController EnemyAc { get; set; }
        [field: SerializeField] public Sprite EnemyImage { get; private set; }
        [field: SerializeField] public ColorSO ImageColor { get; private set; }
        [field: SerializeField] public int Cost {get; private set;}
        [field: SerializeField] public DiceDataListSO DiceDataList { get; private set; }
        [field: SerializeField, Tooltip("후반 노드와 정예가 사용하는 강화 주사위. 비워두면 기본 주사위를 사용합니다.")]
        public DiceDataListSO AdvancedDiceDataList { get; private set; }
        public EnemyRank Rank { get; private set; }
        public float DicePower { get; private set; } = 1f;
        public float HealthMultiplier { get; private set; } = 1f;
        public float EncounterHealth => MaxHealth * HealthMultiplier;
        public event Action<EnemyDataSO> OnDead;

        // 전투마다 복제한 데이터에만 적용한다. 원본 에셋의 밸런스 값은 유지한다.
        public void ConfigureEncounter(EnemyRank rank, float healthMultiplier, float dicePower, bool advancedDice)
        {
            Rank = rank;
            HealthMultiplier = Mathf.Max(0.1f, healthMultiplier);
            DicePower = Mathf.Max(0.1f, dicePower);
            if (advancedDice && AdvancedDiceDataList != null) DiceDataList = AdvancedDiceDataList;
        }

        public void Dead() => OnDead?.Invoke(this);
    }
}
