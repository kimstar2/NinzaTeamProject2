using System;
using System.Collections.Generic;
using System.Linq;
using Members.KJY._01.Scripts.Dice.Data;
using Members.LYW.Scripts;
using Members.LYW.Scripts.MySystem.Events;
using Members.LYW.Scripts.System;
using TMPro;
using UnityEngine;
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
    
    [Header("DiceFragments")]
    [SerializeField] private List<DiceDataSO> meleeDiceFragments;
    [SerializeField] private List<DiceDataSO> rangedDiceFragments;
    [SerializeField] private List<DiceDataSO> magicianDiceFragments;
    [SerializeField] private List<DiceDataSO> healerDiceFragments;
    
    private int needyGold = 0;
    private int randNum;

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
            Debug.Log("성공 (선택한 직업군 주사위 면 지급)");
        }
        else
        {
            GiveRandomDiceFragment();
            Debug.Log("실패 (선택한 직업군을 제외한 직업군 중 랜덤 직업군의 주사위 면 지급)");
        }
        
        EventBus.Publish(new UpgradeFragmentEvent());
        DiceFragment.ResetSelectedValue();
        
        fragmentSetter.SetContents();
        
        RefreshValue();
    }

    private void GiveDiceFragmentByJob()
    {
        if (jobSetter.curJob == Job.Melee)
        {
            // 여기에 근접 직업군에 맞는 주사위 면 지급 로직 작성
            Debug.Log("근접 주사위 면 지급됨.");

            inventory.AddFragment(DiceFragmentSetter(meleeDiceFragments));
        }
        if (jobSetter.curJob == Job.Ranged)
        {
            // 여기에 원거리 직업군에 맞는 주사위 면 지급 로직 작성
            Debug.Log("원거리 주사위 면 지급됨.");
            
            inventory.AddFragment(DiceFragmentSetter(rangedDiceFragments));
        }
        if (jobSetter.curJob == Job.Magician)
        {
            // 여기에 마법 직업군에 맞는 주사위 면 지급 로직 작성
            Debug.Log("마법 주사위 면 지급됨.");
            
            inventory.AddFragment(DiceFragmentSetter(magicianDiceFragments));
        }
        if (jobSetter.curJob == Job.Healer)
        {
            // 여기에 힐러 직업군에 맞는 주사위 면 지급 로직 작성
            Debug.Log("힐러 주사위 면 지급됨.");
            
            inventory.AddFragment(DiceFragmentSetter(healerDiceFragments));
        }
    }

    DiceDataSO DiceFragmentSetter(List<DiceDataSO> diceFragments)
    {
        return diceFragments[Random.Range(0, diceFragments.Count)];
    }
    DiceDataSO DiceFragmentSetter(List<DiceDataSO> diceFragments, bool isFailed)
    {
        var candidates = meleeDiceFragments
            .Concat(rangedDiceFragments)
            .Concat(magicianDiceFragments)
            .Concat(healerDiceFragments)
            .Where(x => !diceFragments.Contains(x))
            .ToList();

        return candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : null;
    }
    
    private void GiveRandomDiceFragment()
    {
        if (jobSetter.curJob == Job.Melee)
        {
            // 여기에 근접 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
            Debug.Log("근접제외 주사위 면 지급됨.");
            
            inventory.AddFragment(DiceFragmentSetter(meleeDiceFragments, true));
        }
        if (jobSetter.curJob == Job.Ranged)
        {
            // 여기에 원거리 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
            Debug.Log("원거리제외 주사위 면 지급됨.");
            
            inventory.AddFragment(DiceFragmentSetter(rangedDiceFragments, true));
        }
        if (jobSetter.curJob == Job.Magician)
        {
            // 여기에 마법 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
            Debug.Log("마법제외 주사위 면 지급됨.");
            
            inventory.AddFragment(DiceFragmentSetter(magicianDiceFragments, true));
        }
        if (jobSetter.curJob == Job.Healer)
        {
            // 여기에 힐러 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
            Debug.Log("힐러제외 주사위 면 지급됨.");
            
            inventory.AddFragment(DiceFragmentSetter(healerDiceFragments, true));
        }
    }
}
