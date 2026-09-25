using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Members.PSW.Code.SettingSystem
{
    [DisallowMultipleComponent]
    public sealed class SettingsCategories : MonoBehaviour
    {
        public enum Category
        {
            Screen = 0,
            Audio = 1
        }

        [Serializable]
        private sealed class CategoryPanel
        {
            public Category category;
            public GameObject root;
            public Button button;
            public GameObject selectedIndicator;
            [NonSerialized] public UnityAction onClick;
        }

        [SerializeField] private Category defaultCategory = Category.Screen;
        [SerializeField] private CategoryPanel[] panels = Array.Empty<CategoryPanel>();

        private readonly Dictionary<Category, CategoryPanel> _panelsByCategory = new();

        private void Awake()
        {
            foreach (CategoryPanel panel in panels)
            {
                if (panel == null || panel.root == null || panel.button == null
                    || panel.selectedIndicator == null || _panelsByCategory.ContainsKey(panel.category))
                {
                    Debug.LogError("Settings categories require unique categories and assigned panel, button and indicator references.", this);
                    enabled = false;
                    return;
                }

                Category category = panel.category;
                panel.onClick = () => Select(category);
                _panelsByCategory.Add(category, panel);
            }

            Select(defaultCategory);
        }

        private void OnEnable()
        {
            foreach (CategoryPanel panel in _panelsByCategory.Values)
                panel.button.onClick.AddListener(panel.onClick);
        }

        private void OnDisable()
        {
            foreach (CategoryPanel panel in _panelsByCategory.Values)
                panel.button.onClick.RemoveListener(panel.onClick);
        }

        public void Select(Category category)
        {
            if (!_panelsByCategory.TryGetValue(category, out CategoryPanel selected))
            {
                Debug.LogError($"Settings category is not registered: {category}", this);
                return;
            }

            foreach (CategoryPanel panel in _panelsByCategory.Values)
            {
                bool isSelected = panel == selected;
                panel.root.SetActive(isSelected);
                panel.selectedIndicator.SetActive(isSelected);
            }
        }
    }
}
