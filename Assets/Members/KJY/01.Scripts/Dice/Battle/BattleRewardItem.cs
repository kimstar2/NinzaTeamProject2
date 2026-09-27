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
        [SerializeField] private GameObject selectionMarker;

        public bool IsRevealing => revealMotion.HasTween;

        private void Awake() => canvasGroup.alpha = 0f;

        public void Reveal() => revealMotion.Sequence();

        public void SetSelected(bool selected)
        {
            if (selectionMarker != null) selectionMarker.SetActive(selected);
        }

        private void OnDisable() => revealMotion.Stop();

        public void Bind(RewardDiceFragmentSO reward)
            => Bind(reward.DiceData, reward.Level);

        public void Bind(DiceDataSO face, float level)
        {
            icon.sprite = face.Icon;
            title.text = string.IsNullOrWhiteSpace(face.MainName) ? face.name : face.MainName;
            var grade = face.DiceGrade;
            gradeMark.color = grade != null ? grade.GradeColor : Color.white;
            string gradeName = grade == null ? DiceGradeSO.GetName(DiceGrade.Common) : grade.DisplayName;
            detail.text = $"{gradeName}  ·  Lv.{level:0.#}";
            icon.color = Color.white;
        }

        // 도감에서 아직 얻지 못한 면: 아이콘 실루엣만 진한 회색으로 보여주고 정보는 가린다.
        public void BindLocked(DiceDataSO face)
        {
            icon.sprite = face != null ? face.Icon : null;
            icon.color = new Color(0.16f, 0.16f, 0.16f, 1f);
            title.text = "???";
            gradeMark.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            detail.text = "미획득";
        }
    }
}
