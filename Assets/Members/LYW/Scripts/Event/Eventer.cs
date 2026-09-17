using UnityEngine;

namespace Members.LYW.Scripts.Event
{
    public class Eventer : MonoBehaviour
    {
        [SerializeField] private GameObject firstEvent;
        [SerializeField] private GameObject secondEvent;
        
        public void Say()
        {
            Debug.Log("Say");
        }

        public void MoveNextEvent()
        {
            firstEvent.SetActive(false);
            secondEvent.SetActive(true);
        }

        public void EndEvent()
        {
            //후 진행 설명 나와야하면 그거 나오는거 구현
            //(필요시) 위 구현과 함께 (필수) 보상 혹은 손해 지급 로직도 작성
            //그 후 노드 맵 띄우기
            //각 UI 전환은 부드럽게 구현
        }
    }
}