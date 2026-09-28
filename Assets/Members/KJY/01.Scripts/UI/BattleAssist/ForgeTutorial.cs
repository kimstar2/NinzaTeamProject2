using System;
using _TevLib.Extension.DoT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    public class ForgeTutorial : MonoBehaviour
    {
        private const string SeenKey = "OverRoll.ForgeTutorial.v1";

        [Serializable]
        private struct Page
        {
            public string title;
            [TextArea(3, 6)] public string description;
        }

        [SerializeField] private GameObject panel;
        [SerializeField] private Button helpButton;
        [SerializeField] private TMP_Text heading, description, progress, nextLabel;
        [SerializeField] private TweenSequencer openMotion, closeMotion, pageMotion;
        [SerializeField] private bool showOnFirstVisit = true;
        [SerializeField] private Page[] pages;
        private int _page;
        private bool _open;

        private void OnEnable()
        {
            if (helpButton != null) helpButton.interactable = true; // 프리팹 기본 비활성
            if (showOnFirstVisit && !_open && !PlayerPrefs.HasKey(SeenKey)) Show();
        }

        private void OnDisable()
        {
            openMotion.Stop();
            closeMotion.Stop();
            pageMotion.Stop();
            _open = false;
        }

        public void Show()
        {
            if (pages == null || pages.Length == 0) return;
            _page = 0;
            _open = true;
            panel.SetActive(true);
            RefreshPage();
            openMotion.Sequence();
        }

        public void PlaceHelpButtonLeftOf(RectTransform neighbor, float spacing = 12f)
        {
            if (helpButton == null || neighbor == null) return;
            var rect = (RectTransform)helpButton.transform;
            rect.SetParent(neighbor.parent, false);
            rect.anchorMin = neighbor.anchorMin;
            rect.anchorMax = neighbor.anchorMax;
            rect.pivot = neighbor.pivot;
            rect.sizeDelta = neighbor.sizeDelta;
            rect.localScale = neighbor.localScale;
            rect.anchoredPosition = neighbor.anchoredPosition - new Vector2(neighbor.rect.width + spacing, 0f);
            var neighborLabel = neighbor.GetComponentInChildren<TMP_Text>(true);
            var helpLabel = helpButton.GetComponentInChildren<TMP_Text>(true);
            if (neighborLabel != null && helpLabel != null)
            {
                helpLabel.enableAutoSizing = neighborLabel.enableAutoSizing;
                helpLabel.fontSize = neighborLabel.fontSize;
            }
        }

        public void Next()
        {
            if (!_open) return;
            if (++_page >= pages.Length) { Finish(); return; }
            RefreshPage();
        }

        public void Finish()
        {
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
            if (!_open) return;
            _open = false;
            closeMotion.Sequence();
        }

        private void RefreshPage()
        {
            heading.text = pages[_page].title;
            description.text = pages[_page].description;
            progress.text = $"재련 안내  {_page + 1} / {pages.Length}";
            if (nextLabel != null) nextLabel.text = _page + 1 >= pages.Length ? "닫기" : "다음";
            pageMotion.Sequence();
        }
    }
}
