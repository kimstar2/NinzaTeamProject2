using System.Collections;
using System.Text;
using _TevLib.Extension.DoT;
using Coffee.UIEffects;
using Members.KJY._01.Scripts.Agent.Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    public class BattleNodePanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private TMP_Text title, description, reward;
        [SerializeField] private Image[] portraits;
        [SerializeField] private Image panelBackground;
        [SerializeField] private UIEffect headerEffect, frameEffect;
        [SerializeField] private TweenSequencer openMotion, closeMotion;
        [SerializeField] private UnityEvent onConfirm, onCancel;
        private bool _closing;
        private Image _frame;
        private Color _backgroundColor, _frameColor, _headerTop, _headerBottom, _frameBottom, _frameShadow;

        private void Awake()
        {
            _frame = frameEffect.GetComponent<Image>();
            _backgroundColor = panelBackground.color;
            _frameColor = _frame.color;
            _headerTop = headerEffect.gradationColor1;
            _headerBottom = headerEffect.gradationColor2;
            _frameBottom = frameEffect.gradationColor2;
            _frameShadow = frameEffect.shadowColor;
            panelRoot.SetActive(false);
        }

        public void Show(BattleData data, string stageName)
        {
            _closing = false;
            bool elite = data.rank == EnemyRank.Elite;
            panelBackground.color = elite ? new Color(0.02f, 0.04f, 0.06f) : _backgroundColor;
            _frame.color = elite ? new Color(0.5f, 0.84f, 1f) : _frameColor;
            headerEffect.gradationColor1 = elite ? new Color(0.26f, 0.65f, 0.82f) : _headerTop;
            headerEffect.gradationColor2 = elite ? new Color(0.04f, 0.28f, 0.43f) : _headerBottom;
            frameEffect.gradationColor2 = elite ? new Color(0.2f, 0.45f, 0.65f) : _frameBottom;
            frameEffect.shadowColor = elite ? new Color(0.3f, 0.7f, 1f, 0.45f) : _frameShadow;
            string rank = data.rank == EnemyRank.Boss ? "보스 전투" : data.rank == EnemyRank.Elite ? "정예 전투" : "전투";
            title.text = $"{stageName} · {rank}";
            var text = new StringBuilder();
            for (int i = 0; i < data.enemies.Length; i++)
            {
                var enemy = data.enemies[i];
                float rankHealth = i != 0 ? 1f : data.rank == EnemyRank.Boss ? 3f : data.rank == EnemyRank.Elite ? 1.5f : 1f;
                text.AppendLine($"{enemy.EnemyName}   ·   체력 {enemy.EncounterHealth * data.healthMultiplier * rankHealth:0}");
            }
            description.text = text.ToString();
            reward.text = $"획득 골드 {data.gold} G\n적을 처치하면 주사위 면을 획득합니다.";
            for (int i = 0; i < portraits.Length; i++)
            {
                portraits[i].gameObject.SetActive(i < data.enemies.Length);
                if (i < data.enemies.Length) portraits[i].sprite = data.enemies[i].EnemyImage;
            }
            panelRoot.SetActive(true);
            panelGroup.interactable = true;
            openMotion.Sequence();
        }

        public void Confirm() { if (!_closing) StartCoroutine(Close(true)); }
        public void Cancel() { if (!_closing) StartCoroutine(Close(false)); }

        private IEnumerator Close(bool confirm)
        {
            _closing = true;
            panelGroup.interactable = false;
            openMotion.Stop();
            closeMotion.Sequence();
            yield return new WaitUntil(() => !closeMotion.HasTween);
            panelRoot.SetActive(false);
            if (confirm) onConfirm.Invoke(); else onCancel.Invoke();
        }
    }
}
