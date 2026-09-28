using System.Collections.Generic;
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
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private string resetScenePath;
        private readonly EventDamageModifiers _damageModifiers = new();
        private bool _hasCompletedBattle;
        private int _completedBattleScene;

        [Serializable]
        private sealed class DamageChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public float outgoingPercent;
            public float incomingPercent;
            public int battles = -1;
            [TextArea] public string resultText;
        }

        [Serializable]
        private sealed class GoldChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public int gold;
        }

        [Serializable]
        private sealed class HealthChoice
        {
            public EventDataSO eventData;
            public int choiceIndex;
            public PlayerType target = PlayerType.All;
            public int health;
            [TextArea] public string resultText;
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
            if (eventChannel != null) eventChannel.AddListener<OnBattleResult>(HandleBattleResult);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private void OnDisable()
        {
            ServiceLocator.UnRegister<IDamageModifiers>(_damageModifiers);
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
                _hasCompletedBattle = false;
            }
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
                    !damageChoices.Exists(effect => effect.eventData == source)) continue;
                var buttons = ButtonsField.GetValue(manager) as List<Button>;
                if (buttons == null || buttons.Count < source.choices ||
                    source.resultText.Count < source.choices || source.choiceEvent.Count < source.choices ||
                    buttons.GetRange(0, source.choices).Exists(button => button == null) ||
                    source.choiceEvent.Exists(type => type != EventType.DefaultComplete &&
                        type != EventType.ChangeExImage))
                {
                    Debug.LogError($"Event rewards for '{source.name}' expect valid choice buttons using DefaultComplete or ChangeExImage.", manager);
                    continue;
                }

                var data = Instantiate(source);
                data.hideFlags = HideFlags.DontSave;
                if (source == sourceEvent) data.resultText[0] = "보상으로 무작위 주사위 면 1개를 획득합니다.";
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
            // The existing completion listener reads this text after this callback returns.
            if (binding.Source == sourceEvent && index == 0)
                binding.Data.resultText[index] = GrantReward();
            else
            {
                var effect = healthChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
                var gold = goldChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
                var damage = damageChoices.Find(entry => entry.eventData == binding.Source && entry.choiceIndex == index);
                BattleInventory inventory = null;
                if (gold != null)
                {
                    ServiceLocator.TryGet<Inventory>(out var registered);
                    inventory = registered as BattleInventory;
                    if (inventory == null)
                    {
                        binding.Data.resultText[index] = "골드 인벤토리가 없어 보상을 적용하지 못했습니다. 모험을 시작한 뒤 이용해 주세요.";
                        return;
                    }
                }
                string result = null;
                if (effect != null)
                {
                    result = ApplyHealthChoice(effect, out bool applied);
                    binding.Data.resultText[index] = result;
                    if (!applied) return;
                }
                if (gold != null)
                {
                    string goldResult = ApplyGoldChoice(gold, inventory);
                    result = result == null ? goldResult : result + "\n" + goldResult;
                    binding.Data.resultText[index] = result;
                }
                if (damage != null)
                {
                    string damageResult = ApplyDamageChoice(damage);
                    binding.Data.resultText[index] = result == null ? damageResult : result + "\n" + damageResult;
                }
            }
        }

        private string ApplyDamageChoice(DamageChoice effect)
        {
            if (eventChannel == null || !ServiceLocator.TryGet<IBattleDataStorage>(out var storage) || storage.Instance == null)
                return "모험 정보가 없어 피해 배율 효과를 적용하지 못했습니다.";
            return _damageModifiers.TryAdd(effect.outgoingPercent, effect.incomingPercent, effect.battles)
                ? effect.resultText : "피해 배율 설정이 올바르지 않아 효과를 적용하지 못했습니다.";
        }

        private void HandleBattleResult(OnBattleResult result)
        {
            if (_hasCompletedBattle) return;
            _hasCompletedBattle = true;
            _completedBattleScene = SceneManager.GetActiveScene().handle;
        }

        private static string ApplyGoldChoice(GoldChoice effect, BattleInventory inventory)
        {
            if (effect.gold >= 0)
            {
                int added = inventory.AddGold(effect.gold);
                return $"{added}골드를 획득했습니다. (보유 {inventory.Gold} G)";
            }
            int amount = effect.gold == int.MinValue ? int.MaxValue : -effect.gold;
            int removed = inventory.RemoveGold(amount);
            return removed < amount
                ? $"보유 골드가 부족하여 {removed}골드만 잃었습니다. (보유 {inventory.Gold} G)"
                : $"{removed}골드를 잃었습니다. (보유 {inventory.Gold} G)";
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
            return found ? effect.resultText : "체력을 변경할 대상이 없어 효과를 적용하지 못했습니다.";
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
            return $"보상으로 주사위 면 [{faceData.MainName}] 1개를 획득했습니다! (Lv.1)";
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            // Keep modifiers through the finishing hit/death effects. Count once when
            // leaving a completed battle; abandoning a scene without a result costs no duration.
            if (_hasCompletedBattle && scene.handle == _completedBattleScene)
            {
                _damageModifiers.CompleteBattle();
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
