using System.Collections.Generic;
using System.Linq;
using Members.KJY._01.Scripts.Dice.Data;
using Members.LYW.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class FragmentSetter : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private List<DiceFragment> diceFragments = new();
    [SerializeField] private bool allowSelection = true;
    [SerializeField] private RectTransform slotFramePrefab;
    private bool _hasContents;
    public event System.Action<DiceDataSO, RectTransform> FragmentClicked;

    private void Start()
    {
        if (!_hasContents) SetContents();
    }

    public void SetInventory(Inventory source)
    {
        inventory = source;
        SetContents();
    }

    public void RemoveSelectedDiceFragment()
    {
        var selectedDices = diceFragments
            .Where(x => x != null && x.isSelected)
            .ToList();

        foreach (var selectedDice in selectedDices)
        {
            inventory.RemoveFragment(selectedDice._fragment);
        }
    }

    public void SetContents()
    {
        if (inventory == null) return;
        _hasContents = true;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        diceFragments.Clear();

        int slotCount = inventory.DiceFragments.Count;
        if (slotFramePrefab != null)
        {
            // Preserve existing items if capacity is reduced in the Inspector.
            slotCount = Mathf.Max(slotCount, inventory.MaxSlots);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = slotFramePrefab != null
                ? Instantiate(slotFramePrefab, transform, false)
                : transform;
            if (i < inventory.DiceFragments.Count && inventory.DiceFragments[i] != null)
                CreateContent(inventory.DiceFragments[i], i, slot);
        }
    }

    private void CreateContent(DiceDataSO fragment, int index, Transform parent)
    {
        GameObject diceFragment = new GameObject("DiceFragment");

        diceFragment.transform.SetParent(parent, false);

        var image = diceFragment.AddComponent<Image>();
        if (slotFramePrefab != null)
        {
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = new Vector2(6f, 6f);
            image.rectTransform.offsetMax = new Vector2(-6f, -6f);
        }
        var clickTrigger = diceFragment.AddComponent<DiceFragment>();

        clickTrigger.Init(fragment);
        clickTrigger.SetIndex(index);

        image.sprite = fragment.Icon;
        image.preserveAspect = true;
        clickTrigger.enabled = allowSelection;
        if (!allowSelection)
        {
            var button = diceFragment.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => FragmentClicked?.Invoke(fragment, image.rectTransform));
        }

        diceFragments.Add(clickTrigger);
    }
}
