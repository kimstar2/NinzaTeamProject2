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
        [Header("제련")]
        [SerializeField, Min(1)] private int forgeBaseCost = 20;
        [SerializeField, Min(0.1f)] private float forgeLevelGain = 0.5f;
        [SerializeField, Min(1)] private float forgeMaxLevel = 3f;
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
        public float GetForgedLevel(float level) => Mathf.Max(level, Mathf.Min(forgeMaxLevel, level + Mathf.Max(0.1f, forgeLevelGain)));

        public bool CanForge(IReadOnlyList<RewardDiceFragmentSO> faces, out string reason)
        {
            reason = "기준 면과 소모할 재료 면 2개를 선택하세요.";
            if (faces == null || faces.Count != 3) return false;
            if (faces[0] != null && faces[0].Level >= forgeMaxLevel)
            {
                reason = $"기준 면이 최대 제련 레벨(Lv.{forgeMaxLevel:0.#})에 도달했습니다.";
                return false;
            }
            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i] == null || faces[i].DiceData == null || !DiceFragments.Contains(faces[i])) return false;
                for (int j = 0; j < i; j++)
                    if (faces[i] == faces[j]) return false;
            }
            int cost = GetForgeCost(faces[0].Level);
            reason = Gold < cost ? $"골드가 {cost - Gold} G 부족합니다." : "재료 면 2개가 소모됩니다. 기준 면의 스킬과 등급은 유지됩니다.";
            return Gold >= cost;
        }

        public bool TryForge(IReadOnlyList<RewardDiceFragmentSO> faces, out string reason)
        {
            if (!CanForge(faces, out reason)) return false;
            var target = faces[0];
            Gold -= GetForgeCost(target.Level);
            target.Initialize(target.DiceData, GetForgedLevel(target.Level));
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
                reward.DiceData == null || reward.DiceData.GetSkillDataStruct(player.AttackType).SkillData == null) return false;
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
