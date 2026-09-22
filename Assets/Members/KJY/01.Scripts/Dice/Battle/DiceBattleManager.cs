using System;
using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Command;
using Members.KJY._01.Scripts.Dice.Interface;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using Members.KJY._01.Scripts.Flags;
using Members.KJY._01.Scripts.Mono;
using UnityEngine;
using UnityEngine.Events;
using ZLinq;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class DiceBattleManager : ModuleOwner , IGetIsSelectService , IRequirePooling
    {
        [field: SerializeField] public PlayerSelector CurrentPlayerSelector { get; private set; }
        [SerializeField] private AbstractDiceRollManager pRollManager,eRollManager;
        [SerializeField] private EventChannelSO eventChannel;
        
        [Header("Line Renderer Set")]
        [SerializeField] private MonoLineRenderer copyLineRenderer;
        [SerializeField] private Transform lRParent;
        [SerializeField] private float lRFadeTime;
        public UnityEvent onStartBattle;
        public UnityEvent onEndBattle;

        [SerializeField] private List<PlayerSelector> key;
        [SerializeField] private List<AbstractSelector> value;
        
        private readonly Dictionary<PlayerSelector, MonoLineRenderer> _lineConnectors = new();
        private BattleObserver _battleObserver;
        

        private readonly LinkedList<(PlayerSelector playerSelector, AbstractSelector targetSelector)> _orderedChain = new();
        public Dictionary<PlayerSelector, LinkedListNode<(PlayerSelector playerSelector, AbstractSelector targetSelector)>> BattleChain { get; private set; } = new();
        public int Count => BattleChain.Count;
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            
            _battleObserver = GetModule<BattleObserver>();
            Debug.Assert(_battleObserver != null,"_battleObserver is null");
                
            ServiceLocator.Register<IGetIsSelectService>(this);
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
        
        private void OnDestroy() => ServiceLocator.UnRegister<IGetIsSelectService>();
        
        private void ConnectLine(PlayerSelector getSelector)
        {
            if (!TryGetValue(getSelector , out AbstractSelector targetSelector)) return;
            if (!_lineConnectors.TryGetValue(getSelector, out MonoLineRenderer line))
            {
                line = Instantiate(copyLineRenderer, lRParent, true);
                _lineConnectors.Add(getSelector, line);
            }
            line.Connect(getSelector.LineConnectTrm, targetSelector.LineConnectTrm,
                getSelector.LineColor.GetGradient(), lRFadeTime);
        }

        public void RemoveLine(PlayerSelector selector)
        {
            if (!_lineConnectors.Remove(selector, out MonoLineRenderer line)) return;
            line.Disconnect(lRFadeTime);
        }

        // 현재 플레이어가 선택중에 있는지 체크
        public (PlayerType type, bool isSelect) GetIsSelect() => 
            (CurrentPlayerSelector != null ? CurrentPlayerSelector.PlayerData.PlayerType : PlayerType.None,
                CurrentPlayerSelector != null && CurrentPlayerSelector.IsSelect);
        
        #region EventHandles

        
        private void HandleDiceSelected(OnPlayerSelect evt) // 플레이어 선택을 했다면?
        {
            CurrentPlayerSelector = evt.PlayerSelector;
            // 여기선 선택만 함. 체인 연결은 적 눌렀을 때
        }

        private void HandleTargetSelected(OnEnemySelect evt) // 타겟(적)을 선택을 했다면
        {
            if (CurrentPlayerSelector == null) return;
            if (!CurrentPlayerSelector.IsSelect) return; // 현재 셀렉터가 존재하면서 선택이 안되어있다면
            if (CurrentPlayerSelector.IsDead || evt.EnemySelector.IsDead) return;

            AddOrMoveToLast(CurrentPlayerSelector,evt.EnemySelector); // 선택되어있는 플레이어 셀렉터랑 선택한 적을 연결 -> 마지막으로감
            ConnectLine(CurrentPlayerSelector);
            eventChannel.RaiseEvent(new OnBattleChainChanged(CurrentPlayerSelector.PlayerData,Count,true));

            CurrentPlayerSelector.OnSetTarget();
            ClearCrtSelector();
        }

        private void HandleDiceUnSelected(OnPlayerUnSelect evt) // 플레이어가 선택을 취소 했다면 
        {
            PlayerSelector selector = null;
            if (CurrentPlayerSelector != null && CurrentPlayerSelector.PlayerData.PlayerType == evt.PlayerType)
            {
                selector = CurrentPlayerSelector;
                ClearCrtSelector();
            }
            else
            {
                foreach (PlayerSelector player in BattleChain.Keys)
                    if (player.PlayerData.PlayerType == evt.PlayerType) { selector = player; break; }
            }
            if (selector == null) return;
            RemoveFromBattleChain(selector);
            RemoveLine(selector); // 다른 애 고르는 중에도 기존 연결은 따로 취소 가능
        }
        
        public void StartBattle() // 배틀 시작 버튼을 눌렀을때
        {
            if (_battleObserver.IsBattle) return;
            if (!pRollManager.AllDiceRollEnd || !eRollManager.AllDiceRollEnd) return;
            
            _battleObserver.AddCommand(new OnActionCommand(onStartBattle.Invoke,null));
            ActionCommand[] getPlayerAttackData = GetP2TAtkCommands();
            foreach (ActionCommand attackCommand in getPlayerAttackData)
                _battleObserver.AddCommand(attackCommand);
            ActionCommand[] getEnemyAttackData = GetE2PAtkCommands();
            foreach (ActionCommand attackCommand in getEnemyAttackData)
                _battleObserver.AddCommand(attackCommand);
            // 현재 명령(커맨드)들을 알림, 이는 배틀 옵저버가 받게 됨

            CurrentPlayerSelector?.OffSetTarget();
            ClearCrtSelector();
            _battleObserver.StartBattle();
            
            RemoveAllLine();
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
            List<PlayerSelector> removeList = BattleChain.Keys.AsValueEnumerable().ToList();

            foreach (PlayerSelector pS in removeList)
            {
                pS.OffSetTarget();
                RemoveFromBattleChain(pS);
                RemoveLine(pS);
            }
            
            onEndBattle?.Invoke();
        }
        
        #endregion

        #region Helper

        private ActionCommand[] GetP2TAtkCommands() // 플레이어가 적한테 공격
        {
            ActionCommand[] d = _orderedChain.AsValueEnumerable().Select(s => new ActionCommand(s.playerSelector, s.targetSelector)).
                ToArray();
            return d;
        }
        
        
        private ActionCommand[] GetE2PAtkCommands() // 플레이어가 적한테 공격
        {
            ActionCommand[] d = _orderedChain.AsValueEnumerable().Select(s => new ActionCommand(s.targetSelector, s.playerSelector)).
                ToArray();
            return d;
        }

        private void ReCheck()
        {
            key = BattleChain.Keys.AsValueEnumerable().ToList();
            var d = BattleChain.Values.AsValueEnumerable().ToList();
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

        private bool RemoveFromBattleChain(PlayerSelector key)
        {
            if (!BattleChain.TryGetValue(key, out var node)) return false;

            _orderedChain.Remove(node);
            BattleChain.Remove(key);
            
            eventChannel.RaiseEvent(new OnBattleChainChanged(key.PlayerData,Count,false));
            return true;
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
        
        public AbstractSelector TryGetValue(PlayerSelector key)
        {
            return BattleChain.TryGetValue(key, out var node) ? node.Value.targetSelector : null;
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
