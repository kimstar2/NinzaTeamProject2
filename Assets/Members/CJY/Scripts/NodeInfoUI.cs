using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.CJY.Scripts
{
    public class NodeInfoUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;

        public void Init(NodeInfoSO info)
        {
            iconImage.sprite = info.icon;
            nameText.text = info.typeName;
        }
    }
}