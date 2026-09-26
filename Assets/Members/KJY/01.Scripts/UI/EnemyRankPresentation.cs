using Members.KJY._01.Scripts.Agent.Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    // 체력/스킬에는 관여하지 않고 등급의 크기, 얇은 잔광, 정보 패널만 표현한다.
    public class EnemyRankPresentation : MonoBehaviour
    {
        [SerializeField] private EnemySelector selector;
        [SerializeField] private SpriteRenderer agentSprite, rankGlow;
        [SerializeField] private TMP_Text enemyName, rankLabel;
        [SerializeField] private Image rankBand;
        private Vector3 _originalScale;
        private EnemyRank _rank;
        private Color _accent;

        private void Awake() => _originalScale = agentSprite.transform.localScale;

        public void Apply(EnemyDataSO data)
        {
            _rank = data != null ? data.Rank : EnemyRank.Normal;
            bool marked = _rank != EnemyRank.Normal;
            _accent = _rank == EnemyRank.Boss ? new Color(0.75f, 0.31f, 0.27f) : new Color(0.81f, 0.67f, 0.36f);
            float scale = _rank == EnemyRank.Boss ? 1.3f : _rank == EnemyRank.Elite ? 1.12f : 1f;
            agentSprite.transform.localScale = _originalScale * scale;
            enemyName.text = data != null ? data.EnemyName : string.Empty;
            enemyName.color = marked ? Color.Lerp(_accent, Color.white, 0.55f) : new Color(0.85f, 0.85f, 0.85f);
            rankLabel.text = _rank == EnemyRank.Boss ? "BOSS" : _rank == EnemyRank.Elite ? "ELITE" : string.Empty;
            rankLabel.gameObject.SetActive(marked);
            rankBand.gameObject.SetActive(marked);
            rankBand.color = _accent;
            rankGlow.enabled = marked;
        }

        private void LateUpdate()
        {
            bool visible = _rank != EnemyRank.Normal && !selector.IsDead && !selector.IsNoneData;
            rankGlow.enabled = visible;
            if (!visible) return;
            rankGlow.sprite = agentSprite.sprite;
            rankGlow.flipX = agentSprite.flipX;
            rankGlow.flipY = agentSprite.flipY;
            // 원본 그림 바로 뒤에 약한 한 겹만 둬서 피격 섬광과 섞이지 않게 한다.
            Color color = _accent;
            color.a = 0.19f + Mathf.Sin(Time.time * 2.3f) * 0.055f;
            rankGlow.color = color;
        }

        private void OnDisable()
        {
            if (rankGlow != null) rankGlow.enabled = false;
        }
    }
}
