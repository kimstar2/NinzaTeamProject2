using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Events;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Service;
using Members.KJY._01.Scripts.UI;
using UnityEngine;
using UnityEngine.Events;
using ZLinq;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    [Serializable]
    public struct EnemyStruct
    {
        [field: SerializeField] public EnemySelector EnemySelector { get; private set; }
        [field: SerializeField] public EnemyType EnemyType { get; private set; }
    }

    public class RoundMaker : MonoBehaviour
    {
        private const int MaxUnitCount = 4;

        [SerializeField] private PlayerSelector tanker, dealer, healer, mage;
        [SerializeField] private List<EnemyStruct> eList;
        [SerializeField] private DiceBattleManager diceBattleManager;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private TweenLayoutGroup enemyLayoutGroup, enemyAgentLayoutGroup;
        [SerializeField, Min(0f)] private float reinforcementDelay = 0.75f;
        private readonly List<EnemyDataSO> onBattleList = new();
        private readonly List<EnemyDataSO> readyBattlesList = new();
        private bool _resultSent;
        private BattleDataStorage _dataStorage;
        private Vector3[] _enemyPositions;
        private bool _hasRound;
        private bool _isMakingRound;
        private CancellationTokenSource _cts;
        public bool IsRoundClear { get; private set; }
        public UnityEvent onRoundClear;
        
        private void Awake()
        {
            _enemyPositions = new Vector3[eList.Count];
            for (int i = 0; i < eList.Count; i++)
                _enemyPositions[i] = eList[i].EnemySelector.DefaultPosition.position;

            _dataStorage = ServiceLocator.Get<IBattleDataStorage>().Instance;
        }

        private void Start()
        {
            MakeRound(_dataStorage.GetRunTimePlayerData(),_dataStorage.GetBattleData().enemyDataList.ToArray());
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnEndBattle>(HandleEndBattle);
            eventChannel.AddListener<OnPlayerDead>(HandlePlayerDead);
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
            enemyLayoutGroup.OnRemoved += HandleRemoved;
            enemyAgentLayoutGroup.OnRemoved += HandleRemoved;
            // ReMake();
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEndBattle>(HandleEndBattle);
            eventChannel.RemoveListener<OnPlayerDead>(HandlePlayerDead);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
            enemyLayoutGroup.OnRemoved -= HandleRemoved;
            enemyAgentLayoutGroup.OnRemoved -= HandleRemoved;
            KillTask();
        }

        private void KillTask()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                
            }
        }

        public void MakeRound(PlayerDataSO[] players, EnemyDataSO[] enemies)
        {
            if (diceBattleManager.IsBattle)
                return;
            if (!ValidateRound(players, enemies)) return;

            _enemiesCount = enemies.Length;
            _playerCount = Array.FindAll(players, player => !player.IsDead).Length;
            _resultSent = false;
            
            KillTask();
            _isMakingRound = true;
            _hasRound = true;
            IsRoundClear = false;
            diceBattleManager.ClearSelection();
            readyBattlesList.Clear();
            onBattleList.Clear();

            foreach (EnemyStruct entry in eList)
            {
                entry.EnemySelector.HideFromBattle();
                entry.EnemySelector.ClearData();
            }

            PlayerSelector[] selectors = { tanker, dealer, healer, mage };
            for (int i = 0; i < selectors.Length; i++)
            {
                if (selectors[i] != null) selectors[i].HideFromBattle();
            }
            
            
            foreach (PlayerDataSO player in players.AsValueEnumerable().OrderBy(s => s.Cost))
            {
                GetPlayerSelector(player.PlayerType).Init(player);
            }

            readyBattlesList.AddRange(enemies.AsValueEnumerable().OrderBy(s => s.Cost).ToArray());
            _isMakingRound = false;
            FillEmptySlots();
            CheckRoundClear();
        }

        private void HandleRemoved(Transform target) => ReMake();
        private void HandleEndBattle(OnEndBattle evt) => ReMake();

        [ContextMenu("ReMake")]
        public void ReMake()
        {
            if (!_hasRound || _isMakingRound || diceBattleManager.IsBattle) return;

            foreach (EnemyStruct entry in eList)
            {
                EnemySelector selector = entry.EnemySelector;
                if (selector.IsNoneData || !selector.IsDead) continue;
                if (!enemyLayoutGroup.IsRemoved(selector.transform) ||
                    !enemyAgentLayoutGroup.IsRemoved(selector.DiceLayoutTarget)) continue;

                onBattleList.Remove(selector.RuntimeEnemyData);
                selector.HideFromBattle();
                selector.ClearData();
            }

            UpdateEnemyPositions();

            KillTask();
            
            _cts =  new CancellationTokenSource();
            
            if (!_resultSent && readyBattlesList.Count > 0 && onBattleList.Count < MaxUnitCount)
                WaitForReinforcement(_cts.Token).Forget();

            CheckRoundClear();
        }

        private async UniTask WaitForReinforcement(CancellationToken ct)
        {
            try
            {
                await UniTask.WaitUntil(() => !diceBattleManager.IsBattle &&
                    !enemyLayoutGroup.IsTransitioning && !enemyAgentLayoutGroup.IsTransitioning, cancellationToken: ct);
                if (reinforcementDelay > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(reinforcementDelay), cancellationToken: ct);
                if (!_hasRound || _resultSent || !isActiveAndEnabled || diceBattleManager.IsBattle) return;
                FillEmptySlots();
                CheckRoundClear();
                await UniTask.WaitUntil(() => !enemyLayoutGroup.IsTransitioning &&
                    !enemyAgentLayoutGroup.IsTransitioning, cancellationToken: ct);
                eventChannel.RaiseEvent(new OnEnemyRollRaise(true));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        }

        private void FillEmptySlots()
        {
            foreach (EnemyStruct entry in eList)
            {
                if (readyBattlesList.Count == 0 || onBattleList.Count >= MaxUnitCount) break;
                EnemySelector selector = entry.EnemySelector;
                if (!selector.IsNoneData) continue;

                EnemyDataSO enemy = readyBattlesList[0];
                readyBattlesList.RemoveAt(0);
                onBattleList.Add(enemy);
                selector.DefaultPosition.position = _enemyPositions[enemyLayoutGroup.ActiveCount];
                selector.EnemyDataChanged(enemy);
                selector.Init();
            }

            UpdateEnemyPositions();
        }

        private int _playerCount = 0, _enemiesCount = 0; 
        private void HandlePlayerDead(OnPlayerDead evt)
        {
            if (!evt.IsDead || !_hasRound || _isMakingRound || _resultSent) return;
            _playerCount--;
            if (_playerCount == 0)
                FinishRound(BattleResult.PlayerLost);
        }
        
        private void HandleEnemyDead(OnEnemyDead evt)
        {
            if (!evt.isDead || !_hasRound || _isMakingRound || _resultSent) return;
            _enemiesCount--;
            if (_enemiesCount == 0)
                FinishRound(BattleResult.PlayerWon);
        }

        private void FinishRound(BattleResult result)
        {
            _resultSent = true;
            KillTask();
            eventChannel.RaiseEvent(new OnBattleResult(result));
        }

        private void CheckRoundClear()
        {
            if (IsRoundClear || onBattleList.Count > 0 || readyBattlesList.Count > 0) return;
            IsRoundClear = true;
            onRoundClear?.Invoke();
        }

        private void UpdateEnemyPositions()
        {
            foreach (EnemyStruct entry in eList)
            {
                EnemySelector selector = entry.EnemySelector;
                if (selector.IsNoneData || selector.IsDead) continue;

                int index = enemyLayoutGroup.GetActiveIndex(selector.transform);
                if (index >= 0 && index < _enemyPositions.Length)
                    selector.DefaultPosition.position = _enemyPositions[index];
            }
        }

        private bool ValidateRound(PlayerDataSO[] players, EnemyDataSO[] enemies)
        {
            if (players == null || enemies == null || players.Length > MaxUnitCount)
                return false;
            var enemySelectors = new HashSet<EnemySelector>();
            var enemyTypes = new HashSet<EnemyType>();
            if (eList.Count == 0 || eList.Count > MaxUnitCount ||
                eList.Exists(s => s.EnemySelector == null ||
                    !enemySelectors.Add(s.EnemySelector) || !enemyTypes.Add(s.EnemyType)))
            {
                return false;
            }
            var playerTypes = new HashSet<PlayerType>();
            if (Array.Exists(enemies, enemy => enemy == null) ||
                Array.Exists(players, player => player == null || GetPlayerSelector(player.PlayerType) == null ||
                    !playerTypes.Add(player.PlayerType)))
                return false;
            return true;
        }

        private PlayerSelector GetPlayerSelector(PlayerType playerType)
        {
            return playerType switch
            {
                PlayerType.Tanker => tanker,
                PlayerType.Dealer => dealer,
                PlayerType.Healer => healer,
                PlayerType.Mage => mage,
                _ => null
            };
        }
    }
}
