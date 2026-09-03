using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Members.KJY.Scripts.UI.Dice
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