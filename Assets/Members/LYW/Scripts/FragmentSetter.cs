using System.Collections.Generic;
using System.Linq;
using Members.LYW.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class FragmentSetter : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private List<DiceFragment> diceFragments = new();

    private void Start()
    {
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
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        diceFragments.Clear();

        for (int i = 0; i < inventory.DiceFragments.Count; i++)
        {
            CreateContent(inventory.DiceFragments[i], i);
        }
    }

    private void CreateContent(DiceFragmentSO fragment, int index)
    {
        GameObject diceFragment = new GameObject("DiceFragment");

        diceFragment.transform.SetParent(transform);

        var image = diceFragment.AddComponent<Image>();
        var clickTrigger = diceFragment.AddComponent<DiceFragment>();

        clickTrigger.Init(fragment);
        clickTrigger.SetIndex(index);

        image.sprite = fragment.diceFragmentSprite;

        diceFragments.Add(clickTrigger);
    }
}