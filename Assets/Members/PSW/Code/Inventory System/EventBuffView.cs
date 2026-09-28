using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Members.KJY._01.Scripts.Agent.Player.Dice;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Events.Dice;
using DG.Tweening;

namespace Members.PSW.Code.InventorySystem
{
    // Presentation of the party's persistent event effects, separate from combat rules.
    public sealed class EventBuffView : MonoBehaviour
    {
        [SerializeField] private string battleScenePath;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text label;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField, Min(0f)] private float fadeDuration = 0.3f;
        private bool _entranceStarted;
        private Tween _entranceTween;
        private EventDamageModifiers _effects;
        private EventGoldModifiers _gold;
        private PlayerDiceRollManager _rollManager;

        public void BindRewards(EventGoldModifiers gold, PlayerDiceRollManager rollManager)
        {
            if (_gold != null) _gold.Changed -= Refresh;
            if (_rollManager != null) _rollManager.EventRerollLockChanged -= Refresh;
            _gold = gold;
            _rollManager = rollManager;
            if (_gold != null) _gold.Changed += Refresh;
            if (_rollManager != null) _rollManager.EventRerollLockChanged += Refresh;
            Refresh();
        }
        private readonly StringBuilder _text = new();

        public void Bind(EventDamageModifiers effects)
        {
            if (_effects != null) _effects.Changed -= Refresh;
            _effects = effects;
            if (_effects != null && isActiveAndEnabled) _effects.Changed += Refresh;
            Refresh();
        }

        private void OnEnable()
        {
            ResetEntrance();
            if (eventChannel != null) eventChannel.AddListener<OnPlayerRoll>(HandleFirstRoll);
            if (eventChannel != null) eventChannel.AddListener<OnStartBattle>(HandleAttackStarted);
            SceneManager.activeSceneChanged += HandleSceneChanged;
            if (_effects != null)
            {
                _effects.Changed -= Refresh;
                _effects.Changed += Refresh;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (eventChannel != null) eventChannel.RemoveListener<OnPlayerRoll>(HandleFirstRoll);
            if (eventChannel != null) eventChannel.RemoveListener<OnStartBattle>(HandleAttackStarted);
            ResetEntrance();
            BindRewards(null, null);
            SceneManager.activeSceneChanged -= HandleSceneChanged;
            if (_effects != null) _effects.Changed -= Refresh;
            if (panel != null) panel.SetActive(false);
        }

        private void HandleSceneChanged(Scene previous, Scene next)
        {
            ResetEntrance();
            Refresh();
        }

        private void ResetEntrance()
        {
            _entranceTween?.Kill();
            _entranceTween = null;
            _entranceStarted = false;
            if (panelGroup != null) panelGroup.alpha = 0f;
        }

        private void HandleFirstRoll(OnPlayerRoll evt)
        {
            if (_entranceStarted || SceneManager.GetActiveScene().path != battleScenePath) return;
            // The battle's opening sequence rolls dice and reveals the party UI in the same step.
            _entranceStarted = true;
            Refresh();
            if (panelGroup != null)
                _entranceTween = panelGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
        }

        private void HandleAttackStarted(OnStartBattle evt)
        {
            ResetEntrance();
            Refresh();
        }

        private void Refresh()
        {
            if (panel == null || label == null) return;
            _text.Clear();
            if (_effects != null && isActiveAndEnabled &&
                SceneManager.GetActiveScene().path == battleScenePath)
            {
                foreach (var effect in _effects.GetEffects())
                {
                    AppendEffect("가하는 피해", effect.outgoingPercent, effect.remainingBattles, true);
                    AppendEffect("받는 피해", effect.incomingPercent, effect.remainingBattles, false);
                }
                if (_gold != null)
                    foreach (var effect in _gold.Effects)
                        AppendEffect("승리 골드", effect.percent, effect.battles, true);
                if (_rollManager != null && _rollManager.IsEventRerollLocked)
                    _text.Append("<color=#FF9A9A>쇼타임! · 첫 턴 리롤 불가</color>\n");
            }
            panel.SetActive(_entranceStarted && _text.Length > 0);
            // This pixel font's bold weight closes the gaps in small Hangul glyphs.
            if (_text.Length > 0) _text.Length--; // Remove the last empty line.
            label.text = _text.Length > 0
                ? "<size=16><color=#CBD5E1>이벤트 효과 · 파티 전체</color></size>\n" + _text
                : string.Empty;
            if (_text.Length > 0)
                ((RectTransform)panel.transform).SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical, label.preferredHeight + 16f);
        }

        private void AppendEffect(string name, float percent, int battles, bool outgoing)
        {
            if (percent == 0f) return;
            bool beneficial = outgoing ? percent > 0f : percent < 0f;
            _text.Append(beneficial ? "<color=#92E6AD>" : "<color=#FF9A9A>")
                .Append(name).Append(' ').Append(percent.ToString("+0.##;-0.##", CultureInfo.InvariantCulture))
                .Append("%</color> · ");
            if (battles == -1) _text.Append("모험 동안");
            else _text.Append("남은 ").Append(battles).Append("전투");
            _text.Append('\n');
        }
    }
}
