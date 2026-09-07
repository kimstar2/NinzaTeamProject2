using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.UI.Dice
{
    public class DiceList : MonoBehaviour
    {
        public UnityEvent onShowList;
        public UnityEvent offShowList;
        
        public void OnViewList()
        {
            onShowList?.Invoke();
        }

        public void OffViewList()
        {
            offShowList?.Invoke();
        }
    }
}