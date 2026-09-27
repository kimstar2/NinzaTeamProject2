using System.Collections;
using _TevLib.Extension.DoT;
using Members.KJY._01.Scripts.Agent.Player.Dice;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    public class BattleRiskFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerDiceRollManager rollManager;
        [SerializeField] private TMP_Text levelLabel, penaltyLabel, hintLabel;
        [SerializeField] private Image accent, fill;
        [SerializeField] private TweenSequencer increaseMotion;
        [SerializeField] private ParticleSystem sparks;
        private float _risk;
        private Coroutine _warning;

        private void OnEnable() => rollManager.onRiskLevelChanged.AddListener(Refresh);
        private void Start() => Refresh(0f);

        private void OnDisable()
        {
            rollManager.onRiskLevelChanged.RemoveListener(Refresh);
            if (_warning != null) StopCoroutine(_warning);
            _warning = null;
            increaseMotion.Stop();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Refresh(float value)
        {
            value = Mathf.Clamp01(value);
            bool increased = value > _risk;
            _risk = value;
            string level = value >= 1f ? "한계" : value >= 0.7f ? "위험" : value >= 0.35f ? "경계" : "안정";
            levelLabel.text = $"위험도 {Mathf.RoundToInt(value * 100f)}% · {level}";
            penaltyLabel.SetText("적 스킬 위력 ×{0:2}", rollManager.GetRiskPenalty());
            hintLabel.text = value >= 1f ? "최대 위험 · 강화된 적을 상대해야 합니다"
                : "다시 굴릴수록 적 스킬이 강해집니다";
            var color = Color.Lerp(new Color(0.35f, 0.8f, 0.85f), new Color(1f, 0.3f, 0.2f), value);
            accent.color = fill.color = levelLabel.color = color;
            var main = sparks.main;
            main.startColor = color;
            if (increased)
            {
                increaseMotion.Sequence();
                sparks.Play();
            }
            if (value >= 0.7f && _warning == null) _warning = StartCoroutine(Warning());
        }

        private IEnumerator Warning()
        {
            var interval = new WaitForSeconds(1.6f);
            while (true)
            {
                yield return interval;
                if (!increaseMotion.HasTween) increaseMotion.Sequence();
            }
        }
    }
}
