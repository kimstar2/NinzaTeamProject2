using DevLib.ServiceLocator;
using Members.CJY.Scripts;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.GameSystem
{
    // 노드 선택은 StageNodeMaker, 원정 상태와 전투 데이터는 BattleDataStorage가 소유한다.
    public class StageMapController : MonoBehaviour
    {
        [SerializeField] private StageNodeMaker nodeMaker;
        [SerializeField] private TMP_Text stageText, noticeText;
        [SerializeField] private Button restartButton;
        private BattleDataStorage _storage;

        private void Start()
        {
            _storage = ServiceLocator.Get<IBattleDataStorage>().Instance;
            nodeMaker.OnNodeEntered += HandleNodeEntered;
            restartButton.onClick.AddListener(Restart);
            OpenMap();
        }

        private void OpenMap()
        {
            nodeMaker.ConfigureStage(_storage.StageIndex);
            nodeMaker.OpenNode();
            stageText.text = _storage.CurrentStageName;
            noticeText.text = _storage.RunCompleted ? "원정 완료! 모든 보스를 쓰러뜨렸습니다." :
                "이동할 노드를 선택하세요.  ◆ 정예  ·  ♛ 보스";
            restartButton.gameObject.SetActive(_storage.RunCompleted);
        }

        private void HandleNodeEntered(NodeConnect node)
        {
            switch (node.info.type)
            {
                case NodeType.Battle:
                case NodeType.Elite:
                case NodeType.Boss:
                    EnemyRank rank = node.info.type == NodeType.Boss ? EnemyRank.Boss :
                        node.info.type == NodeType.Elite ? EnemyRank.Elite : EnemyRank.Normal;
                    _storage.EnterEncounter(node.column, nodeMaker.LastColumn, rank);
                    break;
                case NodeType.Rest:
                    _storage.Rest();
                    noticeText.text = "휴식으로 생존한 동료의 체력을 30% 회복했습니다.";
                    break;
                default:
                    noticeText.text = "길을 정비했습니다. 다음 노드로 이동하세요.";
                    break;
            }
        }

        public void Restart()
        {
            _storage.RestartRun();
            OpenMap();
        }

        private void OnDestroy()
        {
            nodeMaker.OnNodeEntered -= HandleNodeEntered;
            restartButton.onClick.RemoveListener(Restart);
        }
    }
}
