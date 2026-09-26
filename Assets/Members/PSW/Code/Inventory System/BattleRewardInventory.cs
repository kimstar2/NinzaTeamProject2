using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using UnityEngine;

namespace Members.PSW.Code.InventorySystem
{
    // One run's inventory survives scene changes. Re-entering battle keeps the first owner.
    public sealed class BattleRewardInventory : Inventory
    {
        [SerializeField] private EventChannelSO eventChannel;
        private readonly List<RewardDiceFragmentSO> _ownedRewards = new();
        private bool _isOwner;
        private readonly Dictionary<EnemyType, AgentAttackType> _enemyAttackTypes = new();

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
                Debug.LogError("BattleRewardInventory requires an event channel and a scene root object.", this);
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
            else _enemyAttackTypes[evt.EnemyType] = evt.EnemyData.AttackType;
        }

        private void HandleDiceDataBind(OnEnemyDiceDataBind evt)
        {
            if (evt.RollType != EnemyRollType.DeadRoll || evt.DiceData == null) return;

            // The publisher already rejects duplicate dead rolls for the same enemy life.
            // Keep separate entries even when multiple enemies drop the same face.
            var fragment = ScriptableObject.CreateInstance<RewardDiceFragmentSO>();
            fragment.hideFlags = HideFlags.DontSave;
            AgentAttackType? attackType = _enemyAttackTypes.TryGetValue(evt.EnemyType, out var type) ? type : null;
            fragment.Initialize(evt.DiceData, evt.Level, attackType);
            if (AddFragment(fragment))
                _ownedRewards.Add(fragment);
            else
            {
                Destroy(fragment);
                Debug.LogWarning($"Inventory is full ({MaxSlots} slots). The dice reward could not be stored.", this);
            }
        }

        private void OnDestroy()
        {
            if (!_isOwner) return;
            eventChannel.RemoveListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
            eventChannel.RemoveListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
            ServiceLocator.UnRegister<Inventory>(this);
            foreach (var fragment in _ownedRewards)
                if (fragment != null) Destroy(fragment);
            _ownedRewards.Clear();
        }
    }
}
