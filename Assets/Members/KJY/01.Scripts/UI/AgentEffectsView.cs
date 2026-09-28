using Members.KJY._01.Scripts.Agent;
using TMPro;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    public class AgentEffectsView : MonoBehaviour
    {
        [SerializeField] private AbstractSelector selector;
        [SerializeField] private TMP_Text label;

        private void Start()
        {
            selector.Effects.Changed += Refresh;
            Refresh();
            if (CombatStatusVisual.PopupFont == null && label != null) CombatStatusVisual.PopupFont = label.font;
        }

        [SerializeField] private bool showLabel;
        private void Refresh() => label.text = showLabel ? selector.Effects.Description : string.Empty;
        private void OnDestroy() { if (selector != null) selector.Effects.Changed -= Refresh; }
    }
}
