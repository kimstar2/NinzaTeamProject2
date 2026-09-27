using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceRerollUnlink : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private PlayerSelector player;
        [SerializeField] private DiceLockChecker diceLock;

        private void OnEnable() => eventChannel.AddListener<OnPlayerRoll>(HandleRoll);
        private void OnDisable() => eventChannel.RemoveListener<OnPlayerRoll>(HandleRoll);

        private void HandleRoll(OnPlayerRoll evt)
        {
            if (player.RuntimePlayerData == null || player.IsDead || diceLock.IsLocked) return;
            var type = player.RuntimePlayerData.PlayerType;
            if (evt.playerType != type && evt.playerType != PlayerType.All) return;
            eventChannel.RaiseEvent(new OnPlayerUnSelect(type));
        }
    }
}
