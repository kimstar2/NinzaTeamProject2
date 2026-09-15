using DG.Tweening;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDice : AbstractDice
    {
        [SerializeField] private EnemyNumber enemyType;

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyRoll>(HandleEnemyRoll);
        }

        private void OnDisable()
        {
            roll?.Kill();
            roll = null;
            
            eventChannel.RemoveListener<OnEnemyRoll>(HandleEnemyRoll);
        }

        private void HandleEnemyRoll(OnEnemyRoll obj)
            => Roll(destTrm.localPosition,GetRandom());
        protected override void CompleteRoll()
        {
            crtFaceType = diceFaces[currenRan].Type;
            eventChannel.RaiseEvent(new OnEnemyRollEnd(crtFaceType, enemyType));
        }
    }
}