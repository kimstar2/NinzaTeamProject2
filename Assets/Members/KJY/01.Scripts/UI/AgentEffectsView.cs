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
            // 떠오르는 문구도 한글이 나오도록 이 글자의 폰트를 같이 쓴다
            if (CombatStatusVisual.PopupFont == null && label != null) CombatStatusVisual.PopupFont = label.font;
        }

        // 상태 글자(빗나감 1, 보호 2 등)가 전투 내내 떠 있어서 표시하지 않는다.
        // 상태는 캐릭터 위 문구와 캐릭터 색으로 보여준다. 다시 보이게 하려면 showLabel을 켠다.
        [SerializeField] private bool showLabel;
        private void Refresh() => label.text = showLabel ? selector.Effects.Description : string.Empty;
        private void OnDestroy() { if (selector != null) selector.Effects.Changed -= Refresh; }
    }
}
