using System.Collections;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.KJY._01.Scripts.Dice.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    public class DiceCatalogPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private DiceDataSO[] faces;
        [SerializeField] private BattleRewardItem itemPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TweenSequencer openMotion, closeMotion;
        private readonly List<BattleRewardItem> _items = new();
        private DiceCatalogSO _catalog;
        public bool IsOpen => panelRoot.activeSelf;

        private void Awake()
        {
            panelRoot.SetActive(false);
            // 전체 목록은 씬이 아니라 Resources/DiceCatalog 에셋에서 받는다 (씬 저장 상태와 상관없이 항상 최신)
            _catalog = Resources.Load<DiceCatalogSO>(DiceCatalogSO.ResourcePath);
            if (_catalog != null && _catalog.Faces != null && _catalog.Faces.Length > 0) faces = _catalog.Faces;
        }

        public void Open()
        {
            panelRoot.SetActive(true);
            panelGroup.interactable = true;
            if (_items.Count == 0)
            {
                foreach (var face in faces)
                {
                    if (face == null) continue;
                    var item = Instantiate(itemPrefab, content);
                    item.GetComponent<Button>().onClick.AddListener(() => ShowFace(face));
                    _items.Add(item);
                }
            }

            // 열 때마다 획득 기록을 다시 반영한다
            int found = 0;
            for (int i = 0, f = 0; i < faces.Length; i++)
            {
                if (faces[i] == null) continue;
                var item = _items[f++];
                bool discovered = IsUnlocked(faces[i]);
                if (discovered)
                {
                    item.Bind(faces[i], 1f);
                    found++;
                }
                else item.BindLocked(faces[i]);
                item.Reveal();
            }
            description.text = $"수집 {found} / {_items.Count}\n\n게임에서 한 번이라도 얻은 스킬만 도감에 기록됩니다.\n\n면을 선택하면 역할별 효과를 확인할 수 있습니다.";
            openMotion.Sequence();
            if (_scrollTop != null) StopCoroutine(_scrollTop);
            _scrollTop = StartCoroutine(KeepScrollTop());
        }

        // 열림 연출과 아이템 연출이 도는 동안 레이아웃이 다시 잡히며 스크롤이 내려가므로, 끝날 때까지 맨 위에 고정한다.
        private Coroutine _scrollTop;
        private IEnumerator KeepScrollTop()
        {
            var scroll = content.GetComponentInParent<ScrollRect>();
            var rect = (RectTransform)content;
            float until = Time.unscaledTime + 0.6f;
            do
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                if (scroll != null) scroll.StopMovement();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f); // Content 피벗이 위쪽이라 y=0이 맨 위
                yield return null;
            } while (openMotion.HasTween || Time.unscaledTime < until);
            _scrollTop = null;
        }

        // 플레이어 기본 주사위 면은 처음부터 해금
        private bool IsUnlocked(DiceDataSO face) =>
            _catalog != null ? _catalog.IsUnlocked(face) : DiceCatalogProgress.IsDiscovered(face);

        private void ShowFace(DiceDataSO face)
        {
            description.text = IsUnlocked(face)
                ? face.MainName + "\n\n" + face.GetDescription(1f)
                : "???\n\n아직 획득하지 않은 스킬입니다.\n전투 보상이나 이벤트, 재련으로 얻으면 정보가 공개됩니다.";
        }

        public void Close() { if (panelGroup.interactable) StartCoroutine(ClosePanel()); }
        private IEnumerator ClosePanel()
        {
            panelGroup.interactable = false;
            openMotion.Stop();
            closeMotion.Sequence();
            yield return new WaitUntil(() => !closeMotion.HasTween);
            panelRoot.SetActive(false);
        }
    }
}
