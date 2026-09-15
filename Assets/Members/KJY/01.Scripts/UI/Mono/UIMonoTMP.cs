using TMPro;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI.Mono
{
    public class UIMonoTMP : MonoBehaviour
    {
        private TextMeshProUGUI _tmp;

        private void Awake() => _tmp = GetComponent<TextMeshProUGUI>();

        public void SetText(string value)
        {
            _tmp.SetText(value);
        }
        
        public void ClearText() =>  _tmp.SetText(string.Empty);
    }
}