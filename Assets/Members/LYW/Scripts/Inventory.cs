using System;
using System.Collections.Generic;
using Members.KJY._01.Scripts.Dice.Data;
using Members.LYW.Scripts;
using Members.PSW.Code.InventorySystem;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxSlots = 50;
    public int MaxSlots => Mathf.Max(1, maxSlots);
    [field : SerializeField] public List<RewardDiceFragmentSO> DiceFragments { get; private set; } = new();

    public event Action Changed;
    protected void NotifyChanged() => Changed?.Invoke();

    public bool AddFragment(RewardDiceFragmentSO fragment)
    {
        if (fragment == null || DiceFragments.Count >= MaxSlots) return false;
        DiceFragments.Add(fragment);
        DiceCatalogProgress.Discover(fragment.DiceData); // 주사위 도감 기록
        Changed?.Invoke();
        return true;
    }

    public void RemoveFragment(RewardDiceFragmentSO fragment)
    {
        if (DiceFragments.Remove(fragment))
            Changed?.Invoke();
    }
}
