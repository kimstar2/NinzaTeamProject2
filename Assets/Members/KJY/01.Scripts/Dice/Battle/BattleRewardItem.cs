using _TevLib.Extension.DoT;
using Members.KJY._01.Scripts.Dice.Data;
using Members.PSW.Code.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class BattleRewardItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image gradeMark;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text detail;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TweenSequencer revealMotion;

        public bool IsRevealing => revealMotion.HasTween;

        private void Awake() => canvasGroup.alpha = 0f;

        public void Reveal() => revealMotion.Sequence();

        private void OnDisable() => revealMotion.Stop();

        public void Bind(RewardDiceFragmentSO reward)
        {
            icon.sprite = reward.Icon;
            title.text = reward.SkillData != null ? reward.SkillData.SkillName : reward.name;
            var grade = reward.DiceData.DiceGrade;
            gradeMark.color = grade != null ? grade.GradeColor : Color.white;
            string gradeName = grade == null ? "일반" : grade.Grade switch
            {
                DiceGrade.Uncommon => "고급",
                DiceGrade.Rare => "희귀",
                _ => "일반"
            };
            detail.text = $"{gradeName}  ·  Lv.{reward.Level:0.#}";
        }
    }
}
