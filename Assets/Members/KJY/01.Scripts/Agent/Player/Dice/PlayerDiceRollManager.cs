using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Events.Player;
using Members.KJY._01.Scripts.Service;
using UnityEngine;
using UnityEngine.Events;
using ZLinq;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    [Serializable]
    public class PlayerDiceRollCheck
    {
        [field:SerializeField] public PlayerType PlayerType {get; private set;}
        public bool DiceRollEnd { get; private set; } = true;
        public bool IsRolling { get; private set; }
        public bool IsDead { get; private set; }
        public UnityEvent<PlayerDiceRollData> onRollEnd;
        public UnityEvent<PlayerType> onRoll;
        
        public void OnRollEnd(DiceFaceType diceFaceType)
        {
            onRollEnd?.Invoke(new PlayerDiceRollData(PlayerType, diceFaceType));
            DiceRollEnd = true;
            IsRolling = false;
        }
        
        public void OnRoll()
        {
            DiceRollEnd = false;
            IsRolling = true;
            onRoll?.Invoke(PlayerType);
        }

        public void OnDead()
        {
            IsDead = true;
            IsRolling = false;
        }
        public void Init()
        {
            IsDead = false;
            DiceRollEnd = false;
            IsRolling = false;
        }
    }
    
    public class PlayerDiceRollManager : AbstractDiceRollManager , IGetRiskPenalty
    {
        [field:SerializeField] public List<PlayerDiceRollCheck> DiceRollCheckList {get; private set;}
        [SerializeField] private float maxRiskLevel; 
        [Min(1f),SerializeField] private float riskLevelIncreaseMulti;
        [SerializeField] private float riskLevelIncrease;
        private float _maxRiskPenalty;
        private float _riskLevel;
        private int _rollCount;
        private BattleDataStorage _battleDataStorage;
        
        public UnityEvent<float> onRiskLevelChanged;
        public UnityEvent onReachMaxRisk;


        protected override void InitializeModules()
        {
            base.InitializeModules();
            ServiceLocator.Register<IGetRiskPenalty>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IGetRiskPenalty>();
        }

        private void Start()
        {
                _battleDataStorage = ServiceLocator.Get<IBattleDataStorage>().Instance;
            var data = _battleDataStorage.GetBattleData();
            
            _maxRiskPenalty = data.maxRiskPenalty;
            riskLevelIncrease = data.riskLevelIncrease;
            riskLevelIncreaseMulti = data.riskLevelIncreaseMulti;
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerRollEnd>(HandleRollEnd);
            eventChannel.AddListener<OnPlayerDead>(HandleDead);
            eventChannel.AddListener<OnPlayerDataReceive>(HandleDataReceive);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerRollEnd>(HandleRollEnd);
            eventChannel.RemoveListener<OnPlayerDead>(HandleDead);
            eventChannel.RemoveListener<OnPlayerDataReceive>(HandleDataReceive);
        }

        private void HandleDataReceive(OnPlayerDataReceive evt)
        {
            DiceRollCheckList.Find(x => x.PlayerType == evt.PlayerDataData.PlayerType)?.Init();
            CheckAllRollEnd();
        }
        

        private void HandleRollEnd(OnPlayerRollEnd obj)
        {
            PlayerDiceRollCheck check = DiceRollCheckList.Find(x => x.PlayerType == obj.PlayerType);
            if (check == null || check.IsDead) return;
            check.OnRollEnd(obj.DiceFaceType);
            CheckAllRollEnd();
        }

        private void HandleDead(OnPlayerDead evt)
        {
            if (!evt.IsDead) return;
            DiceRollCheckList.Find(x => x.PlayerType == evt.PlayerType)?.OnDead();
            CheckAllRollEnd();
        }

        private void CheckAllRollEnd()
        {
            AllDiceRollEnd = DiceRollCheckList.TrueForAll(x => x.IsDead || x.DiceRollEnd);
            if (AllDiceRollEnd) onAllDiceRollEnd?.Invoke();
        }

        private bool _isFirstRoll = true;
        private bool _canRoll = false;
        public void Roll(bool isStartRoll)
        {
            if (isStartRoll)
                _canRoll = true;
            else if (!_canRoll) return;
            
            if (DiceRollCheckList.TrueForAll(x => x.IsDead) ||
                DiceRollCheckList.Exists(x => !x.IsDead && x.IsRolling)) return;
            RollLogic();
            if (_isFirstRoll)
            {
                _isFirstRoll = false;
                return;
            }
            CalcRisk();
        }

        protected override void RollLogic()
        {
            AllDiceRollEnd = false;
            foreach (PlayerDiceRollCheck check in DiceRollCheckList)
            {
                if (check.IsDead) continue;
                check.OnRoll();
                eventChannel.RaiseEvent(new OnPlayerRoll(check.PlayerType));
            }
            
            CheckAllRollEnd();
        }

        private bool _reachMaxRisk;
        private void CalcRisk()
        {
            _rollCount++;
            float countRisk = DiceRollCheckList.AsValueEnumerable().Count(s => !s.IsDead) * _rollCount * 0.25f;;
            float riskLevel = riskLevelIncreaseMulti * riskLevelIncrease + _riskLevel;
            
            _riskLevel = Mathf.Min
                (maxRiskLevel,countRisk + riskLevel);
            onRiskLevelChanged?.Invoke(_riskLevel / maxRiskLevel);
            if (_reachMaxRisk)
                onReachMaxRisk?.Invoke();
            else
                eventChannel.RaiseEvent(new OnRiskPenaltyChanged(GetRiskPenalty()));
            
            if (_riskLevel/maxRiskLevel >= 1f)
                _reachMaxRisk = true;
        }

        public float GetRiskPenalty()
        {
            return _riskLevel / maxRiskLevel * _maxRiskPenalty + 1;
        }

        public void ResetRollCount() => _rollCount = 0;
    }
}
