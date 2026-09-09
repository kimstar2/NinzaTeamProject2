using DevLib.CoreLib.Runtime;
using DG.Tweening;
using Members.PSW.Code.Unit_Logic.Runtime.Skill;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using TMPro;
using UnityEngine;

namespace Members.PSW.Code.Test
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private EventChannelSO _event;

        private void OnEnable()
        {
            _text.DOFade(0, 0.3f);
            _event.AddListener<CalcValueEvent>(HandleGetValue);
        }

        private void OnDisable()
        {
            _event.RemoveListener<CalcValueEvent>(HandleGetValue);
        }

        private void HandleGetValue(CalcValueEvent evt)
        {
            if (evt.SkillType != SkillType.Damage) return;
            
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() => _text.SetText($"{evt.Value}"));
            seq.Append(_text.DOFade(1, 0.3f));
            seq.AppendInterval(0.7f);
            seq.Append(_text.DOFade(0, 0.3f));
        }
    }
}