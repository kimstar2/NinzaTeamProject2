using System;
using System.Collections.Generic;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxSlots = 50;
    public int MaxSlots => Mathf.Max(1, maxSlots);
    [field : SerializeField] public List<DiceDataSO> DiceFragments { get; private set; } = new();

    public event Action Changed;

    public bool AddFragment(DiceDataSO fragment)
    {
        if (fragment == null || DiceFragments.Count >= MaxSlots) return false;
        DiceFragments.Add(fragment);
        Changed?.Invoke();
        return true;
    }

    public void RemoveFragment(DiceDataSO fragment)
    {
        if (DiceFragments.Remove(fragment))
            Changed?.Invoke();
    }
}
