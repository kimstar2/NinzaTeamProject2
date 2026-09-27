using System;
using System.Collections.Generic;
using System.Linq;
using Members.KJY._01.Scripts.Dice.Data;
using Members.LYW.Scripts;
using Members.LYW.Scripts.MySystem.Events;
using Members.LYW.Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum Job
{
    Melee,
    Ranged,
    Magician,
    Healer
}
public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private JobSetter jobSetter;
    [SerializeField] private FragmentSetter fragmentSetter;
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private Inventory inventory;

    [Header("DiceGrade")]
    [SerializeField] private DiceGradeSO common;
    [SerializeField] private DiceGradeSO unCommon;
    [SerializeField] private DiceGradeSO rare;
    
    [Header("DiceFragments")]
    [SerializeField] private List<DiceDataSO> allDice;
    [SerializeField] private List<DiceDataSO> commonDice;
    [SerializeField] private List<DiceDataSO> unCommonDice;
    [SerializeField] private List<DiceDataSO> rareDice;
    
    private int needyGold = 0;
    private int randNum;

    private void Awake()
    {
        foreach (var dice in allDice)
        {
            if (dice.DiceGrade == common)
                commonDice.Add(dice);
            else  if (dice.DiceGrade == unCommon)
                unCommonDice.Add(dice);
            else if (dice.DiceGrade == rare)
                rareDice.Add(dice);
            else
            {
                Debug.Log("뭔등급이죠?");
            }
        }
    }

    private void Start()
    {
        InitValue();
    }

    private void InitValue()
    {
        needyGold = 1;
        randNum = Random.Range(0, 99);
        upgradeText.SetText($"|합성하기 - 필요 금액 : {needyGold}G|");
        percentText.SetText($"성공 확률 : {randNum}%");
    }

    private void RefreshValue()
    {
        randNum = Random.Range(0, 99);
        needyGold *= 2;
        
        upgradeText.SetText($"|합성하기 - 필요 금액 : {needyGold}G|");
        percentText.SetText($"성공 확률 : {randNum}%");
    }
    
    public void TryUpgrade()
    {
        // 플레이어 소지 금액이 needyGold 보다 작다면 return; 로직 작성
        /*if ( ??? < needyGold)
        {
            return false;
        }*/
        
        bool isSuccess = randNum >= 50;
        
        fragmentSetter.RemoveSelectedDiceFragment();
        
        if (isSuccess)
        {
            GiveDiceFragmentByJob();
            Debug.Log("성공 (좋은 등급 나올 확률 상승)");
        }
        else
        {
            GiveRandomDiceFragment();
            Debug.Log("실패 (안좋은 등급 나올 확률 상승)");
        }
        
        EventBus.Publish(new UpgradeFragmentEvent());
        DiceFragment.ResetSelectedValue();
        
        fragmentSetter.SetContents();
        
        RefreshValue();
    }

    private void GiveDiceFragmentByJob() // 성공
    {
        float random =  Random.Range(0, 100);
        if (random <= 10)
            inventory.AddFragment(DiceFragmentSetter(rareDice));
        else if (random <= 40)
            inventory.AddFragment(DiceFragmentSetter(unCommonDice));
        else
            inventory.AddFragment(DiceFragmentSetter(commonDice));
    }

    DiceDataSO DiceFragmentSetter(List<DiceDataSO> diceFragments)
    {
        return diceFragments[Random.Range(0, diceFragments.Count)];
    }
    DiceDataSO DiceFragmentSetter(List<DiceDataSO> diceFragments, bool isFailed)
    {
        var candidates = commonDice
            .Concat(unCommonDice)
            .Concat(rareDice)
            .Where(x => !diceFragments.Contains(x))
            .ToList();

        return candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : null;
    }
    
    private void GiveRandomDiceFragment()
    {
        float random =  Random.Range(0, 100);
        if (random <= 20)
            inventory.AddFragment(DiceFragmentSetter(commonDice));
        else if (random <= 50)
            inventory.AddFragment(DiceFragmentSetter(unCommonDice));
        else
            inventory.AddFragment(DiceFragmentSetter(rareDice));
    }
}
