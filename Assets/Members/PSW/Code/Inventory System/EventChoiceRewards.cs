using System.Collections.Generic;
using System.Collections;
using Members.KJY._01.Scripts.Agent.Player.Dice;
using System.Reflection;
using System;
using DevLib.ServiceLocator;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Events;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Service;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.LYW.Scripts.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;
using EventType = Members.LYW.Scripts.Event.EventType;
using Random = UnityEngine.Random;

namespace Members.PSW.Code.InventorySystem
{
    // PSW-only integration: the existing EventManager has no public selection hook.
    // Only runtime event copies are changed; source events and scenes remain untouched.
    public sealed class EventChoiceRewards : MonoBehaviour
    {
        [SerializeField] private EventDataSO sourceEvent;
        [SerializeField] private List<DiceDataSO> rewardPool = new();
        [SerializeField] private List<HealthChoice> healthChoices = new();
        [SerializeField] private List<GoldChoice> goldChoices = new();
        [SerializeField] private List<DamageChoice> damageChoices = new();
        [SerializeField] private List<BattleChoice> battleChoices = new();
        [SerializeField] private List<RiskChoice> riskChoices = new();
        [SerializeField] private List<BattleRewardChoice> battleRewardChoices = new();
        private readonly EventGoldModifiers _goldModifiers = new();
        private BattleInventory _goldInventory;
        private bool _pendingRerollLock;
        [SerializeField] private string battleScenePath;
        private float _pendingStartRisk, _pendingRerollRisk;
        private readonly Dictionary<PlayerDataSO, float> _originalMaxHealth = new();
        [SerializeField] private EventBattleTransition battleTransition;
        [SerializeField] private EventBuffView buffView;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private string resetScenePath;
        private readonly EventDamageModifiers _damageModifiers = new();
        private bool _hasCompletedBattle;
        private int _completedBattleScene;

        [Serializable]
        private sealed class BattleChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
        }

        [Serializable]
        private sealed class DamageChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public float outgoingPercent;
            public float incomingPercent;
            public int battles = -1;
            public float healthCostRatio;
            public float maxHealthBonusRatio;
        }

        [Serializable]
        private sealed class RiskChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public float startingPercent;
            public float rerollPercent;
        }

        [Serializable]
        private sealed class GoldChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public int gold;
        }

        [Serializable]
        private sealed class BattleRewardChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public float goldPercent;
            public int battles = -1;
            public float maxHealthBonus;
            public bool lockFirstTurn;
        }

        [Serializable]
        private sealed class HealthChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public PlayerType target = PlayerType.All;
            public int health;
        }

        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo EventField = typeof(EventManager).GetField("_eventData", Fields);
        private static readonly FieldInfo ButtonsField = typeof(EventManager).GetField("buttons", Fields);
        private readonly List<Binding> _bindings = new();
        private readonly List<OwnedReward> _rewards = new();
        private readonly List<EventManager> _pendingManagers = new();

        private sealed class Binding
        {
            public EventManager Manager;
            public EventDataSO Source;
            public EventDataSO Data;
            public readonly List<Button> Buttons = new();
            public readonly List<UnityAction> Callbacks = new();
            public bool Claimed;
        }

        private struct OwnedReward
        {
            public Inventory Inventory;
            public RewardDiceFragmentSO Fragment;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var prefab = Resources.Load<EventChoiceRewards>("EventChoiceRewards");
            if (prefab == null)
            {
                Debug.LogError("Event reward configuration is missing: Resources/EventChoiceRewards.prefab");
                return;
            }
            var instance = Instantiate(prefab);
            DontDestroyOnLoad(instance.gameObject);
        }

        private void OnEnable()
        {
            ServiceLocator.Register<IDamageModifiers>(_damageModifiers);
            if (buffView != null) buffView.Bind(_damageModifiers);
            if (buffView != null) buffView.BindRewards(_goldModifiers, null);
            if (eventChannel != null) eventChannel.AddListener<OnBattleResult>(HandleBattleResult);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (_goldInventory != null) _goldInventory.SetVictoryGoldModifier(null);
            if (buffView != null) buffView.BindRewards(null, null);
            ServiceLocator.UnRegister<IDamageModifiers>(_damageModifiers);
            if (buffView != null) buffView.Bind(null);
            if (eventChannel != null) eventChannel.RemoveListener<OnBattleResult>(HandleBattleResult);
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            for (int i = _bindings.Count - 1; i >= 0; i--) ReleaseBinding(i);
            _pendingManagers.Clear();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Current new-game flow always starts from the title scene.
            if (scene.path == resetScenePath)
            {
                _damageModifiers.Clear();
                _goldModifiers.Clear();
                _pendingRerollLock = false;
                _pendingStartRisk = _pendingRerollRisk = 0f;
                foreach (var entry in _originalMaxHealth)
                {
                    if (entry.Key == null) continue;
                    entry.Key.SetMaxHealth(entry.Value);
                    if (entry.Key.CurrentHealth > entry.Value)
                        entry.Key.TakeDamage(entry.Key.CurrentHealth - entry.Value);
                }
                _originalMaxHealth.Clear();
                _hasCompletedBattle = false;
            }
            if (scene.path == battleScenePath) StartCoroutine(ApplyPendingRisk(scene));
            ReleaseUnownedRewards();
            if (sourceEvent == null || EventField == null || ButtonsField == null)
            {
                Debug.LogError("Cannot bind event rewards: check the configuration and EventManager field names.", this);
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
            foreach (var manager in root.GetComponentsInChildren<EventManager>(true))
                if (!_pendingManagers.Contains(manager)) _pendingManagers.Add(manager);
            BindPendingManagers();
        }

        private void Update() => BindPendingManagers();

        private void BindPendingManagers()
        {
            // SC_5-2 starts inactive. Wait for its Awake to select data, then bind
            // before EventManager's delayed StartEvent enables the choice buttons.
            for (int pendingIndex = _pendingManagers.Count - 1; pendingIndex >= 0; pendingIndex--)
            {
                var manager = _pendingManagers[pendingIndex];
                if (manager == null)
                {
                    _pendingManagers.RemoveAt(pendingIndex);
                    continue;
                }
                var source = EventField.GetValue(manager) as EventDataSO;
                if (source == null) continue;
                _pendingManagers.RemoveAt(pendingIndex);
                if (source != sourceEvent && !healthChoices.Exists(effect => effect.eventData == source) &&
                    !goldChoices.Exists(effect => effect.eventData == source) &&
                    !damageChoices.Exists(effect => effect.eventData == source) &&
                    !riskChoices.Exists(effect => effect.eventData == source) &&
                    !battleRewardChoices.Exists(effect => effect.eventData == source) &&
                    !battleChoices.Exists(effect => effect.eventData == source)) continue;
                var buttons = ButtonsField.GetValue(manager) as List<Button>;
                if (buttons == null || buttons.Count < source.choices ||
                    source.resultText.Count < source.choices || source.choiceEvent.Count < source.choices ||
                    buttons.GetRange(0, source.choices).Exists(button => button == null) ||
                    source.choiceEvent.Exists(type => type != EventType.DefaultComplete &&
                        type != EventType.ChangeExImage && type != EventType.MoveNextEvent))
                {
                    Debug.LogError($"Event rewards for '{source.name}' expect valid choice buttons using DefaultComplete or ChangeExImage.", manager);
                    continue;
                }

                var data = Instantiate(source);
                data.hideFlags = HideFlags.DontSave;
                EventField.SetValue(manager, data);
                var binding = new Binding { Manager = manager, Source = source, Data = data };
                for (int i = 0; i < source.choices; i++)
                {
                    int index = i;
                    UnityAction callback = () => Claim(binding, index);
                    buttons[i].onClick.AddListener(callback);
                    binding.Buttons.Add(buttons[i]);
                    binding.Callbacks.Add(callback);
                }
                _bindings.Add(binding);
            }
        }

        private void Claim(Binding binding, int index)
        {
            if (binding.Claimed) return;
            binding.Claimed = true;
            // Keep the original EventDataSO result copy. Reward failures go to the console.
            if (battleChoices.Exists(entry => entry.eventData == binding.Source && entry.choiceIndex == index))
            {
                string failure = "이벤트 전투 연결이 설정되지 않았습니다.";
                if (battleTransition == null || !battleTransition.TryPrepare(binding.Manager.gameObject.scene, out failure))
                    ReportRewardFailure(failure);
                return;
            }
            if (binding.Source == sourceEvent && index == 0)
            {
                ReportRewardFailure(GrantReward());
                return;
            }
            var battleReward = battleRewardChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
            if (battleReward != null)
            {
                ReportRewardFailure(ApplyBattleReward(battleReward));
                return;
            }
            var risk = riskChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
            if (risk != null)
            {
                if (!ServiceLocator.TryGet<IBattleDataStorage>(out var storage) || storage.Instance == null)
                {
                    ReportRewardFailure("모험 정보가 없어 위험도 효과를 적용하지 못했습니다.");
                    return;
                }
                _pendingStartRisk = Mathf.Clamp(_pendingStartRisk + risk.startingPercent, 0f, 100f);
                _pendingRerollRisk = Mathf.Clamp(_pendingRerollRisk + risk.rerollPercent, 0f, 100f);
            }
            var health = healthChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
            var gold = goldChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
            var damage = damageChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
            BattleInventory inventory = null;
            if (gold != null)
            {
                ServiceLocator.TryGet<Inventory>(out var registered);
                inventory = registered as BattleInventory;
                if (inventory == null)
                {
                    ReportRewardFailure("골드 인벤토리가 없어 보상을 적용하지 못했습니다.");
                    return;
                }
            }
            if (health != null)
            {
                ReportRewardFailure(ApplyHealthChoice(health, out bool applied));
                if (!applied) return;
            }
            if (gold != null) ApplyGoldChoice(gold, inventory);
            if (damage != null) ReportRewardFailure(ApplyDamageChoice(damage));
        }

        private void ReportRewardFailure(string message)
        {
            if (!string.IsNullOrEmpty(message)) Debug.LogWarning(message, this);
        }

        private string ApplyDamageChoice(DamageChoice effect)
        {
            if (eventChannel == null || !ServiceLocator.TryGet<IBattleDataStorage>(out var storage) || storage.Instance == null)
                return "모험 정보가 없어 피해 배율 효과를 적용하지 못했습니다.";
            if (!_damageModifiers.TryAdd(effect.outgoingPercent, effect.incomingPercent, effect.battles))
                return "피해 배율 설정이 올바르지 않아 효과를 적용하지 못했습니다.";
            if (effect.healthCostRatio > 0f || effect.maxHealthBonusRatio > 0f)
            {
                foreach (var player in storage.Instance.GetRunTimePlayerData())
                {
                    if (player == null || player.IsDead) continue;
                    float previousMax = player.MaxHealth;
                    if (!_originalMaxHealth.ContainsKey(player)) _originalMaxHealth.Add(player, previousMax);
                    player.TakeDamage(Mathf.Min(player.CurrentHealth, previousMax * effect.healthCostRatio));
                    player.SetMaxHealth(previousMax * (1f + effect.maxHealthBonusRatio));
                }
            }
            return null;
        }

        private IEnumerator ApplyPendingRisk(Scene scene)
        {
            // Wait for the roll manager and risk HUD Start methods to initialize.
            yield return null;
            if (!scene.IsValid() || !scene.isLoaded) yield break;
            ConnectGoldInventory();
            foreach (var root in scene.GetRootGameObjects())
            {
                var manager = root.GetComponentInChildren<PlayerDiceRollManager>();
                if (manager == null) continue;
                if (_pendingRerollLock) { manager.LockFirstTurnReroll(); _pendingRerollLock = false; }
                if (buffView != null) buffView.BindRewards(_goldModifiers, manager);
                if (!manager.ApplyEventRisk(_pendingStartRisk, _pendingRerollRisk)) continue;
                _pendingStartRisk = _pendingRerollRisk = 0f;
                yield break;
            }
            Debug.LogWarning("전투의 주사위 관리자를 찾지 못해 이벤트 위험도 적용을 보류했습니다.", this);
        }

        private void HandleBattleResult(OnBattleResult result)
        {
            if (_hasCompletedBattle) return;
            _hasCompletedBattle = true;
            _completedBattleScene = SceneManager.GetActiveScene().handle;
        }

        private bool ConnectGoldInventory()
        {
            if (!ServiceLocator.TryGet<Inventory>(out var inventory) || inventory is not BattleInventory battleInventory)
                return false;
            _goldInventory = battleInventory;
            _goldInventory.SetVictoryGoldModifier(_goldModifiers.Apply);
            return true;
        }

        private string ApplyBattleReward(BattleRewardChoice choice)
        {
            if (!ServiceLocator.TryGet<IBattleDataStorage>(out var storage) || storage.Instance == null ||
                (choice.goldPercent != 0f && !ConnectGoldInventory()))
                return "모험 정보 또는 인벤토리가 없어 이벤트 효과를 적용하지 못했습니다.";
            if (choice.goldPercent != 0f) _goldModifiers.Add(choice.goldPercent, choice.battles);
            if (choice.lockFirstTurn) _pendingRerollLock = true;
            if (choice.maxHealthBonus > 0f)
                foreach (var player in storage.Instance.GetRunTimePlayerData())
                {
                    if (player == null) continue;
                    if (!_originalMaxHealth.ContainsKey(player)) _originalMaxHealth.Add(player, player.MaxHealth);
                    player.SetMaxHealth(player.MaxHealth + choice.maxHealthBonus);
                }
            return null;
        }

        private static void ApplyGoldChoice(GoldChoice effect, BattleInventory inventory)
        {
            if (effect.gold >= 0) inventory.AddGold(effect.gold);
            else inventory.RemoveGold(effect.gold == int.MinValue ? int.MaxValue : -effect.gold);
        }

        private string ApplyHealthChoice(HealthChoice effect, out bool applied)
        {
            applied = false;
            if (!ServiceLocator.TryGet<IBattleDataStorage>(out var service) || service.Instance == null)
                return "파티 정보가 없어 체력 변화를 적용하지 못했습니다. 모험을 시작한 뒤 이용해 주세요.";

            // Event scenes have no combat HealthModule. Persist HP in the run data;
            // PlayerSelector.Init restores it into HealthModule on the next battle.
            var storage = service.Instance;
            var targets = effect.target == PlayerType.All
                ? storage.GetRunTimePlayerData()
                : new[] { storage.GetRunTimePlayerData(effect.target) };
            bool found = false;
            foreach (var player in targets)
            {
                if (player == null) continue;
                found = true;
                if (player.IsDead) continue;
                if (effect.health > 0) player.Heal(effect.health);
                else if (effect.health < 0)
                    player.TakeDamage(Mathf.Min(-(float)effect.health, player.CurrentHealth));
            }
            applied = found;
            return found ? null : "체력을 변경할 대상이 없어 효과를 적용하지 못했습니다.";
        }

        private string GrantReward()
        {
            if (!ServiceLocator.TryGet<Inventory>(out var inventory) || inventory == null)
                return "인벤토리가 없어 주사위 면을 받을 수 없습니다. 모험을 시작한 뒤 이용해 주세요.";
            if (inventory.DiceFragments.Count >= inventory.MaxSlots)
                return "인벤토리가 가득 차 주사위 면을 받지 못했습니다.";

            var candidates = new List<DiceDataSO>();
            foreach (var face in rewardPool)
            {
                if (face == null || candidates.Contains(face) || face.SkillDataStructs == null ||
                    !face.SkillDataStructs.Exists(entry => entry.SkillData != null)) continue;
                candidates.Add(face);
            }
            if (candidates.Count == 0)
            {
                Debug.LogError("SC_2 reward pool has no usable dice faces.", this);
                return "보상 주사위 면이 설정되지 않아 보상을 받지 못했습니다.";
            }

            var faceData = candidates[Random.Range(0, candidates.Count)];
            var reward = ScriptableObject.CreateInstance<RewardDiceFragmentSO>();
            reward.hideFlags = HideFlags.DontSave;
            reward.Initialize(faceData, 1f);
            if (!inventory.AddFragment(reward))
            {
                Destroy(reward);
                return "인벤토리에 주사위 면을 추가하지 못했습니다.";
            }
            // BattleInventory owns its battle drops only; this integration owns event drops.
            _rewards.Add(new OwnedReward { Inventory = inventory, Fragment = reward });
            return null;
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            // Keep modifiers through the finishing hit/death effects. Count once when
            // leaving a completed battle; abandoning a scene without a result costs no duration.
            if (_hasCompletedBattle && scene.handle == _completedBattleScene)
            {
                _damageModifiers.CompleteBattle();
                _goldModifiers.CompleteBattle();
                if (buffView != null) buffView.BindRewards(_goldModifiers, null);
                _hasCompletedBattle = false;
            }
            _pendingManagers.RemoveAll(manager => manager == null || manager.gameObject.scene == scene);
            for (int i = _bindings.Count - 1; i >= 0; i--)
                if (_bindings[i].Manager == null || _bindings[i].Manager.gameObject.scene == scene)
                    ReleaseBinding(i);
            ReleaseUnownedRewards();
        }

        private void ReleaseBinding(int index)
        {
            var binding = _bindings[index];
            for (int i = 0; i < binding.Buttons.Count; i++)
                if (binding.Buttons[i] != null) binding.Buttons[i].onClick.RemoveListener(binding.Callbacks[i]);
            if (binding.Manager != null && EventField.GetValue(binding.Manager) as EventDataSO == binding.Data)
                EventField.SetValue(binding.Manager, binding.Source);
            if (binding.Data != null) Destroy(binding.Data);
            _bindings.RemoveAt(index);
        }

        private void ReleaseUnownedRewards()
        {
            for (int i = _rewards.Count - 1; i >= 0; i--)
            {
                var owned = _rewards[i];
                if (owned.Fragment != null && owned.Inventory != null &&
                    owned.Inventory.DiceFragments.Contains(owned.Fragment)) continue;
                if (owned.Fragment != null) Destroy(owned.Fragment);
                _rewards.RemoveAt(i);
            }
        }

        private void OnDestroy()
        {
            foreach (var owned in _rewards)
                if (owned.Fragment != null) Destroy(owned.Fragment);
            _rewards.Clear();
        }
    }
}
