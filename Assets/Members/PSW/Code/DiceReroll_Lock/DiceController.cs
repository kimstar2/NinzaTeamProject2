using System.Collections.Generic;
using System.Linq;
using DevLib.CoreLib.Runtime;
using Members.PSW.Code.DiceReroll_Lock.Events;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.DiceReroll_Lock
{
    public class DiceController : MonoBehaviour
    {
        [Header("EventChannel")] 
        [SerializeField] private EventChannelSO uiChannel;
        
        [SerializeField] private bool canCancelLock = false;
        [SerializeField] private TestDiceUIConnector uiConnector;
        
        private List<IReroll> _diceList;
        
        private void Awake()
        {
            _diceList = GetComponentsInChildren<IReroll>().ToList();
        }

        private void OnEnable()
        {
            EventSubscribe();
        }

        private void OnDisable()
        {
            EventDisSubscribe();
        }

        private void EventSubscribe()
        {
            uiChannel.AddListener<RerollEvent>(HandleReroll);
            uiChannel.AddListener<LockEvent>(LockDice);
        }

        private void EventDisSubscribe()
        {
            uiChannel.RemoveListener<RerollEvent>(HandleReroll);
            uiChannel.RemoveListener<LockEvent>(LockDice);

        }

        private void HandleReroll(RerollEvent evt)
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

        private void LockDice(LockEvent evt)
        {
            if (evt.DiceNum < 0 || evt.DiceNum >= _diceList.Count)
            {
                Debug.Log("해당 index에 값이 존재하지 않습니다.");
                return;
            }

            if (_diceList[evt.DiceNum].IsLocked && !canCancelLock)
            {
                Debug.Log($"{evt.DiceNum}번 주사위는 이미 잠겨있습니다. canCancelLock 값을 통해 잠금 해제 가능 여부를 조절할 수 있습니다.");
                return;  //여기는 락 상태일 때 락 취소를 false로 하면 락을 다시 풀지 않고 return
            }
            
            _diceList[evt.DiceNum].Lock();
        }
    }
}