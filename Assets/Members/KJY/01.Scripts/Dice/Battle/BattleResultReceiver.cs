using System;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Events;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class BattleResultReceiver : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private UIMonoTMP resultText;

        private void OnEnable()
        {
            eventChannel.AddListener<OnBattleResult>(HandleBattleResult);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnBattleResult>(HandleBattleResult);
        }

        private void HandleBattleResult(OnBattleResult obj)
        {
            resultText.gameObject.SetActive(true);
            resultText.SetText(obj.BattleResult.ToString());
        }
    }
}