using JetBrains.Annotations;
using UnityEngine;

namespace Members.LYW.Scripts.Event
{
    public class Eventer : MonoBehaviour
    {
        [SerializeField] private GameObject firstEvent;
        [SerializeField] private GameObject secondEvent;
        
        [SerializeField] private CompleteUI completeUI;
        
        public void Say()
        {
            Debug.Log("Say");
        }

        public void MoveNextEvent()
        {
            firstEvent.SetActive(false);
            secondEvent.SetActive(true);
        }

        public void EndEvent(string text)
        {
            completeUI.Complete(text);
        }

        public void SetPlayerStatus([CanBeNull] ChangeStatusDataSO changeStatusData)
        {
            Debug.Log("플레이어 값 변동됨.");
            //플레이어 json 에서 changeStatusData 만큼씩 제거하는 로직 작성
        }
    }
}