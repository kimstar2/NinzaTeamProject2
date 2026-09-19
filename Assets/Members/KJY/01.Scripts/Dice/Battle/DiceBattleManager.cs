using System;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using DG.Tweening;
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
        [SerializeField] private EventChannelSO eventChannel;
        
        [Header("Line Renderer Set")]
        [SerializeField] private MonoLineRenderer copyLineRenderer;
        [SerializeField] private Transform lRParent;
        [SerializeField] private float lRFadeTime;
        public UnityEvent onStartBattle;
        public UnityEvent onEndBattle;

        private readonly Dictionary<PlayerSelector, LineRenderer> _lineConnectors = new();
        private BattleObserver _battleObserver;
        [SerializeField] private List<PlayerSelector> key;
        [SerializeField] private List<AbstractSelector> value;
        
        public int Count => BattleChain.Count;

        private readonly LinkedList<(PlayerSelector playerSelector, AbstractSelector targetSelector)> _orderedChain = new();
        // public Dictionary<PlayerSelector,EnemySelector> BattleChain { get; private set; } = new();
        public Dictionary<PlayerSelector, LinkedListNode<(PlayerSelector playerSelector, AbstractSelector targetSelector)>> BattleChain { get; private set; } = new();
        
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
            eventChannel.AddListener<OnStartBattle>(HandleStartBattle);
            eventChannel.AddListener<OnEndBattle>(HandleEndBattle);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerSelect>(HandleDiceSelected);
            eventChannel.RemoveListener<OnPlayerUnSelect>(HandleDiceUnSelected);
            eventChannel.RemoveListener<OnEnemySelect>(HandleTargetSelected);
            eventChannel.RemoveListener<OnStartBattle>(HandleStartBattle);
            eventChannel.RemoveListener<OnEndBattle>(HandleEndBattle);
        }
        
        private void OnDestroy() => ServiceLocator.UnRegister<IGetIsSelectService>();
        
        private void ConnectLine(PlayerSelector getSelector)
        {
            if (!TryGetValue(getSelector , out AbstractSelector targetSelector)) return;
            Vector3[] a2B = { getSelector.LineConnectTrm.position, targetSelector.LineConnectTrm.position };

            if (_lineConnectors.TryGetValue(getSelector, out LineRenderer lR))
            {
                lR.SetPositions(a2B);
                lR.enabled = true;
                return;
            }
            copyLineRenderer.SetGradient(getSelector.LineColor.GetGradient());
            lR = Instantiate(copyLineRenderer.LineRenderer, lRParent, true);

            lR.widthMultiplier = 0;
            
            DOTween.To(() => lR.widthMultiplier,
                    x => lR.widthMultiplier = x, 1, lRFadeTime)
                .SetLink(lR.gameObject , LinkBehaviour.KillOnDestroy);
            
            lR.SetPositions(a2B);
            
            _lineConnectors.Add(getSelector, lR);
        }

        public void RemoveLine(PlayerSelector selector)
        {
            if (!_lineConnectors.Remove(selector, out LineRenderer lR)) return; // 있으면? 가져옴
            DOTween.To(() => lR.widthMultiplier,
                    x => lR.widthMultiplier = x, 0, lRFadeTime)
                .SetLink(lR.gameObject , LinkBehaviour.KillOnDestroy)
                .OnComplete(() =>
                    {
                        Destroy(lR.gameObject); // 나중에 풀링 대체여
                    }
                );
        }

        // 현재 플레이어가 선택중에 있는지 체크
        public (PlayerType type, bool isSelect) GetIsSelect() => 
            (CurrentPlayerSelector != null ? CurrentPlayerSelector.PlayerData.PlayerType : PlayerType.None,
                CurrentPlayerSelector != null && CurrentPlayerSelector.IsSelect);
        
        #region EventHandles

        
        private void HandleDiceSelected(OnPlayerSelect evt) // 플레이어 선택을 했다면?
        {
            CurrentPlayerSelector = evt.PlayerSelector;
            
            if (!CurrentPlayerSelector.IsSelect) return;
            AddOrMoveToLast(CurrentPlayerSelector,evt.PlayerSelector); // 선택되어있는 플레이어 셀렉터랑 선택한 적을 연결 -> 마지막으로감
            ConnectLine(CurrentPlayerSelector);
            eventChannel.RaiseEvent(new OnBattleChainChanged(CurrentPlayerSelector.PlayerData,Count,true));

            CurrentPlayerSelector.OnSetTarget();

            // 현재 셀렉터는 선택한 플레이어 셀렉터
        }

        private void HandleTargetSelected(OnEnemySelect evt) // 타겟(적)을 선택을 했다면
        {
            if (CurrentPlayerSelector == null) return;
            if (!CurrentPlayerSelector.IsSelect) return; // 현재 셀렉터가 존재하면서 선택이 안되어있다면

            AddOrMoveToLast(CurrentPlayerSelector,evt.EnemySelector); // 선택되어있는 플레이어 셀렉터랑 선택한 적을 연결 -> 마지막으로감
            ConnectLine(CurrentPlayerSelector);
            eventChannel.RaiseEvent(new OnBattleChainChanged(CurrentPlayerSelector.PlayerData,Count,true));

            CurrentPlayerSelector.OnSetTarget();
        }

        private void HandleDiceUnSelected(OnPlayerUnSelect evt) // 플레이어가 선택을 취소 했다면 
        {
            if (CurrentPlayerSelector == null) return; // 근데 구라핑이면 리턴
            if (CurrentPlayerSelector.PlayerData.PlayerType != evt.PlayerType) return; // 취소한애가 현재 셀렉터랑 같냐? (선택되어있는 상태에서 한번더 눌렀을때)
            
            RemoveFromBattleChain(CurrentPlayerSelector); // 선택이 취소 된거니까 체인 연결이 되어있을경우 체인을 파기
            RemoveLine(CurrentPlayerSelector);

            ClearCrtSelector(); // 현재 셀렉터는 없음
        }
        
        private void HandleStartBattle(OnStartBattle evt) // 배틀 시작 버튼을 눌렀을때
        {
            _battleObserver.AddCommand(new OnActionCommand(onStartBattle.Invoke,null));
            ActionCommand[] getPlayerAttackData = GetP2TAtkCommands();
            foreach (ActionCommand attackCommand in getPlayerAttackData)
                _battleObserver.AddCommand(attackCommand);
            ActionCommand[] getEnemyAttackData = GetE2PAtkCommands();
            foreach (ActionCommand attackCommand in getEnemyAttackData)
                _battleObserver.AddCommand(attackCommand);
            // 현재 명령(커맨드)들을 알림, 이는 배틀 옵저버가 받게 됨

            ClearCrtSelector();
            _battleObserver.StartBattle();
            
            RemoveAllLine();
        }

        public void ExecuteNextCommand() => eventChannel.RaiseEvent(new OnExecuteNextCommand());
        
        private void RemoveAllLine()
        {
            List<PlayerSelector> removeList = BattleChain.Keys.AsValueEnumerable().ToList();

            foreach (PlayerSelector pS in removeList)
                RemoveLine(pS);
        }

        private void HandleEndBattle(OnEndBattle obj)
        {
            List<PlayerSelector> removeList = BattleChain.Keys.AsValueEnumerable().ToList();

            foreach (PlayerSelector pS in removeList)
            {
                pS.SelectToggle();
                RemoveFromBattleChain(pS);
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
