using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;
using Members.PSW.Code.InventorySystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    // 슬롯과 주사위 목록은 공용 Inventory를 사용하고, 전투 보상과 재화는 여기서 관리한다.
    public sealed class BattleInventory : Inventory
    {
        [SerializeField] private EventChannelSO eventChannel;
        [Header("재련")]
        [SerializeField, Min(1)] private int forgeBaseCost = 20;
        [Tooltip("재료 2개가 모두 기준 면보다 낮은 등급일 때 오르는 레벨")]
        [SerializeField, Min(0.1f)] private float forgeLevelGain = 0.5f;
        [Tooltip("재료 등급이 섞였거나, 등급 업 조건인데 더 올릴 등급이 없을 때 오르는 레벨")]
        [SerializeField, Min(0.1f)] private float mixedLevelGain = 1f;
        [SerializeField, Min(1)] private float forgeMaxLevel = 3f;
        [Header("등급 업")]
        [Tooltip("등급 업 비용 = 재련 비용 × 이 값")]
        [SerializeField, Min(1f)] private float gradeUpCostMultiplier = 1.35f;
        [Tooltip("등급 업 결과로 나올 수 있는 면 목록")]
        [SerializeField] private DiceFacePoolSO gradeUpPool;
        [Tooltip("재료 강함도를 결과에 반영하는 폭. 작을수록 재료와 비슷한 강함도의 스킬만 나온다")]
        [SerializeField, Range(0.05f, 1f)] private float gradeUpSpread = 0.3f;
        private readonly List<DiceDataSO> _candidates = new();
        private readonly List<float> _weights = new();
        private readonly List<RewardDiceFragmentSO> _ownedRewards = new();
        private readonly List<RewardDiceFragmentSO> _pendingRewards = new();
        private readonly List<RewardDiceFragmentSO> _encounterRewards = new();
        private readonly HashSet<EnemyType> _rewardedEnemies = new();
        private bool _isOwner;
        private bool _isCollecting;

        public IReadOnlyList<RewardDiceFragmentSO> EncounterRewards => _encounterRewards;
        public int Gold { get; private set; }
        public int GoldEarned { get; private set; }
        public int SkippedRewards { get; private set; }

        // Returns the amount actually added. Non-positive amounts do nothing.
        public int AddGold(int amount)
        {
            if (amount <= 0) return 0;
            int added = Mathf.Min(amount, int.MaxValue - Gold);
            if (added == 0) return 0;
            Gold += added;
            NotifyChanged();
            return added;
        }

        // Event losses stop at zero; returns the amount actually removed.
        public int RemoveGold(int amount)
        {
            if (amount <= 0) return 0;
            int removed = Mathf.Min(amount, Gold);
            if (removed == 0) return 0;
            Gold -= removed;
            NotifyChanged();
            return removed;
        }

        public int GetForgeCost(float level) => Mathf.CeilToInt(Mathf.Max(1, forgeBaseCost) * Mathf.Max(1, level));

        public enum ForgeMode { None, Enhance, GradeUp }

        public readonly struct ForgePlan
        {
            public readonly ForgeMode Mode;
            public readonly int Cost;
            public readonly float ResultLevel;
            public readonly DiceGrade ResultGrade;
            public readonly string Message;

            public ForgePlan(ForgeMode mode, int cost, float resultLevel, DiceGrade resultGrade, string message)
            {
                Mode = mode;
                Cost = cost;
                ResultLevel = resultLevel;
                ResultGrade = resultGrade;
                Message = message;
            }

            public static ForgePlan Invalid(string message) => new(ForgeMode.None, 0, 0f, DiceGrade.Common, message);
        }

        private static DiceGrade GradeOf(DiceDataSO face) =>
            face != null && face.DiceGrade != null ? face.DiceGrade.Grade : DiceGrade.Common;

        // 낮음 2: 강화 / 섞임: 크게 강화 / 같거나 높음: 등급 업
        public ForgePlan GetForgePlan(IReadOnlyList<RewardDiceFragmentSO> faces)
        {
            const string selectMessage = "기준 면과 소모할 재료 면 2개를 선택하세요.";
            if (faces == null || faces.Count != 3) return ForgePlan.Invalid(selectMessage);
            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i] == null || faces[i].DiceData == null || !DiceFragments.Contains(faces[i]))
                    return ForgePlan.Invalid(selectMessage);
                for (int j = 0; j < i; j++)
                    if (faces[i] == faces[j]) return ForgePlan.Invalid(selectMessage);
            }

            var target = faces[0];
            DiceGrade baseGrade = GradeOf(target.DiceData);
            int lowerCount = 0;
            for (int i = 1; i < faces.Count; i++)
                if (GradeOf(faces[i].DiceData) < baseGrade) lowerCount++;

            if (lowerCount == 2) return Enhance(target, forgeLevelGain, "재료 면 2개가 소모되고 레벨이 오릅니다.");
            if (lowerCount == 1) return Enhance(target, mixedLevelGain, "재료 등급이 섞여 있어 레벨이 더 많이 오릅니다.");
            if (baseGrade == DiceGrade.Rare) return Enhance(target, mixedLevelGain, "전설 면은 더 올릴 등급이 없어 레벨이 오릅니다.");

            DiceGrade nextGrade = baseGrade + 1;
            if (GetGradeUpCandidates(target.DiceData, nextGrade).Count == 0)
                return Enhance(target, mixedLevelGain, "올라갈 수 있는 상위 등급 스킬이 없어 레벨이 오릅니다.");
            int cost = Mathf.CeilToInt(GetForgeCost(target.Level) * gradeUpCostMultiplier);
            return new ForgePlan(ForgeMode.GradeUp, cost, 1f, nextGrade,
                $"재료 면 2개가 소모되고 {DiceGradeSO.GetName(nextGrade)} 등급 스킬로 바뀝니다.");
        }

        private ForgePlan Enhance(RewardDiceFragmentSO target, float gain, string message)
        {
            if (target.Level >= forgeMaxLevel)
                return ForgePlan.Invalid($"기준 면이 최대 재련 레벨(Lv.{forgeMaxLevel:0.#})에 도달했습니다.");
            float level = Mathf.Min(forgeMaxLevel, target.Level + Mathf.Max(0.1f, gain));
            return new ForgePlan(ForgeMode.Enhance, GetForgeCost(target.Level), level, GradeOf(target.DiceData),
                $"{message} (Lv.{target.Level:0.#} → Lv.{level:0.#})");
        }

        private List<DiceDataSO> GetGradeUpCandidates(DiceDataSO baseFace, DiceGrade grade)
        {
            _candidates.Clear();
            if (gradeUpPool == null) return _candidates;
            var baseTypes = baseFace.GetUsableTypes();
            for (int pass = 0; pass < 2 && _candidates.Count == 0; pass++)
            {
                foreach (var face in gradeUpPool.Faces)
                {
                    if (face == null || face == baseFace || GradeOf(face) != grade) continue;
                    var types = face.GetUsableTypes();
                    if (types.Count == 0) continue;
                    bool keep = pass == 0 ? baseTypes.TrueForAll(types.Contains) : baseTypes.Exists(types.Contains);
                    if (keep) _candidates.Add(face);
                }
            }
            return _candidates;
        }

        private DiceDataSO PickGradeUpResult(IReadOnlyList<RewardDiceFragmentSO> faces, DiceGrade grade)
        {
            var candidates = GetGradeUpCandidates(faces[0].DiceData, grade);
            if (candidates.Count == 0) return null;

            DiceGrade baseGrade = GradeOf(faces[0].DiceData);
            float score = 0f;
            for (int i = 1; i < faces.Count; i++)
            {
                var material = faces[i].DiceData;
                score += GradeOf(material) > baseGrade ? 1f : GetStrengthPercentile(material.Strength, GradeOf(material));
            }
            score /= faces.Count - 1;

            int min = int.MaxValue, max = int.MinValue;
            foreach (var face in candidates)
            {
                min = Mathf.Min(min, face.Strength);
                max = Mathf.Max(max, face.Strength);
            }

            float total = 0f;
            _weights.Clear();
            foreach (var face in candidates)
            {
                float percentile = max > min ? (face.Strength - min) / (float)(max - min) : 0.5f;
                float diff = (percentile - score) / Mathf.Max(0.05f, gradeUpSpread);
                float weight = Mathf.Exp(-diff * diff) + 0.02f;
                _weights.Add(weight);
                total += weight;
            }

            float pick = Random.value * total;
            for (int i = 0; i < candidates.Count; i++)
            {
                pick -= _weights[i];
                if (pick < 0f) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }

        private float GetStrengthPercentile(int strength, DiceGrade grade)
        {
            int min = int.MaxValue, max = int.MinValue;
            if (gradeUpPool != null)
                foreach (var face in gradeUpPool.Faces)
                {
                    if (face == null || GradeOf(face) != grade) continue;
                    min = Mathf.Min(min, face.Strength);
                    max = Mathf.Max(max, face.Strength);
                }
            return min < max ? Mathf.Clamp01((strength - min) / (float)(max - min)) : 0.5f;
        }

        public bool CanForge(IReadOnlyList<RewardDiceFragmentSO> faces, out string reason)
        {
            var plan = GetForgePlan(faces);
            reason = plan.Message;
            if (plan.Mode == ForgeMode.None) return false;
            if (Gold >= plan.Cost) return true;
            reason = $"골드가 {plan.Cost - Gold} G 부족합니다.";
            return false;
        }

        public bool TryForge(IReadOnlyList<RewardDiceFragmentSO> faces, out string reason)
        {
            if (!CanForge(faces, out reason)) return false;
            var plan = GetForgePlan(faces);
            var target = faces[0];
            var resultFace = target.DiceData;
            if (plan.Mode == ForgeMode.GradeUp)
            {
                resultFace = PickGradeUpResult(faces, plan.ResultGrade);
                if (resultFace == null)
                {
                    reason = "올라갈 수 있는 상위 등급 스킬이 없습니다.";
                    return false;
                }
            }
            Gold -= plan.Cost;
            target.Initialize(resultFace, plan.ResultLevel);
            DiceCatalogProgress.Discover(resultFace);
            for (int i = 1; i < faces.Count; i++)
            {
                DiceFragments.Remove(faces[i]);
                _encounterRewards.Remove(faces[i]);
                if (_ownedRewards.Remove(faces[i])) Destroy(faces[i]);
            }
            // 골드와 재료가 모두 정산된 상태만 UI에 알린다.
            NotifyChanged();
            return true;
        }

        public bool EquipFace(PlayerDataSO player, DiceFaceType slot, RewardDiceFragmentSO reward)
        {
            if (player == null || player.DiceList == null || reward == null || !DiceFragments.Contains(reward) ||
                reward.DiceData == null || !reward.DiceData.CanUse(player.AttackType)) return false;
            var previous = player.DiceList.GetDiceData(slot);
            float previousLevel = player.DiceList.GetLevel(slot);
            player.DiceList.SetDiceData(reward.DiceData, slot, reward.Level);
            RemoveFragment(reward);
            if (previous != null)
            {
                reward.Initialize(previous, previousLevel);
                AddFragment(reward);
            }
            else Destroy(reward);
            return true;
        }

        public void ResetRun()
        {
            ReleasePendingRewards();
            foreach (var reward in new List<RewardDiceFragmentSO>(DiceFragments)) RemoveFragment(reward);
            foreach (var reward in _ownedRewards) if (reward != null) Destroy(reward);
            _ownedRewards.Clear();
            _encounterRewards.Clear();
            Gold = GoldEarned = 0;
        }

        public void BeginEncounter()
        {
            ReleasePendingRewards();
            _encounterRewards.Clear();
            _rewardedEnemies.Clear();
            GoldEarned = 0;
            SkippedRewards = 0;
            _isCollecting = true;
        }

        // 마지막 사망 주사위가 정해진 뒤 한 번만 정산한다.
        public void CompleteEncounter(bool victory, int gold)
        {
            if (!_isCollecting) return;
            _isCollecting = false;
            if (!victory)
            {
                ReleasePendingRewards();
                return;
            }

            foreach (var fragment in _pendingRewards)
            {
                if (AddFragment(fragment))
                {
                    _ownedRewards.Add(fragment);
                    _encounterRewards.Add(fragment);
                }
                else
                {
                    SkippedRewards++;
                    Destroy(fragment);
                }
            }
            _pendingRewards.Clear();
            GoldEarned = Mathf.Max(0, gold);
            Gold += GoldEarned;
        }

        private void Awake()
        {
            if (ServiceLocator.TryGet<Inventory>(out var existing) && existing != null && existing != this)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            if (eventChannel == null || transform.parent != null)
            {
                Debug.LogError("BattleInventory requires an event channel and a scene root object.", this);
                enabled = false;
                return;
            }

            _isOwner = true;
            ServiceLocator.Register<Inventory>(this);
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (!_isOwner) return;
            eventChannel.AddListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
            eventChannel.AddListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
        }

        private void OnDisable()
        {
            if (!_isOwner) return;
            eventChannel.RemoveListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
            eventChannel.RemoveListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
        }

        private void HandleEnemyDataChanged(OnEnemyDataChanged evt)
        {
            if (evt.EnemyData != null) _rewardedEnemies.Remove(evt.EnemyType);
        }

        private void HandleDiceDataBind(OnEnemyDiceDataBind evt)
        {
            if (!_isCollecting || evt.RollType != EnemyRollType.DeadRoll || evt.DiceData == null ||
                !_rewardedEnemies.Add(evt.EnemyType)) return;

            // 서로 다른 적이 같은 면을 떨어뜨리더라도 각각 하나의 보상으로 보관한다.
            var fragment = ScriptableObject.CreateInstance<RewardDiceFragmentSO>();
            fragment.hideFlags = HideFlags.DontSave;
            fragment.Initialize(evt.DiceData, evt.Level);
            _pendingRewards.Add(fragment);
        }

        private void ReleasePendingRewards()
        {
            foreach (var fragment in _pendingRewards)
                if (fragment != null) Destroy(fragment);
            _pendingRewards.Clear();
        }

        private void OnDestroy()
        {
            if (!_isOwner) return;
            eventChannel.RemoveListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
            eventChannel.RemoveListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
            ServiceLocator.UnRegister<Inventory>(this);
            ReleasePendingRewards();
            foreach (var fragment in _ownedRewards)
                if (fragment != null) Destroy(fragment);
            _ownedRewards.Clear();
        }
    }
}
