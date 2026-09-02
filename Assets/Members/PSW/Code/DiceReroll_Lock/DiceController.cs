using System.Collections.Generic;
using System.Linq;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.DiceReroll_Lock
{
    public class DiceController : MonoBehaviour
    {
        [SerializeField] private bool canCancelLock = false;
        
        private List<IReroll> _diceList;

        [SerializeField]private TestDiceUIConnector uiConnector;

        private void Awake()
        {
            _diceList = GetComponentsInChildren<IReroll>().ToList();
        }

        private void OnEnable()
        {
            EventSubscribe();
        }

        private void OnDestroy()
        {
            EventDisSubscribe();
        }

        private void EventSubscribe()
        {
            uiConnector.OnRerollClick += HandleReroll;
            uiConnector.OnDiceLock += LockDice;
        }

        private void EventDisSubscribe()
        {
            uiConnector.OnRerollClick -= HandleReroll;
            uiConnector.OnDiceLock -= LockDice;
        }

        private void HandleReroll()
        {
            foreach (IReroll dice in _diceList)
            {
                if (dice.IsLocked)
                {
                    Debug.Log("주사위가 잠긴 상태입니다.");
                    continue;
                }
                
                Debug.Log("주사위 리롤 성공");
                dice.Reroll();
            }
        }

        private void LockDice(int index)
        {
            if (index < 0 || index >= _diceList.Count)
            {
                Debug.Log("해당 index에 값이 존재하지 않습니다.");
                return;
            }

            if (_diceList[index].IsLocked && !canCancelLock)
            {
                Debug.Log($"{index}번 주사위는 이미 잠겨있습니다. canCancelLock 값을 통해 잠금 해제 가능 여부를 조절할 수 있습니다.");
                return;  //여기는 락 상태일 때 락 취소를 false로 하면 락을 다시 풀지 않고 return
            }
            
            _diceList[index].Lock();
        }
    }
}