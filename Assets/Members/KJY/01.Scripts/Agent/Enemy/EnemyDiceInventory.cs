using System;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public class EnemyDiceInventory : AbstractDiceInventory
    {
        private void OnEnable()
        {
            eventChannel.AddListener<OnEndBattle>(HandleEndBattle);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEndBattle>(HandleEndBattle);
        }

        private void Start()
        {
            ApplyDiceData();
        }

        public override void DiceDataChanged()
        {
            
        }

        public override void Apply()
        {
            
        }
        
        
        private void HandleEndBattle(OnEndBattle obj)
        {
            ApplyDiceData();
        }

        private void ApplyDiceData() => savedDiceData = RunTimeDiceDataList.GetDiceData(DiceFaceType.Front); // 테스트
    }
}