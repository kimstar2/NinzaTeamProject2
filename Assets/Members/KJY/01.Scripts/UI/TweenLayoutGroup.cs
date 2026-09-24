using System;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    // A layout owns destinations; the saved sequencers own exit/reflow motion.
    public class TweenLayoutGroup : MonoBehaviour
    {
        [Serializable]
        private class Item
        {
            public Transform target;
            public TweenSequencer exitSequence;
            public TweenSequencer reflowSequence;
            public CanvasGroup canvasGroup;
            public Vector3 exitOffset;
            [NonSerialized] public bool removing;
            [NonSerialized] public UnityAction onExit;
            [NonSerialized] public Vector3 scale;
        }

        [SerializeField] private HorizontalOrVerticalLayoutGroup layoutGroup;
        [SerializeField] private List<Item> items = new();
        private Vector3[] _slots;
        private bool _layoutWasEnabled;
        private bool _layoutFrozen;
        private readonly List<Item> _activeItems = new();
        public event Action<Transform> OnRemoved;
        public int ActiveCount => _activeItems.Count;

        public int GetActiveIndex(Transform target)
        {
            Item item = FindItem(target);
            return item == null ? -1 : _activeItems.IndexOf(item);
        }

        private void Awake()
        {
            _layoutWasEnabled = layoutGroup != null && layoutGroup.enabled;
            foreach (Item item in items)
            {
                item.onExit = () => FinishRemove(item);
                item.exitSequence.onSeqComplete.AddListener(item.onExit);
                item.scale = item.target.localScale;
                if (item.target.gameObject.activeSelf) _activeItems.Add(item);
            }
        }

        private void Start() => CacheSlots();

        private void CacheSlots()
        {
            if (_slots != null) return;
            if (layoutGroup != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);
            _slots = new Vector3[items.Count];
            for (int i = 0; i < items.Count; i++)
                _slots[i] = items[i].target.localPosition;
        }

        public void Remove(Transform target)
        {
            Item item = FindItem(target);
            if (item == null || item.removing || !item.target.gameObject.activeSelf) return;

            CacheSlots();
            item.removing = true;
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
                _layoutFrozen = true;
            }
            item.reflowSequence.Stop();
            if (item.canvasGroup != null)
            {
                item.canvasGroup.interactable = false;
                item.canvasGroup.blocksRaycasts = false;
            }
            item.exitSequence.SetPositionValue(item.target.localPosition + item.exitOffset);
            if (!item.exitSequence.SequenceAndResult()) FinishRemove(item);
        }

        private void FinishRemove(Item item)
        {
            if (!item.removing) return;
            item.removing = false;
            _activeItems.Remove(item);
            item.target.gameObject.SetActive(false);
            Reflow();
            OnRemoved?.Invoke(item.target);
        }

        private Item FindItem(Transform target)
            => items.Find(item => target == item.target || target.IsChildOf(item.target));

        public bool IsRemoved(Transform target)
        {
            Item item = FindItem(target);
            return item != null && !item.removing && !item.target.gameObject.activeSelf;
        }

        public void Hide(Transform target)
        {
            Item item = FindItem(target);
            if (item == null) return;
            CacheSlots();
            item.exitSequence.Stop();
            item.reflowSequence.Stop();
            item.removing = false;
            _activeItems.Remove(item);
            item.target.gameObject.SetActive(false);
        }

        public void Add(Transform target)
        {
            Item item = FindItem(target);
            if (item == null) return;
            CacheSlots();
            item.exitSequence.Stop();
            item.reflowSequence.Stop();
            item.removing = false;
            _activeItems.Remove(item);
            _activeItems.Add(item); // 증원은 남아 있는 유닛 뒤로 들어옴
            item.target.SetAsLastSibling();
            item.target.gameObject.SetActive(true);
            item.target.localScale = item.scale;
            if (item.canvasGroup != null)
            {
                item.canvasGroup.alpha = 1f;
                item.canvasGroup.interactable = true;
                item.canvasGroup.blocksRaycasts = true;
            }

            Vector3 destination = _slots[_activeItems.Count - 1];
            item.target.localPosition = destination + item.exitOffset;
            Reflow();
        }

        public void Reflow()
        {
            CacheSlots();
            var previous = new Vector3[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                previous[i] = items[i].target.localPosition;
                items[i].reflowSequence.Stop();
            }
            
            if (layoutGroup != null)
            {
                layoutGroup.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform); // 재계산
                layoutGroup.enabled = false;
                _layoutFrozen = true;
            }

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                if (!item.target.gameObject.activeSelf) continue;
                
                Vector3 dest = 
                    layoutGroup != null ?
                        item.target.localPosition :
                        _slots[_activeItems.IndexOf(item)];
                
                item.target.localPosition = previous[i];
                if (item.removing) continue;
                item.reflowSequence.SetPositionValue(dest);
                item.reflowSequence.Sequence();
            }
        }

        private void OnDisable()
        {
            foreach (Item item in items)
            {
                item.exitSequence.Stop();
                item.reflowSequence.Stop();
            }
            if (layoutGroup != null) layoutGroup.enabled = _layoutWasEnabled;
        }

        private void LateUpdate()
        {
            if (!_layoutFrozen || layoutGroup == null) return;
            foreach (Item item in items)
                if (item.target.gameObject.activeSelf &&
                    (item.removing || item.reflowSequence.HasTween)) return;
            _layoutFrozen = false;
            layoutGroup.enabled = _layoutWasEnabled;
        }

        private void OnDestroy()
        {
            foreach (Item item in items)
                if (item.exitSequence != null)
                    item.exitSequence.onSeqComplete.RemoveListener(item.onExit);
        }
    }
}
