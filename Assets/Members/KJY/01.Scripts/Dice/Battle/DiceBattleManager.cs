using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.KJY._01.Scripts.Command;
using Members.KJY._01.Scripts.Dice.Interface;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using Members.KJY._01.Scripts.Flags;
using Members.KJY._01.Scripts.Mono;
using UnityEngine;
using UnityEngine.Events;
using ZLinq;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class DiceBattleManager : ModuleOwner , IRequirePooling
    {
        public const int MaxBossRetaliations = 2;
        [field: SerializeField] public PlayerSelector CurrentPlayerSelector { get; private set; }
        [SerializeField] private AbstractDiceRollManager pRollManager,eRollManager;
        [SerializeField] private EventChannelSO eventChannel;
        
        [Header("Line Renderer Set")]
        [SerializeField] private MonoLineRenderer copyLineRenderer;
        [SerializeField] private Transform lRParent;
        [SerializeField] private float lRFadeTime;
        public UnityEvent onStartBattle;
        public UnityEvent onEndBattle;
        
        private readonly Dictionary<PlayerSelector, MonoLineRenderer> _lineConnectors = new();
        private BattleObserverService _battleObserverService;
        private readonly List<PlayerSelector> _players = new();
        private readonly List<EnemySelector> _enemies = new();
        
        private readonly LinkedList<(PlayerSelector playerSelector, AbstractSelector targetSelector)> _orderedChain = new();
        public Dictionary<PlayerSelector, LinkedListNode<(PlayerSelector playerSelector, AbstractSelector targetSelector)>> BattleChain { get; private set; } = new();
        public DiceBattleManager Instance => this;
        public int Count => BattleChain.Count;
        public bool IsBattle => _battleObserverService != null && _battleObserverService.IsBattle;
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            
            _battleObserverService = GetModule<BattleObserverService>();
            Debug.Assert(_battleObserverService != null,"_battleObserver is null");
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerSelect>(HandleDiceSelected);
            eventChannel.AddListener<OnPlayerUnSelect>(HandleDiceUnSelected);
            eventChannel.AddListener<OnEnemySelect>(HandleTargetSelected);
            eventChannel.AddListener<OnEndBattle>(HandleEndBattle);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerSelect>(HandleDiceSelected);
            eventChannel.RemoveListener<OnPlayerUnSelect>(HandleDiceUnSelected);
            eventChannel.RemoveListener<OnEnemySelect>(HandleTargetSelected);
            eventChannel.RemoveListener<OnEndBattle>(HandleEndBattle);
        }
        
        private void ConnectLine(PlayerSelector getSelector)
        {
            if (!TryGetValue(getSelector , out AbstractSelector targetSelector)) return;
            if (targetSelector == getSelector)
            {
                RemoveLine(getSelector);
                return;
            }
            if (!_lineConnectors.TryGetValue(getSelector, out MonoLineRenderer line))
            {
                line = Instantiate(copyLineRenderer, lRParent, true);
                _lineConnectors.Add(getSelector, line);
            }
            line.Connect(getSelector.LineConnectTrm, targetSelector.LineConnectTrm,
                getSelector.LineColor.GetGradient(), lRFadeTime, targetSelector is PlayerSelector);
        }

        public void RemoveLine(PlayerSelector selector)
        {
            if (!_lineConnectors.Remove(selector, out MonoLineRenderer line)) return;
            line.Disconnect(lRFadeTime);
        }

        // 현재 플레이어가 선택중에 있는지 체크

        #region EventHandles

        
        private void HandleDiceSelected(OnPlayerSelect evt)
        {
            if (IsBattle || _battleObserverService.HasBattleResult || !pRollManager.AllDiceRollEnd) return;
            var clicked = evt.PlayerSelector;
            if (clicked == null || clicked.IsDead) return;
            if (TryConnectTarget(clicked)) return;
            if (BattleChain.ContainsKey(clicked))
            {
                clicked.OffSetTarget();
                RemoveFromBattleChain(clicked);
                RemoveLine(clicked);
                return;
            }
            bool cancel = CurrentPlayerSelector == clicked;
            CurrentPlayerSelector?.OffSetTarget();
            CurrentPlayerSelector = cancel ? null : clicked;
            if (cancel) return;
            clicked.BeginSelection();
            var skill = clicked.CurrentSkill;
            if (skill?.Target == SkillDataSO.TargetType.Self) TryConnectTarget(clicked);
            else if (skill != null && skill.IsArea) TryConnectTarget(clicked);
        }

        private void HandleTargetSelected(OnEnemySelect evt) => TryConnectTarget(evt.EnemySelector);

        private bool TryConnectTarget(AbstractSelector target)
        {
            if (IsBattle || !pRollManager.AllDiceRollEnd || CurrentPlayerSelector == null || !CurrentPlayerSelector.IsSelect ||
                CurrentPlayerSelector.CurrentSkill == null) return false;
            var skill = CurrentPlayerSelector.CurrentSkill;
            bool areaSelf = skill.IsArea && target == CurrentPlayerSelector;
            if (!areaSelf && !skill.CanTarget(CurrentPlayerSelector, target)) return false;
            AddOrMoveToLast(CurrentPlayerSelector, target);
            ConnectLine(CurrentPlayerSelector);
            eventChannel.RaiseEvent(new OnBattleChainChanged(CurrentPlayerSelector.RuntimePlayerData,Count,true));
            CurrentPlayerSelector.OnSetTarget();
            ClearCrtSelector();
            return true;
        }

        private void HandleDiceUnSelected(OnPlayerUnSelect evt) // 플레이어가 선택을 취소 했다면 
        {
            PlayerSelector selector = null;
            if (CurrentPlayerSelector != null && CurrentPlayerSelector.RuntimePlayerData.PlayerType == evt.PlayerType)
            {
                selector = CurrentPlayerSelector;
                ClearCrtSelector();
            }
            else
            {
                foreach (PlayerSelector player in BattleChain.Keys)
                    if (player.RuntimePlayerData.PlayerType == evt.PlayerType) { selector = player; break; }
            }
            if (selector == null) return;
            selector.OffSetTarget();
            RemoveFromBattleChain(selector);
            RemoveLine(selector); // 다른 애 고르는 중에도 기존 연결은 따로 취소 가능
        }
        
        public void StartBattle() // 배틀 시작 버튼을 눌렀을때
        {
            if (_battleObserverService.IsBattle || _battleObserverService.HasBattleResult) return;
            if (Count == 0) return;
            if (!pRollManager.AllDiceRollEnd || !eRollManager.AllDiceRollEnd) return;
            
            _battleObserverService.AddCommand(new OnActionCommand(onStartBattle.Invoke,null));
            ActionCommand[] getPlayerAttackData = GetP2TAtkCommands();
            foreach (ActionCommand attackCommand in getPlayerAttackData)
                _battleObserverService.AddCommand(attackCommand);
            ActionCommand[] getEnemyAttackData = GetE2PAtkCommands();
            foreach (ActionCommand attackCommand in getEnemyAttackData)
                _battleObserverService.AddCommand(attackCommand);
            _battleObserverService.AddCommand(new StatusTickCommand(GetCombatants));
            // 현재 명령(커맨드)들을 알림, 이는 배틀 옵저버가 받게 됨

            CurrentPlayerSelector?.OffSetTarget();
            ClearCrtSelector();
            _battleObserverService.StartBattle();
            
            RemoveAllLine();
            eventChannel.RaiseEvent(new OnStartBattle());
        }

        public void ExecuteNextCommand() => eventChannel.RaiseEvent(new OnExecuteNextCommand());
        
        private void RemoveAllLine()
        {
            List<PlayerSelector> removeList = BattleChain.Keys.AsValueEnumerable().ToList();

            foreach (PlayerSelector pS in removeList)
            {
                pS.OffSetTarget();
                RemoveLine(pS);
            }
        }

        private void HandleEndBattle(OnEndBattle obj)
        {
            ClearSelection();
            onEndBattle?.Invoke();
        }

        public void ClearSelection()
        {
            CurrentPlayerSelector?.OffSetTarget();
            ClearCrtSelector();
            List<PlayerSelector> removeList = BattleChain.Keys.AsValueEnumerable().ToList();

            foreach (PlayerSelector pS in removeList)
            {
                pS.OffSetTarget();
                RemoveFromBattleChain(pS);
                RemoveLine(pS);
            }
            
        }
        
        #endregion

        #region Helper
        
        private ActionCommand[] GetP2TAtkCommands() // 플레이어가 적한테 공격
        {
            ActionCommand[] d = _orderedChain.AsValueEnumerable().Select(s => new ActionCommand(s.playerSelector, s.targetSelector)).
                ToArray();
            return d;
        }
        
        
        // Default: null
        private PlayerSelector PickPlayerTarget(TargetRule rule)
        {
            if (rule == TargetRule.Default) return null;
            var alive = new List<PlayerSelector>();
            foreach (var player in _players)
                if (player != null && !player.IsDead && player.AgentData != null) alive.Add(player);
            if (alive.Count == 0) return null;
            if (rule == TargetRule.Random) return alive[UnityEngine.Random.Range(0, alive.Count)];

            PlayerSelector lowest = alive[0];
            float lowestRatio = float.MaxValue;
            foreach (var player in alive)
            {
                var health = player.MyAgent.HealthModule;
                float ratio = health.CurrentHealth / Mathf.Max(1f, health.DefaultMaxHealth);
                if (ratio < lowestRatio) { lowestRatio = ratio; lowest = player; }
            }
            return lowest;
        }

        private IEnumerable<AbstractSelector> GetCombatants()
        {
            foreach (var player in _players) yield return player;
            foreach (var enemy in _enemies) yield return enemy;
        }

        public void SetCombatants(IEnumerable<PlayerSelector> players, IEnumerable<EnemySelector> enemies)
        {
            _players.Clear();
            _players.AddRange(players);
            _enemies.Clear();
            _enemies.AddRange(enemies);
        }

        private ActionCommand[] GetE2PAtkCommands()
        {
            var commands = new List<ActionCommand>();
            foreach (var enemy in _enemies)
            {
                if (enemy == null || enemy.IsDead || enemy.IsNoneData || enemy.CurrentSkill == null) continue;
                AbstractSelector target;
                if (enemy.CurrentSkill.Target == SkillDataSO.TargetType.Self) target = enemy;
                else if (enemy.CurrentSkill.Target == SkillDataSO.TargetType.Ally)
                {
                    target = _enemies.Where(e => !e.IsNoneData && !e.IsDead)
                        .OrderBy(e => e.MyAgent.HealthModule.CurrentHealth / e.MyAgent.HealthModule.DefaultMaxHealth)
                        .FirstOrDefault();
                }
                else
                {
                    target = PickPlayerTarget(enemy.CurrentSkill.EnemyTargetRule);
                    if (target == null)
                        target = _orderedChain.FirstOrDefault(pair => pair.targetSelector == enemy).playerSelector;
                    if (target == null || target.IsDead)
                        target = _players.FirstOrDefault(player => player != null && !player.IsDead);
                }
                if (target == null) continue;
                int actions = enemy.RuntimeEnemyData.Rank == EnemyRank.Boss && !enemy.CurrentSkill.IsArea &&
                    enemy.CurrentSkill.Target == SkillDataSO.TargetType.Enemy ? MaxBossRetaliations : 1;
                for (int i = 0; i < actions; i++) commands.Add(new ActionCommand(enemy, target));
            }
            return commands.ToArray();
        }
        
        private void ClearCrtSelector() => CurrentPlayerSelector = null;

        public void AddOrMoveToLast(PlayerSelector key, AbstractSelector value)
        {
            if (BattleChain.TryGetValue(key, out var node))
            {
                node.Value = (playerSelector: key, targetSelector: value);

                _orderedChain.Remove(node);
                _orderedChain.AddLast(node);
                return;
            }
            
            BattleChain.Add(key,_orderedChain.AddLast((key,value)));
        }

        private void RemoveFromBattleChain(PlayerSelector key)
        {
            if (!BattleChain.TryGetValue(key, out var node)) return;

            _orderedChain.Remove(node);
            BattleChain.Remove(key);
            
            eventChannel.RaiseEvent(new OnBattleChainChanged(key.RuntimePlayerData,Count,false));
        }

        public bool TryGetValue(PlayerSelector key, out AbstractSelector value)
        {
            if (BattleChain.TryGetValue(key, out var node))
            {
                value = node.Value.targetSelector;
                return true;
            }
            value = null;
            return false;
        }

        #endregion
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            foreach (var p in _orderedChain)
                Gizmos.DrawLine(p.playerSelector.LineConnectTrm.position, p.targetSelector.LineConnectTrm.position);
        }
        #endif
    }
}
