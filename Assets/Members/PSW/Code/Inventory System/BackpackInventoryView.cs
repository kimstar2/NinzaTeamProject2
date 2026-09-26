using DevLib.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.PSW.Code.InventorySystem
{
    // Resolve the run inventory at the scene boundary; FragmentSetter remains reusable.
    public sealed class BackpackInventoryView : MonoBehaviour
    {
        [SerializeField] private FragmentSetter fragmentSetter;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform skillInfoPanel;
        [SerializeField] private TMP_Text skillTitle;
        [SerializeField] private TMP_Text skillDescription;
        [SerializeField] private Image skillIcon;
        private Inventory _inventory;
        private DiceDataSO _shownFragment;
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            if (fragmentSetter == null ||
                !ServiceLocator.TryGet<Inventory>(out _inventory) || _inventory == null)
            {
                Debug.LogError("BackpackInventoryView needs a FragmentSetter and an active run inventory.", this);
                return;
            }

            fragmentSetter.SetInventory(_inventory);
            _inventory.Changed += Refresh;
            fragmentSetter.FragmentClicked += ShowSkillInfo;
            HideSkillInfo();
            if (scrollRect != null)
            {
                _scrollPosition = scrollRect.normalizedPosition;
                scrollRect.onValueChanged.AddListener(HandleScroll);
            }
        }

        private void OnDisable()
        {
            if (_inventory != null) _inventory.Changed -= Refresh;
            if (fragmentSetter != null) fragmentSetter.FragmentClicked -= ShowSkillInfo;
            if (scrollRect != null) scrollRect.onValueChanged.RemoveListener(HandleScroll);
            HideSkillInfo();
            _inventory = null;
        }

        private void Refresh()
        {
            HideSkillInfo();
            fragmentSetter.SetContents();
        }

        private void HandleScroll(Vector2 position)
        {
            if ((position - _scrollPosition).sqrMagnitude > 0.000001f) HideSkillInfo();
            _scrollPosition = position;
        }

        private void HideSkillInfo()
        {
            _shownFragment = null;
            if (skillInfoPanel != null) skillInfoPanel.gameObject.SetActive(false);
        }

        private void ShowSkillInfo(DiceDataSO fragment, RectTransform source)
        {
            if (skillInfoPanel == null || skillTitle == null || skillDescription == null || skillIcon == null)
                return;
            if (_shownFragment == fragment)
            {
                HideSkillInfo();
                return;
            }

            skillIcon.sprite = fragment.Icon;
            skillTitle.text = fragment.name;
            skillDescription.text = string.Empty;
            if (fragment is RewardDiceFragmentSO reward)
            {
                if (reward.SkillData != null)
                {
                    skillTitle.text = reward.SkillData.SkillName;
                    skillDescription.text = reward.SkillData.GetDescription(reward.Level);
                }
                else if (reward.DiceData != null && reward.DiceData.SkillDataStructs != null)
                {
                    // Old rewards without an attack type still expose their available skills.
                    var description = new StringBuilder();
                    foreach (var entry in reward.DiceData.SkillDataStructs)
                    {
                        if (entry.SkillData == null) continue;
                        if (description.Length > 0) description.AppendLine().AppendLine();
                        description.AppendLine(entry.SkillData.SkillName);
                        description.Append(entry.SkillData.GetDescription(reward.Level));
                    }
                    skillDescription.text = description.ToString();
                }
            }

            skillInfoPanel.gameObject.SetActive(true);
            skillInfoPanel.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(skillInfoPanel);
            var parent = (RectTransform)skillInfoPanel.parent;
            Vector3 point = parent.InverseTransformPoint(source.TransformPoint(new Vector3(source.rect.xMax, 0f)));
            Rect bounds = parent.rect;
            float width = skillInfoPanel.rect.width;
            float height = skillInfoPanel.rect.height;
            float x = point.x + 12f;
            if (x + width > bounds.xMax - 12f) x = point.x - source.rect.width - width - 12f;
            // Pivot (0, 1): fit the tooltip beside the icon without leaving the inventory box.
            x = Mathf.Clamp(x, bounds.xMin + 12f, Mathf.Max(bounds.xMin + 12f, bounds.xMax - width - 12f));
            float y = Mathf.Clamp(point.y + 38f, Mathf.Min(bounds.yMin + height + 12f, bounds.yMax - 12f), bounds.yMax - 12f);
            skillInfoPanel.anchoredPosition = new Vector2(x, y);
            _shownFragment = fragment;
        }
    }
}
