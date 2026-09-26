using System;
using System.Collections.Generic;
using Members.KJY._01.Scripts.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts
{
    [CreateAssetMenu(fileName = "Stage Data", menuName = "KJY/Node/StageData", order = 0)]
    public class StageDataSO : ScriptableObject
    {
        [Tooltip("약한 적부터 강한 적 순서")]
        public EnemyDataSO[] enemies;
        public EnemyDataSO boss;
        [Header("초반 → 후반")]
        public Vector2Int enemyCount = new(2, 4);
        public Vector2 healthMultiplier = new(1f, 1.35f);
        public Vector2 dicePower = new(1f, 1.15f);
        public Vector2Int gold = new(18, 48);

        public bool IsValid => enemies != null && enemies.Length > 0 && boss != null &&
            Array.TrueForAll(enemies, enemy => enemy != null && enemy != boss);

        public BattleData CreateBattle(int column, int lastColumn, int seed, EnemyRank rank)
        {
            if (!IsValid) return null;
            float progress = Mathf.Clamp01(column / (float)Mathf.Max(1, lastColumn));
            var random = new System.Random(seed);
            int minCount = Mathf.Clamp(enemyCount.x, 1, 4);
            int maxCount = Mathf.Clamp(enemyCount.y, minCount, 4);
            int count = rank == EnemyRank.Boss ? 1 : Mathf.RoundToInt(Mathf.Lerp(minCount, maxCount, progress));
            var picked = new EnemyDataSO[count];
            int width = Mathf.Min(enemies.Length, Mathf.Max(3, enemies.Length / 3));
            int first = Mathf.RoundToInt(progress * (enemies.Length - width));
            var candidates = new List<EnemyDataSO>();
            for (int i = 0; i < count; i++)
            {
                if (rank == EnemyRank.Boss) { picked[i] = boss; break; }
                if (candidates.Count == 0)
                    for (int j = first; j < first + width; j++) candidates.Add(enemies[j]);
                int index = random.Next(candidates.Count);
                picked[i] = candidates[index];
                candidates.RemoveAt(index);
            }
            float reward = rank == EnemyRank.Boss ? 3f : rank == EnemyRank.Elite ? 1.5f : 1f;
            return new BattleData
            {
                enemies = picked,
                gold = Mathf.RoundToInt(Mathf.Lerp(Mathf.Max(0, gold.x), Mathf.Max(0, Mathf.Max(gold.x, gold.y)), progress) * reward),
                healthMultiplier = Mathf.Lerp(Mathf.Max(0.1f, healthMultiplier.x), Mathf.Max(0.1f, Mathf.Max(healthMultiplier.x, healthMultiplier.y)), progress),
                dicePower = Mathf.Lerp(Mathf.Max(0.1f, dicePower.x), Mathf.Max(0.1f, Mathf.Max(dicePower.x, dicePower.y)), progress),
                rank = rank
            };
        }
    }

    [Serializable]
    public class BattleData
    {
        public EnemyDataSO[] enemies;
        public int gold;
        public float healthMultiplier, dicePower;
        public EnemyRank rank;
        public bool IsValid => enemies != null && enemies.Length > 0 && Array.TrueForAll(enemies, e => e != null);
    }
}
