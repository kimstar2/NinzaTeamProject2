using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [field : SerializeField] public List<DiceFragmentSO> DiceFragments { get; private set; } = new();

    public void RemoveFragment(DiceFragmentSO fragment)
    {
        DiceFragments.Remove(fragment);
    }
}
