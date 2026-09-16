using System;
using Members.LYW.Scripts;
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
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI percentText;
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
        upgradeText.SetText(needyGold.ToString($"|합성하기 - 필요 금액 : {needyGold}G|"));
        percentText.SetText(needyGold.ToString($"성공 확률 : {randNum}%"));
    }

    private void RefreshValue()
    {
        randNum = Random.Range(0, 99);
        needyGold *= 2;
        
        upgradeText.SetText(needyGold.ToString($"|합성하기 - 필요 금액 : {needyGold}G|"));
        percentText.SetText(needyGold.ToString($"성공 확률 : {randNum}%"));
    }
    
    public void TryUpgrade()
    {
        // 플레이어 소지 금액이 needyGold 보다 작다면 return; 로직 작성
        /*if ( ??? < needyGold)
        {
            return false;
        }*/
        
        if (randNum is >= 50 and <= 99)
        {
            RefreshValue();
            
            GiveDiceFragmentByJob();
            Debug.Log("성공");
            return;
        }

        RefreshValue();
        GiveRandomDiceFragment();
        Debug.Log("실패");
    }

    private void GiveDiceFragmentByJob()
    {
        if (jobSetter.curJob == Job.Melee)
        {
            // 여기에 근접 직업군에 맞는 주사위 면 지급 로직 작성
        }
        if (jobSetter.curJob == Job.Ranged)
        {
            // 여기에 원거리 직업군에 맞는 주사위 면 지급 로직 작성
        }
        if (jobSetter.curJob == Job.Magician)
        {
            // 여기에 마법 직업군에 맞는 주사위 면 지급 로직 작성
        }
        if (jobSetter.curJob == Job.Healer)
        {
            // 여기에 힐러 직업군에 맞는 주사위 면 지급 로직 작성
        }
    }

    private void GiveRandomDiceFragment()
    {
        if (jobSetter.curJob == Job.Melee)
        {
            // 여기에 근접 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
        }
        if (jobSetter.curJob == Job.Ranged)
        {
            // 여기에 원거리 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
        }
        if (jobSetter.curJob == Job.Magician)
        {
            // 여기에 마법 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
        }
        if (jobSetter.curJob == Job.Healer)
        {
            // 여기에 힐러 직업군을 제외한 직업군 중의 주사위 면 지급 로직 작성
        }
    }
}
