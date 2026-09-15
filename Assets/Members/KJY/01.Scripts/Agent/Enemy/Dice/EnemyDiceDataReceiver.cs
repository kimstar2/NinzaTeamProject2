using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDiceDataReceiver : AbstractDiceDataReceiver
    {
        [SerializeField] private EnemyNumber enemyType;

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDiceDataChanged>(HandleDiceDataChanged);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDiceDataChanged>(HandleDiceDataChanged);
        }

        private void HandleDiceDataChanged(OnEnemyDiceDataChanged evt)
        {
            if (evt.EnemyType != enemyType) return;
            DiceDataListSO list = evt.DiceDataList;
            FrontImage.SetSprite(list.Front.Icon);
            BackImage.SetSprite(list.Back.Icon);
            LeftImage.SetSprite(list.Left.Icon);
            RightImage.SetSprite(list.Right.Icon);
            TopImage.SetSprite(list.Top.Icon);
            BottomImage.SetSprite(list.Bottom.Icon);
        }
    }
}