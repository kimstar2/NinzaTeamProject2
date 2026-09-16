using System.Collections.Generic;
using System.Linq;
using Members.LYW.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class FragmentSetter : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    private void Start()
    {
        SetContents();
    }

    public void SetContents()
    {
        List<Transform> children = transform.Cast<Transform>().ToList();

        if (children.Count > 0)
        {
            foreach (Transform child in children)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < inventory.DiceFragments.Count; i++)
            {
                CreateContent(inventory.DiceFragments[i], i);
            }

            return;
        }
        
        for (int i = 0; i < inventory.DiceFragments.Count; i++)
        {
            CreateContent(inventory.DiceFragments[i], i);
        }
    }

    private void CreateContent(DiceFragmentSO fragment, int index)
    {
        GameObject diceFragment = new GameObject()
        {
            name = "DiceFragment",
        };
        diceFragment.transform.SetParent(transform);
        var image = diceFragment.AddComponent<Image>();
        var clickTrigger = diceFragment.AddComponent<DiceFragment>();
        clickTrigger.Init(fragment);
        clickTrigger.SetIndex(index);
        image.sprite = fragment.diceFragmentSprite;
    }
}
