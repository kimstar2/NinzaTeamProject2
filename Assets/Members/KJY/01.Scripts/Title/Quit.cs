using Members.KJY._01.Scripts.Dice;
using UnityEngine;

namespace Members.KJY._01.Scripts.Title
{
    public class Quit : MonoBehaviour
    {
        [SerializeField] private TitleDice titleDice;
        public void QuitGame()
        {
            if (titleDice != null && !titleDice.CanNavigate) return;
            Application.Quit();
        }
    }
}
