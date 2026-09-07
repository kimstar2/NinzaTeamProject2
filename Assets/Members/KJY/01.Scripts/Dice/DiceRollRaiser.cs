using System.Collections;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Services;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceRollRaiser : MonoModule
    {
        [SerializeField] private DiceDataSO testData;
        [SerializeField] private float maxRiskLevel;
        [SerializeField] private float riskLevel;
        [SerializeField] private float riskLevelIncrease; // 테스트용임
        [Min(1f),SerializeField] private float riskLevelIncreasePer; // 테스트용임
        private EventChannelSO _eventChannel;
        private int _rollCount;
        
        public UnityEvent<float> onRiskLevelChanged;

        private void Start()
        {
            _eventChannel = ServiceLocator.Get<IGetEventService>().EventChannel;
        }

        public void Roll()
        {
            _eventChannel.RaiseEvent(new OnRoll());

            _rollCount++;
            riskLevel += riskLevelIncreasePer * _rollCount * riskLevelIncrease;
            onRiskLevelChanged?.Invoke(riskLevel / maxRiskLevel);
        }
        
        public void ResetRollCount() => _rollCount = 0;
    }
}