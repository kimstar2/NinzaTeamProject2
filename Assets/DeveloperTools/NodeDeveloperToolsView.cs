#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DeveloperTools
{
    // Owns the temporary modal's presentation; commands and inventory remain in the controller.
    internal sealed class NodeDeveloperToolsView : IDisposable
    {
        private static readonly Color Panel = new Color32(23, 29, 53, 255);
        private static readonly Color Card = new Color32(32, 39, 67, 255);
        private static readonly Color Purple = new Color32(153, 130, 245, 255);
        private static readonly Color Muted = new Color32(166, 178, 213, 255);
        private readonly GameObject _root;
        private readonly RectTransform _window, _content;
        private readonly ScrollRect _scroll;
        private readonly TMP_FontAsset _font;
        private readonly Sprite _frame;
        private readonly TextMeshProUGUI _status, _count, _empty;
        private readonly TMP_InputField _search;
        private readonly Button[] _tabs = new Button[2];
        private readonly List<(GameObject root, string search)> _rows = new();
        private int _selectedTab;

        internal bool IsSearchFocused => _search.isFocused;

        internal NodeDeveloperToolsView(GameObject template, Transform owner, Action close, Action<int> tabChanged)
        {
            _root = new GameObject("Developer Tools Canvas", typeof(RectTransform));
            _root.SetActive(false);
            _root.transform.SetParent(owner, false);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760;
            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _root.AddComponent<GraphicRaycaster>();

            var inputRoot = new GameObject("Tool Event System", typeof(EventSystem));
            inputRoot.transform.SetParent(_root.transform, false);
            var module = inputRoot.AddComponent<InputSystemUIInputModule>();
            module.inputOverride = inputRoot.AddComponent<DeveloperImeInput>();
            module.AssignDefaultActions();

            var dim = Image(_root.transform, "Modal Dim", new Color(0, 0, 0, 0.76f));
            Stretch(dim.rectTransform, 0, 0, 0, 0);
            dim.raycastTarget = true;
            var panel = Object.Instantiate(template, _root.transform);
            _window = (RectTransform)panel.transform;
            _window.anchorMin = _window.anchorMax = new Vector2(0.5f, 0.5f);
            _window.pivot = new Vector2(0.5f, 0.5f);
            _window.anchoredPosition = Vector2.zero;
            var panelImage = panel.GetComponent<Image>();
            _frame = panelImage.sprite;
            panelImage.sprite = null;
            panelImage.color = Panel;
            panelImage.raycastTarget = true;
            Border(_window, new Color32(93, 102, 146, 255));
            var title = panel.GetComponentInChildren<TextMeshProUGUI>();
            _font = title.font;
            title.text = "개발자 도구";
            title.fontSize = 36;
            title.color = Color.white;
            title.raycastTarget = false;
            Position(title.rectTransform, 32, 22, 460, 48);
            Position(Text(_window, "노드 이동과 주사위 면 지급", 19, Muted).rectTransform, 34, 72, 650, 30);
            Position(Text(_window, "DEVELOPMENT", 16, Purple).rectTransform, 34, 110, 240, 24);
            var closeButton = Button(_window, "닫기  ×", close, false);
            closeButton.GetComponent<RectTransform>().anchorMin = closeButton.GetComponent<RectTransform>().anchorMax = Vector2.one;
            closeButton.GetComponent<RectTransform>().pivot = Vector2.one;
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28, -28);
            closeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(134, 48);

            for (int i = 0; i < 2; i++)
            {
                int tab = i;
                _tabs[i] = Button(_window, i == 0 ? "노드 이동" : "주사위 면 지급", () =>
                {
                    _selectedTab = tab;
                    _search.SetTextWithoutNotify("");
                    PaintTabs();
                    tabChanged(tab);
                }, false);
                Position((RectTransform)_tabs[i].transform, 32 + i * 254, 150, 244, 50);
            }

            var inputImage = Image(_window, "Search", new Color32(14, 19, 38, 255));
            TopStretch(inputImage.rectTransform, 32, 32, 218, 56);
            inputImage.raycastTarget = true;
            Border(inputImage.transform, new Color32(93, 102, 146, 255));
            _search = inputImage.gameObject.AddComponent<TMP_InputField>();
            var viewport = Rect(inputImage.transform, "Text Viewport");
            Stretch(viewport, 16, 14, 12, 12);
            viewport.gameObject.AddComponent<RectMask2D>();
            var searchText = Text(viewport, "", 22, Color.white);
            Stretch(searchText.rectTransform, 0, 0, 0, 0);
            var placeholder = Text(viewport, "이름 / 직업 / 노드 종류 검색 (한글·초성 지원)", 20, Muted);
            Stretch(placeholder.rectTransform, 0, 0, 0, 0);
            _search.textViewport = viewport;
            _search.textComponent = searchText;
            _search.placeholder = placeholder;
            _search.targetGraphic = inputImage;
            _search.contentType = TMP_InputField.ContentType.Standard;
            _search.characterValidation = TMP_InputField.CharacterValidation.None;
            _search.lineType = TMP_InputField.LineType.SingleLine;
            _search.richText = false;
            _search.customCaretColor = true;
            _search.caretColor = Purple;
            _search.selectionColor = new Color(0.6f, 0.51f, 0.96f, 0.35f);
            _search.onValueChanged.AddListener(Filter);
            _search.restoreOriginalTextOnEscape = false;

            _count = Text(_window, "", 18, Muted);
            TopStretch(_count.rectTransform, 34, 32, 282, 32);
            var list = Rect(_window, "List");
            Stretch(list, 32, 32, 322, 136);
            var listViewport = Rect(list, "Viewport");
            Stretch(listViewport, 0, 12, 0, 0);
            listViewport.gameObject.AddComponent<RectMask2D>();
            var listBackground = Image(listViewport, "Background", new Color(0, 0, 0, 0.01f));
            Stretch(listBackground.rectTransform, 0, 0, 0, 0);
            listBackground.raycastTarget = true;
            _content = Rect(listViewport, "Content");
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = Vector2.one;
            _content.pivot = new Vector2(0.5f, 1);
            _content.sizeDelta = Vector2.zero;
            var layout = _content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            _content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _scroll = list.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = listViewport;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 38;
            var track = Image(list, "Scrollbar", new Color32(14, 19, 38, 255));
            track.rectTransform.anchorMin = new Vector2(1, 0);
            track.rectTransform.anchorMax = Vector2.one;
            track.rectTransform.offsetMin = new Vector2(-7, 0);
            track.rectTransform.offsetMax = Vector2.zero;
            var handle = Image(track.transform, "Handle", Purple);
            Stretch(handle.rectTransform, 0, 0, 0, 0);
            handle.raycastTarget = true;
            var scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle.rectTransform;
            scrollbar.targetGraphic = handle;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            _scroll.verticalScrollbar = scrollbar;
            _empty = Text(listViewport, "검색 결과가 없습니다. 다른 이름이나 초성으로 검색해 주세요.", 21, Muted);
            TopStretch(_empty.rectTransform, 20, 20, 24, 90);
            _empty.alignment = TextAlignmentOptions.Center;
            _status = Text(_window, "", 19, Color.white);
            BottomStretch(_status.rectTransform, 34, 34, 60, 56);
            var footer = Text(_window, "Ctrl + Shift + Space + T / Esc 닫기   ·   변경 사항은 실제 진행에 반영됩니다.", 16, Muted);
            BottomStretch(footer.rectTransform, 34, 34, 20, 30);
            PaintTabs();
        }

        internal void Show() { _root.SetActive(true); Resize(); }
        internal void Hide()
        {
            if (_root == null) return;
            _search.DeactivateInputField();
            _root.SetActive(false);
        }
        internal void Resize()
        {
            float scale = Mathf.Sqrt(Screen.width / 1920f * Screen.height / 1080f);
            _window.sizeDelta = new Vector2(Mathf.Min(1180, Screen.width / scale - 40), Mathf.Min(900, Screen.height / scale - 40));
        }
        internal void SetStatus(string value) => _status.text = value;
        internal void ClearRows()
        {
            foreach (var row in _rows) { row.root.SetActive(false); Object.Destroy(row.root); }
            _rows.Clear();
        }
        internal void AddRow(Sprite icon, string title, string detail, Color gradeColor, string search,
            string primaryLabel, Action primary, Action secondary = null)
        {
            var card = Image(_content, title, Card);
            Border(card.transform, new Color32(65, 76, 115, 255));
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = 112;
            var stripe = Image(card.transform, "Grade", gradeColor);
            Position(stripe.rectTransform, 0, 10, 4, 92);
            var iconWell = Image(card.transform, "Icon Well", Panel);
            Position(iconWell.rectTransform, 18, 14, 84, 84);
            var picture = Image(iconWell.transform, "Face Image", Color.white);
            picture.sprite = icon;
            picture.preserveAspect = true;
            Stretch(picture.rectTransform, 7, 7, 7, 7);
            picture.enabled = icon != null;
            if (icon == null)
            {
                var missing = Text(iconWell.transform, "?", 28, Muted);
                Stretch(missing.rectTransform, 0, 0, 0, 0);
                missing.alignment = TextAlignmentOptions.Center;
            }
            var heading = Text(card.transform, title, 25, Color.white);
            TopStretch(heading.rectTransform, 122, secondary == null ? 172 : 258, 16, 36);
            var description = Text(card.transform, detail, 17, Muted);
            TopStretch(description.rectTransform, 122, secondary == null ? 172 : 258, 54, 46);
            var give = Button(card.transform, primaryLabel, primary, true);
            RightCenter((RectTransform)give.transform, 18, secondary == null ? 136 : 100, 48);
            if (secondary != null)
            {
                var other = Button(card.transform, "이동", secondary, false);
                RightCenter((RectTransform)other.transform, 128, 100, 48);
            }
            _rows.Add((card.gameObject, search));
        }
        internal void ApplyFilter() => Filter(_search.text);
        private void Filter(string query)
        {
            int count = 0;
            foreach (var row in _rows)
            {
                bool matches = DeveloperSearch.Matches(row.search, query);
                row.root.SetActive(matches);
                if (matches) count++;
            }
            _count.text = $"{(_selectedTab == 0 ? "노드" : "주사위 면")}  {count} / {_rows.Count}";
            _empty.gameObject.SetActive(count == 0);
            _scroll.StopMovement();
            _scroll.verticalNormalizedPosition = 1;
        }
        private void PaintTabs()
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                var colors = _tabs[i].colors;
                colors.normalColor = i == _selectedTab ? Purple : Card;
                _tabs[i].colors = colors;
            }
        }
        private TextMeshProUGUI Text(Transform parent, string value, float size, Color color)
        {
            var label = Rect(parent, "Text").gameObject.AddComponent<TextMeshProUGUI>();
            label.font = _font;
            label.fontSize = size;
            label.color = color;
            label.text = value;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            label.richText = false;
            return label;
        }
        private Button Button(Transform parent, string label, Action clicked, bool primary)
        {
            var image = Image(parent, label, Color.white);
            Border(image.transform, primary ? Purple : new Color32(93, 102, 146, 255));
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = primary ? new Color32(94, 74, 157, 255) : Card;
            colors.highlightedColor = new Color32(127, 105, 191, 255);
            colors.pressedColor = new Color32(75, 56, 128, 255);
            colors.selectedColor = colors.normalColor;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => clicked());
            var text = Text(image.transform, label, 21, Color.white);
            Stretch(text.rectTransform, 8, 8, 0, 0);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }
        private void Border(Transform parent, Color color)
        {
            var border = Image(parent, "Pixel Frame", color);
            border.sprite = _frame;
            border.type = UnityEngine.UI.Image.Type.Sliced;
            border.fillCenter = false;
            Stretch(border.rectTransform, 0, 0, 0, 0);
        }
        private static Image Image(Transform parent, string name, Color color)
        {
            var image = Rect(parent, name).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
        private static RectTransform Rect(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = 5;
            rect.SetParent(parent, false);
            return rect;
        }
        private static void Position(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
        private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(-right, -top);
        }
        private static void TopStretch(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1);
            rect.offsetMin = new Vector2(left, -top - height); rect.offsetMax = new Vector2(-right, -top);
        }
        private static void BottomStretch(RectTransform rect, float left, float right, float bottom, float height)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = new Vector2(1, 0);
            rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(-right, bottom + height);
        }
        private static void RightCenter(RectTransform rect, float right, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-right, 0); rect.sizeDelta = new Vector2(width, height);
        }
        public void Dispose() => Object.Destroy(_root);
    }
}
#endif
