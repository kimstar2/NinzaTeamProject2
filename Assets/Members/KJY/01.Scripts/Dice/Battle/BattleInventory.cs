using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.PSW.Code.InventorySystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    // 슬롯과 주사위 목록은 공용 Inventory를 사용하고, 전투 보상과 재화는 여기서 관리한다.
    public sealed class BattleInventory : Inventory
    {
        [SerializeField] private EventChannelSO eventChannel;
        private readonly List<RewardDiceFragmentSO> _ownedRewards = new();
        private readonly List<RewardDiceFragmentSO> _pendingRewards = new();
        private readonly List<RewardDiceFragmentSO> _encounterRewards = new();
        private readonly HashSet<EnemyType> _rewardedEnemies = new();
        private bool _isOwner;
        private bool _isCollecting;
        private readonly Dictionary<EnemyType, AgentAttackType> _enemyAttackTypes = new();

        public IReadOnlyList<RewardDiceFragmentSO> EncounterRewards => _encounterRewards;
        public int Gold { get; private set; }
        public int GoldEarned { get; private set; }
        public int SkippedRewards { get; private set; }

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
            _enemyAttackTypes.Clear();
        }

        private void HandleEnemyDataChanged(OnEnemyDataChanged evt)
        {
            if (evt.EnemyData == null) _enemyAttackTypes.Remove(evt.EnemyType);
            else
            {
                _enemyAttackTypes[evt.EnemyType] = evt.EnemyData.AttackType;
                _rewardedEnemies.Remove(evt.EnemyType);
            }
        }

        private void HandleDiceDataBind(OnEnemyDiceDataBind evt)
        {
            if (!_isCollecting || evt.RollType != EnemyRollType.DeadRoll || evt.DiceData == null ||
                !_rewardedEnemies.Add(evt.EnemyType)) return;

            // 서로 다른 적이 같은 면을 떨어뜨리더라도 각각 하나의 보상으로 보관한다.
            var fragment = ScriptableObject.CreateInstance<RewardDiceFragmentSO>();
            fragment.hideFlags = HideFlags.DontSave;
            AgentAttackType? attackType = _enemyAttackTypes.TryGetValue(evt.EnemyType, out var type) ? type : null;
            fragment.Initialize(evt.DiceData, evt.Level, attackType);
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
