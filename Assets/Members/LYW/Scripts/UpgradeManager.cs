using System.Collections;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Dice.Battle;
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
    private int _slot;
    private bool _busy;

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
        bool revealResult = !resultCard.gameObject.activeSelf;
        resultCard.gameObject.SetActive(hasTarget);
        resultTitle.text = "제련 후";
        currentDescription.text = hasTarget ? Describe(target, target.Level) : "기준 면을 고르세요.\n\n스킬과 등급을 유지한 채 레벨을 올립니다.";
        resultDescription.text = hasTarget ? Describe(target, _inventory.GetForgedLevel(target.Level)) : "강화 후의 스킬 수치를\n여기서 비교할 수 있습니다.";
        if (hasTarget)
        {
            resultCard.Bind(target.DiceData, _inventory.GetForgedLevel(target.Level));
            if (revealResult) resultCard.Reveal();
        }
        goldText.text = _inventory != null ? $"보유 골드  {_inventory.Gold} G" : "모험을 시작한 뒤 이용할 수 있습니다.";
        costText.text = hasTarget ? $"제련하기  ·  {_inventory.GetForgeCost(target.Level)} G" : "제련하기";
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
        resultTitle.text = "제련 완료";
        resultDescription.text = Describe(_selected[0], _selected[0].Level);
        notice.text = $"{_selected[0].DiceData.MainName} · Lv.{_selected[0].Level:0.#} 제련 완료. 면공방에서 장착할 수 있습니다.";
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
