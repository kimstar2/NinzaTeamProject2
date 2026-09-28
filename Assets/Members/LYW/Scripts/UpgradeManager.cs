using System.Collections;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.UI;
using Members.PSW.Code.InventorySystem;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Job { Melee, Ranged, Magician, Healer }

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot, choiceRoot, emptyBagHint;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private BattleRewardItem rewardPrefab, resultCard;
    [SerializeField] private BattleRewardItem[] slotCards;
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private GameObject[] emptySlotHints;
    [SerializeField] private Transform rewardParent;
    [SerializeField] private TMP_Text goldText, currentDescription, resultDescription, notice, costText, resultTitle;
    [SerializeField] private Button forgeButton;
    [SerializeField] private TweenSequencer openMotion, closeMotion, forgeMotion;
    [SerializeField, Scene] private string returnScene;
    private readonly RewardDiceFragmentSO[] _selected = new RewardDiceFragmentSO[3];
    private readonly List<BattleRewardItem> _items = new();
    private BattleInventory _inventory;
    [SerializeField, Tooltip("[재련 정보] 버튼과 설명창. 지도 복귀 버튼 왼쪽에 자동 배치")] private ForgeTutorial tutorialPrefab;
    private int _slot;
    private bool _busy;
    private bool _lastGradeUp;

    private void Start()
    {
        ServiceLocator.TryGet<Inventory>(out var inventory);
        _inventory = inventory as BattleInventory;
        if (_inventory != null) _inventory.Changed += Refresh;
        goldText.text = _inventory != null ? $"보유 골드  {_inventory.Gold} G" : "모험을 시작한 뒤 이용할 수 있습니다.";
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i;
            slotButtons[i].onClick.AddListener(() => SelectSlot(slot));
        }
        var tutorial = tutorialPrefab != null ? tutorialPrefab : Resources.Load<ForgeTutorial>("ForgeTutorial");
        if (tutorial != null)
        {
            var instance = Instantiate(tutorial, panelRoot.transform, false);
            var mapButton = FindMapButton();
            if (mapButton != null) instance.PlaceHelpButtonLeftOf((RectTransform)mapButton.transform);
        }
    }

    private Button FindMapButton()
    {
        foreach (var button in panelRoot.GetComponentsInChildren<Button>(true))
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == nameof(Close))
                    return button;
        return null;
    }

    public void Open()
    {
        choiceRoot.SetActive(false);
        panelRoot.SetActive(true);
        panelGroup.interactable = true;
        Refresh();
        openMotion.Sequence();
    }

    public void SelectSlot(int index)
    {
        if (_busy) return;
        _slot = index;
        RefreshSelection();
    }

    private void SelectFace(RewardDiceFragmentSO reward)
    {
        if (_busy) return;
        int previous = System.Array.IndexOf(_selected, reward);
        if (previous >= 0)
        {
            _selected[previous] = null;
            _slot = previous;
        }
        else
        {
            _selected[_slot] = reward;
            int empty = System.Array.IndexOf(_selected, null);
            if (empty >= 0) _slot = empty;
        }
        RefreshSelection();
    }

    private void Refresh()
    {
        if (_busy || !panelRoot.activeSelf) return;
        foreach (var item in _items) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
        _items.Clear();
        if (_inventory != null)
        {
            foreach (var reward in _inventory.DiceFragments)
            {
                if (reward == null || reward.DiceData == null) continue;
                var item = Instantiate(rewardPrefab, rewardParent);
                item.Bind(reward);
                item.GetComponent<Button>().onClick.AddListener(() => SelectFace(reward));
                item.Reveal();
                _items.Add(item);
            }
            for (int i = 0; i < _selected.Length; i++)
                if (!_inventory.DiceFragments.Contains(_selected[i])) _selected[i] = null;
        }
        emptyBagHint.SetActive(_items.Count == 0);
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < slotCards.Length; i++)
        {
            bool reveal = !slotCards[i].gameObject.activeSelf;
            emptySlotHints[i].SetActive(_selected[i] == null);
            slotCards[i].gameObject.SetActive(_selected[i] != null);
            if (_selected[i] == null) continue;
            slotCards[i].Bind(_selected[i]);
            slotCards[i].SetSelected(i == _slot);
            if (reveal) slotCards[i].Reveal();
        }
        int itemIndex = 0;
        if (_inventory != null)
            foreach (var reward in _inventory.DiceFragments)
            {
                if (reward == null || reward.DiceData == null) continue;
                _items[itemIndex++].SetSelected(System.Array.IndexOf(_selected, reward) >= 0);
            }
        var target = _selected[0];
        bool hasTarget = target != null && _inventory != null;
        var plan = hasTarget ? _inventory.GetForgePlan(_selected) : BattleInventory.ForgePlan.Invalid(string.Empty);
        bool gradeUp = plan.Mode == BattleInventory.ForgeMode.GradeUp;
        float resultLevel = plan.Mode == BattleInventory.ForgeMode.Enhance ? plan.ResultLevel : hasTarget ? target.Level : 1f;
        bool revealResult = !resultCard.gameObject.activeSelf;
        resultCard.gameObject.SetActive(hasTarget && !gradeUp);
        resultTitle.text = gradeUp ? "등급 상승" : "재련 후";
        currentDescription.text = hasTarget ? Describe(target, target.Level) : "기준 면을 고르세요.\n\n재료 면의 등급에 따라 레벨이 오르거나 등급이 오릅니다.";
        resultDescription.text = !hasTarget ? "강화 후의 스킬 수치를\n여기서 비교할 수 있습니다." :
            gradeUp ? $"{DiceGradeSO.GetName(plan.ResultGrade)} 등급 스킬로 바뀝니다.\n\n적합 직업은 유지되고 Lv.1부터 시작합니다.\n좋은 재료를 넣을수록 강한 스킬이 나오기 쉽습니다." :
            Describe(target, resultLevel);
        if (hasTarget && !gradeUp)
        {
            resultCard.Bind(target.DiceData, resultLevel);
            if (revealResult) resultCard.Reveal();
        }
        goldText.text = _inventory != null ? $"보유 골드  {_inventory.Gold} G" : "모험을 시작한 뒤 이용할 수 있습니다.";
        string action = gradeUp ? "등급 상승" : "재련하기";
        int cost = plan.Mode != BattleInventory.ForgeMode.None ? plan.Cost : hasTarget ? _inventory.GetForgeCost(target.Level) : 0;
        costText.text = hasTarget ? $"{action}  ·  {cost} G" : action;
        string reason = "전투에서 획득한 면 3개가 필요합니다.";
        forgeButton.interactable = _inventory != null && _inventory.CanForge(_selected, out reason);
        notice.text = forgeButton.interactable ? reason : (_slot == 0 ? "기준 면" : $"재료 {_slot}") + " 선택 중 · " + reason;
    }

    private static string Describe(RewardDiceFragmentSO face, float level)
        => $"{face.DiceData.MainName}  Lv.{level:0.#}\n\n{face.DiceData.GetDescription(level)}";

    public void TryUpgrade()
    {
        if (_busy || _inventory == null) return;
        _busy = true;
        _lastGradeUp = _inventory.GetForgePlan(_selected).Mode == BattleInventory.ForgeMode.GradeUp;
        if (!_inventory.TryForge(_selected, out var reason))
        {
            _busy = false;
            Refresh();
            notice.text = reason;
            return;
        }
        StartCoroutine(ShowResult());
    }

    private IEnumerator ShowResult()
    {
        panelGroup.interactable = false;
        forgeButton.interactable = false;
        _selected[1] = _selected[2] = null;
        forgeMotion.Sequence();
        yield return new WaitUntil(() => !forgeMotion.HasTween);
        _busy = false;
        panelGroup.interactable = true;
        _slot = 1;
        Refresh();
        resultCard.Bind(_selected[0]);
        resultCard.Reveal();
        var face = _selected[0].DiceData;
        resultTitle.text = _lastGradeUp ? "등급 상승 완료" : "재련 완료";
        resultDescription.text = Describe(_selected[0], _selected[0].Level);
        notice.text = _lastGradeUp
            ? $"{face.MainName} ({(face.DiceGrade != null ? face.DiceGrade.DisplayName : "일반")}) 스킬로 바뀌었습니다. 면공방에서 장착할 수 있습니다."
            : $"{face.MainName} · Lv.{_selected[0].Level:0.#} 재련 완료. 면공방에서 장착할 수 있습니다.";
    }

    public void Close() { if (!_busy) StartCoroutine(Leave()); }
    private IEnumerator Leave()
    {
        _busy = true;
        panelGroup.interactable = false;
        openMotion.Stop();
        closeMotion.Sequence();
        yield return new WaitUntil(() => !closeMotion.HasTween);
        ReturnToMap();
    }

    public void ReturnToMap() => SceneTransition.Load(returnScene);

    private void OnDestroy()
    {
        if (_inventory != null) _inventory.Changed -= Refresh;
    }
}
