using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Members.KJY._01.Scripts.Agent.Enemy;

namespace Members.KJY._01.Scripts.GameSystem.EnemyParty
{
    public enum EnemyPartyType
    {
        Stage1,
        Stage2,
        Stage3
    }

    [CreateAssetMenu(fileName = "EnemyPartyList", menuName = "KJY/System/EnemyPartyList", order = 0)]
    public class EnemyPartyListSO : ScriptableObject
    {
        [field:SerializeField] public List<EnemyParty> EnemyPartyList {get; private set;}

        public int StageCount => EnemyPartyList.Count;

        public BattleDataSO CreateEncounter(BattleDataSO template, int stageIndex, float progress, EnemyRank rank, int seed)
        {
            if (template == null || stageIndex < 0 || stageIndex >= StageCount) return null;
            EnemyParty stage = EnemyPartyList[stageIndex];
            EnemyDataSO[] pool = stage.EnemyDataList?.Where(e => e != null && e != stage.Boss).OrderBy(e => e.Cost).ToArray();
            if (pool == null || pool.Length == 0 || (rank == EnemyRank.Boss && stage.Boss == null)) return null;

            progress = Mathf.Clamp01(progress);
            var random = new System.Random(seed);
            var enemies = new List<EnemyDataSO>();
            int count = rank == EnemyRank.Boss ? 1 : Mathf.Clamp(2 + Mathf.FloorToInt(progress * 2.5f), 2, 4);
            // 앞부분은 낮은 Cost, 후반은 높은 Cost의 적이 중심이 된다.
            int first = Mathf.FloorToInt(progress * Mathf.Max(0, pool.Length - 4));
            int last = Mathf.Min(pool.Length, first + Mathf.Max(3, Mathf.CeilToInt(pool.Length * 0.45f)));
            float health = Mathf.Lerp(1f, Mathf.Max(1f, stage.LateHealthMultiplier), progress);
            float power = Mathf.Lerp(1f, Mathf.Max(1f, stage.LateDicePower), progress);
            var available = new List<EnemyDataSO>(pool.Skip(first).Take(last - first));

            for (int i = 0; i < count; i++)
            {
                EnemyDataSO source;
                if (rank == EnemyRank.Boss) source = stage.Boss;
                else
                {
                    if (available.Count == 0) available.AddRange(pool.Skip(first).Take(last - first));
                    int index = random.Next(available.Count);
                    source = available[index];
                    available.RemoveAt(index);
                }
                EnemyRank unitRank = rank == EnemyRank.Boss ? EnemyRank.Boss :
                    rank == EnemyRank.Elite && i == 0 ? EnemyRank.Elite : EnemyRank.Normal;
                EnemyDataSO enemy = Instantiate(source);
                enemy.hideFlags = HideFlags.DontSave;
                float rankHealth = unitRank == EnemyRank.Boss ? 3.2f : unitRank == EnemyRank.Elite ? 1.55f : 1f;
                float rankPower = unitRank == EnemyRank.Boss ? 1.2f : unitRank == EnemyRank.Elite ? 1.12f : 1f;
                enemy.ConfigureEncounter(unitRank, health * rankHealth, power * rankPower,
                    unitRank != EnemyRank.Normal || progress >= 0.55f);
                enemies.Add(enemy);
            }

            BattleDataSO battle = Instantiate(template);
            battle.hideFlags = HideFlags.DontSave;
            float rewardMultiplier = rank == EnemyRank.Boss ? 3f : rank == EnemyRank.Elite ? 1.7f : 1f;
            int gold = Mathf.RoundToInt(Mathf.Max(10, stage.BaseGoldReward) * (1f + progress * 0.5f) * rewardMultiplier);
            battle.SetEncounter(stage.StageName, rank, gold, enemies);
            return battle;
        }
    }
}
