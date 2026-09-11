using System.Collections.Generic;
using System.Linq;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Command;
using Members.KJY._01.Scripts.Dice.Interface;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using Members.KJY._01.Scripts.Flags;
using Members.KJY._01.Scripts.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class DiceBattleManager : ModuleOwner , IGetIsSelectService , IRequirePooling
    {
        [field: SerializeField] public DiceSelector CurrentDiceSelector { get; private set; }
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private MonoLineRenderer copyLineRenderer;
        [SerializeField] private Transform lRParent;

        private readonly Dictionary<DiceSelector, LineRenderer> _lineConnectors = new();
        private BattleObserver _battleObserver;
        
        public Dictionary<DiceSelector,EnemySelector> BattleChain { get; private set; } = new();
        public List<DiceSelector> keys ;
        public List<EnemySelector> values;

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
            eventChannel.AddListener<OnStartBattle>(HandleBattleStart);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerSelect>(HandleDiceSelected);
            eventChannel.RemoveListener<OnPlayerUnSelect>(HandleDiceUnSelected);
            eventChannel.RemoveListener<OnEnemySelect>(HandleTargetSelected);
            eventChannel.RemoveListener<OnStartBattle>(HandleBattleStart);
        }

        private void OnDestroy() => ServiceLocator.UnRegister<IGetIsSelectService>();
        
        private void ConnectLine(DiceSelector getSelector)
        {
            EnemySelector enemySelector = BattleChain[getSelector];
            Vector3[] a2B = { getSelector.LineConnectTrm.position, enemySelector!.LineConnectTrm.position };

            if (_lineConnectors.TryGetValue(getSelector, out LineRenderer lR))
            {
                lR.SetPositions(a2B);
                lR.enabled = true;
                return;
            }
            copyLineRenderer.SetGradient(getSelector.LineColor.GetGradient());
            lR = Instantiate(copyLineRenderer.LineRenderer, lRParent, true);
            
            lR.SetPositions(a2B);
            
            _lineConnectors.Add(getSelector, lR);
        }
        
        public void RemoveLine(DiceSelector selector)
        {
            if (!_lineConnectors.TryGetValue(selector, out LineRenderer lR)) return; // 있으면? 가져옴
            Destroy(lR.gameObject); // 나중에 풀링 대체여
            _lineConnectors.Remove(selector);
        }

        private void ReCheck() // 키,밸 확인용
        {
            keys = BattleChain.Keys.ToList(); 
            values = BattleChain.Values.ToList(); 
        }
        
        // 현재 플레이어가 선택중에 있는지 체크
        public (PlayerType type, bool isSelect) GetIsSelect() => 
            (CurrentDiceSelector != null ? CurrentDiceSelector.PlayerType : PlayerType.None,
                CurrentDiceSelector != null && CurrentDiceSelector.IsSelect);
        
        #region EventHandles

        
        private void HandleDiceSelected(OnPlayerSelect evt) // 플레이어 선택을 했다면?
            => CurrentDiceSelector = evt.DiceSelector; // 현재 셀렉터는 선택한 플레이어 셀렉터

        private void HandleTargetSelected(OnEnemySelect evt) // 타겟(적)을 선택을 했다면
        {
            if (CurrentDiceSelector == null) return;
            if (!CurrentDiceSelector.IsSelect) return; // 현재 셀렉터가 존재하면서 선택이 되어있다면
            
            if (BattleChain.Contains(new KeyValuePair<DiceSelector, EnemySelector>(CurrentDiceSelector,evt.EnemySelector)))
            {
                BattleChain.Remove(CurrentDiceSelector);
                RemoveLine(CurrentDiceSelector);
                return;
            }
            
            BattleChain.Add(CurrentDiceSelector,evt.EnemySelector); // 선택되어있는 플레이어 셀렉터랑 선택한 적을 연결
            ConnectLine(CurrentDiceSelector);

            CurrentDiceSelector.OnSetTarget();
            ReCheck();
        }

        private void HandleDiceUnSelected(OnPlayerUnSelect evt) // 플레이어가 선택을 취소 했다면 
        {
            if (CurrentDiceSelector == null) return; // 근데 구라핑이면 리턴

            if (CurrentDiceSelector.PlayerType != evt.PlayerType) return; // 취소한애가 현재 셀렉터랑 같냐? (선택되어있는 상태에서 한번더 눌렀을때)
            
            BattleChain.Remove(CurrentDiceSelector); // 선택이 취소 된거니까 체인 연결이 되어있을경우 체인을 파기
                
            RemoveLine(CurrentDiceSelector);
            ClearCrtSelector(); // 현재 셀렉터는 없음
            ReCheck();
        }
        
        private void HandleBattleStart(OnStartBattle evt) // 배틀 시작 버튼을 눌렀을때
        {
            AttackCommand[] getPlayerAttackData = GetP2TAtkCommands();
            foreach (AttackCommand attackCommand in getPlayerAttackData)
                _battleObserver.AddCommand(attackCommand);
            
            // AttackCommand[] getEnemyAttackData = GetP2TAtkCommands();

            // foreach (AttackCommand attackCommand in getEnemyAttackData)
            //     _battleObserver.AddCommand(attackCommand);
            
            // 현재 명령(커맨드)들을 알림, 이는 배틀 옵저버가 받게 됨

            ClearCrtSelector();
            _battleObserver.StartBattle();
        }
        
        #endregion

        #region Helper

        private AttackCommand[] GetP2TAtkCommands() // 플레이어가 적한테 공격
        {
            AttackCommand[] d = BattleChain.
                Select(s => new AttackCommand(s.Key, s.Value)).
                ToArray();
            return d;
        }
        
        private AttackCommand[] GetE2PAtkCommands() // 플레이어가 적한테 공격
        {
            AttackCommand[] d = BattleChain.
                Select(s => new AttackCommand(s.Value, s.Key)).
                ToArray();
            return d;
        }
        
        private void ClearCrtSelector() => CurrentDiceSelector = null;

        #endregion
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            foreach (KeyValuePair<DiceSelector, EnemySelector> p in BattleChain)
                Gizmos.DrawLine(p.Key.LineConnectTrm.position, p.Value.transform.position);
        }
        #endif
    }
}