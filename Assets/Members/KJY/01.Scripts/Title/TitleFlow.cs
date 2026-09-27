using Members.KJY._01.Scripts.UI;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.KJY._01.Scripts.Service;
using UnityEngine;

namespace Members.KJY._01.Scripts.Title
{
    public class TitleFlow : MonoBehaviour
    {
        [SerializeField] private TitleDice titleDice;
        public void StartGame()
        {
            if (titleDice != null && !titleDice.CanNavigate) return;
            if (ServiceLocator.TryGet<IBattleDataStorage>(out var storage)) storage.Instance.ResetRun();
            if (ServiceLocator.TryGet<Inventory>(out var inventory) && inventory is BattleInventory battleInventory)
                battleInventory.ResetRun();
            SceneTransition.Load("Assets/Members/CJY/Scene/CJY.unity");
        }
    }
}
