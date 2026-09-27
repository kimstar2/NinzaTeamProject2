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
        }

        private void Refresh() => label.text = selector.Effects.Description;
        private void OnDestroy() { if (selector != null) selector.Effects.Changed -= Refresh; }
    }
}
