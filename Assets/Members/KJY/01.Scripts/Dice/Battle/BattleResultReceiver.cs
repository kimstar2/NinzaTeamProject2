using System.Collections;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Enemy.Dice;
using Members.KJY._01.Scripts.Events;
using Members.KJY._01.Scripts.Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    // 전투 준비 안내와 결과 표시를 담당한다. 보상 지급은 인벤토리에 맡긴다.
    public class BattleResultReceiver : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private EnemyDiceRollManager enemyRollManager;
        [SerializeField] private DiceBattleManager battleManager;
        [Header("전투 준비 · Life Cycle에서 실행")]
        [SerializeField] private CanvasGroup startBanner;
        [SerializeField] private TMP_Text startTitle;
        [SerializeField] private TMP_Text encounterText;
        [Header("전투 결과")]
        [SerializeField] private CanvasGroup resultPanel;
        [SerializeField] private RectTransform resultCard;
        [SerializeField] private TMP_Text resultTitle;
        [SerializeField] private TMP_Text resultDescription;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private GameObject rewardSection;
        [SerializeField] private RectTransform rewardContent;
        [SerializeField] private BattleRewardItem rewardItemPrefab;
        [SerializeField] private TMP_Text rewardNotice;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text continueLabel;
        [Header("연출 · 속도와 반동은 각 Sequencer에서 조절")]
        [SerializeField] private TweenSequencer bannerMotion;
        [SerializeField] private TweenSequencer backdropInMotion;
        [SerializeField] private TweenSequencer backdropOutMotion;
        [SerializeField] private TweenSequencer victoryMotion;
        [SerializeField] private TweenSequencer defeatMotion;
        [SerializeField] private TweenSequencer closeMotion;

        private BattleDataStorage _storage;
        private BattleInventory _inventory;
        private bool _hasResult;
        private bool _canContinue;
        private bool _victory;

        private void Awake()
        {
            startBanner.alpha = 0f;
            startBanner.blocksRaycasts = false;
            resultPanel.alpha = 0f;
            resultPanel.gameObject.SetActive(false);
            continueButton.onClick.AddListener(Continue);
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnBattleResult>(HandleBattleResult);
        }

        private void Start()
        {
            _storage = ServiceLocator.Get<IBattleDataStorage>().Instance;
            ServiceLocator.TryGet<Inventory>(out var inventory);
            _inventory = inventory as BattleInventory;
            if (_inventory == null)
            {
                Debug.LogError("BattleResultReceiver: 전투 보상 인벤토리가 필요합니다.", this);
                enabled = false;
                return;
            }
            _inventory.BeginEncounter();
            var encounter = _storage.GetBattleData();
            encounterText.text = $"{encounter.StageName}  ·  {encounter.EncounterLabel}";
        }

        // 준비 안내의 실행 시점은 씬의 WaitOnPlay 순서에서 정한다.
        public void ShowPreparation()
        {
            if (_hasResult || !isActiveAndEnabled) return;
            bannerMotion.Stop();
            startTitle.text = "전투 준비";
            startBanner.alpha = 0f;
            bannerMotion.Sequence();
        }

        private void HandleBattleResult(OnBattleResult evt)
        {
            if (_hasResult) return;
            _hasResult = true;
            _victory = evt.BattleResult == BattleResult.PlayerWon;
            bannerMotion.Stop();
            startBanner.alpha = 0f;
            StartCoroutine(ShowResult());
        }

        private IEnumerator ShowResult()
        {
            // 사망 알림보다 주사위 보상 확정이 늦다. 그동안 추가 입력만 막는다.
            resultPanel.gameObject.SetActive(true);
            resultPanel.blocksRaycasts = true;
            resultPanel.interactable = false;
            continueButton.interactable = false;
            yield return null;
            yield return new WaitUntil(() => !battleManager.IsBattle &&
                (!_victory || enemyRollManager.AllDiceRollEnd));

            _inventory.CompleteEncounter(_victory, _storage.GetBattleData().GoldReward);
            bool finalClear = _victory && _storage.IsFinalEncounter;
            resultTitle.text = _victory ? finalClear ? "모험 완료" : "전투 승리" : "전투 패배";
            resultTitle.color = _victory ? new Color(1f, 0.83f, 0.46f) : new Color(0.93f, 0.52f, 0.5f);
            resultDescription.text = _victory
                ? finalClear ? "두 스테이지의 마지막 적을 물리쳤습니다." : "길을 가로막던 적을 물리쳤습니다."
                : "잠시 숨을 고르고 다시 도전해 보세요.";
            rewardSection.SetActive(_victory);
            goldText.text = $"+ {_inventory.GoldEarned} G   <size=70%>보유 {_inventory.Gold} G</size>";
            var rewardItems = new List<BattleRewardItem>();
            foreach (var reward in _inventory.EncounterRewards)
            {
                var item = Instantiate(rewardItemPrefab, rewardContent);
                item.Bind(reward);
                rewardItems.Add(item);
            }
            rewardNotice.text = _inventory.SkippedRewards > 0
                ? $"가방이 가득 차 주사위 {_inventory.SkippedRewards}개를 담지 못했습니다."
                : _inventory.EncounterRewards.Count == 0 ? "획득한 주사위가 없습니다." : $"주사위 {_inventory.EncounterRewards.Count}개를 가방에 보관했습니다.";
            continueLabel.text = _victory ? finalClear ? "모험 마무리" : "다음으로" : "다시 도전하기";
            if (!_victory)
            {
                resultCard.sizeDelta = new Vector2(resultCard.sizeDelta.x, 350f);
                resultTitle.rectTransform.anchoredPosition = new Vector2(0, 100f);
                resultDescription.rectTransform.anchoredPosition = new Vector2(0, 25f);
                ((RectTransform)continueButton.transform).anchoredPosition = new Vector2(0, -95f);
            }

            TweenSequencer openMotion = _victory ? victoryMotion : defeatMotion;
            backdropInMotion.Sequence();
            openMotion.Sequence();

            // 카드가 펼쳐지는 중에 보상이 하나씩 붙는다. 레이아웃 위치는 건드리지 않는다.
            if (rewardItems.Count > 0)
            {
                yield return new WaitForSecondsRealtime(0.18f);
                for (int i = 0; i < rewardItems.Count; i++)
                {
                    rewardItems[i].Reveal();
                    if (i < rewardItems.Count - 1)
                        yield return new WaitForSecondsRealtime(0.065f);
                }
            }
            yield return new WaitUntil(() => !openMotion.HasTween && !backdropInMotion.HasTween &&
                rewardItems.TrueForAll(item => !item.IsRevealing));
            _canContinue = true;
            resultPanel.interactable = true;
            continueButton.interactable = true;
        }

        private void Continue()
        {
            if (!_canContinue) return;
            _canContinue = false;
            continueButton.interactable = false;
            resultPanel.interactable = false;
            StartCoroutine(CloseResult());
        }

        private IEnumerator CloseResult()
        {
            closeMotion.Sequence();
            backdropOutMotion.Sequence();
            // 입력은 한 번만 받고, 접히는 모션이 끝난 뒤에 씬을 이동한다.
            yield return new WaitUntil(() => !closeMotion.HasTween && !backdropOutMotion.HasTween);
            _storage.CompleteEncounter(_victory);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnBattleResult>(HandleBattleResult);
            StopAllCoroutines();
            bannerMotion.Stop();
            backdropInMotion.Stop();
            backdropOutMotion.Stop();
            victoryMotion.Stop();
            defeatMotion.Stop();
            closeMotion.Stop();
        }

        private void OnDestroy()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(Continue);
        }
    }
}
