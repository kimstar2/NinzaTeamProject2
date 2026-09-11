using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    public class EnemySelector : AbstractSelector
    {

        protected override void Select()
        {
            eventChannel.RaiseEvent(new OnEnemySelect(this));
            onSelect?.Invoke();
        }
        
        protected override void UnSelect()
        {
            onUnSelect?.Invoke();
        }

        public override void ApplyDamage(float damage) // 추후 데이터 추가 예정
        {
            HealthModule.TakeDamage(damage);
            Debug.Log($"아야 입은데미지 : {damage}");
        }

        public override void OnAttackCommand() { }
    }
}