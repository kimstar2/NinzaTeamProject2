using JetBrains.Annotations;
using UnityEngine;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Service;

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
            if (changeStatusData == null || !ServiceLocator.TryGet<IBattleDataStorage>(out var storage)) return;
            foreach (var player in storage.Instance.GetRunTimePlayerData())
            {
                if (changeStatusData.health > 0) player.Heal(changeStatusData.health);
                else if (changeStatusData.health < 0) player.TakeDamage(-changeStatusData.health);
                if (changeStatusData.healMaxHealthRatio > 0f) player.Heal(player.MaxHealth * changeStatusData.healMaxHealthRatio);
            }
        }
    }
}
