using Members.KJY._01.Scripts.Dice;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Title
{
    public class TitleMenu : MonoBehaviour
    {
        public UnityEvent onMenuOpen;
        public void Set()
        {
            onMenuOpen?.Invoke();
        }
    }
}