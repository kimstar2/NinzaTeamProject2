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
            Debug.Log("OnSelect");
        }
        
        protected override void UnSelect()
        {
            onUnSelect?.Invoke();
            Debug.Log("OnUnSelect");
        }
    }
}