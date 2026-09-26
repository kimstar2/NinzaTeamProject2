using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Enemy.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

namespace Members.PSW.Code.Battle.LastReward
{
    public class LastRewardController : MonoBehaviour
    {
        [SerializeField] private EventChannelSO evtChannel;
        [SerializeField] private EnemyDiceInventory enemyDiceInventory;
        [SerializeField] private Transform headAnchor;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private LastDice lastDicePrefab;

        private LastDice _activeDice;
        private bool _rewardStarted;
        private LastDiceEvent _pendingReward;

        public void HandleEnemyDead()
        {
            if (_rewardStarted)
                return;
            if (evtChannel == null || enemyDiceInventory == null || headAnchor == null || lastDicePrefab == null)
            {
                Debug.Assert(false, "LastRewardController: 채널, 적 주사위 인벤토리, 머리 위치, 프리팹을 연결하세요.", this);
                return;
            }

            DiceDataListSO diceData = enemyDiceInventory.RunTimeDiceDataList;
            if (diceData == null)
            {
                Debug.Assert(false, "LastRewardController: 적의 런타임 주사위 데이터가 초기화되지 않았습니다.", this);
                return;
            }

            // 죽은 적의 6면 중 보상 확정
            DiceFaceType resultFace = (DiceFaceType)Random.Range(0, 6);
            DiceDataSO resultData = diceData.GetDiceData(resultFace);
            if (resultData == null)
            {
                Debug.Assert(false, "LastRewardController: 선택된 적 주사위 면의 데이터가 없습니다.", this);
                return;
            }
            _pendingReward = new LastDiceEvent(resultFace, resultData);

            _activeDice = Instantiate(lastDicePrefab, headAnchor.position + worldOffset, lastDicePrefab.transform.rotation);
            _activeDice.RollComplete += HandleRollComplete;
            _rewardStarted = true;
            if (!_activeDice.Throw(diceData, resultFace))
            {
                Debug.Assert(false, "LastRewardController: 주사위 프리팹의 활성 상태 및 면 설정을 확인하세요.", this);
                ReleaseActiveDice();
                _rewardStarted = false;
            }
        }

        private void HandleRollComplete()
        {
            _activeDice.RollComplete -= HandleRollComplete;
            _activeDice = null;
            LastDiceEvent reward = _pendingReward;
            _pendingReward = null;
            evtChannel.RaiseEvent(reward);
        }

        // 부활 및 재사용 시 초기화
        public void ResetReward()
        {
            ReleaseActiveDice();
            _rewardStarted = false;
        }

        private void ReleaseActiveDice()
        {
            _pendingReward = null;
            if (_activeDice == null)
                return;
            _activeDice.RollComplete -= HandleRollComplete;
            Destroy(_activeDice.gameObject);
            _activeDice = null;
        }

        private void OnDestroy() => ReleaseActiveDice();
    }
}
