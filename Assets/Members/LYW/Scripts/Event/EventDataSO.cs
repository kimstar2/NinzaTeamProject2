using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Members.LYW.Scripts.Event
{
    [CreateAssetMenu(fileName = "EventDataSO", menuName = "LYW/SO/EventDataSO")]
    public class EventDataSO : ScriptableObject
    {
        public string EventTitle;
        public string EventExplain;
        public Sprite backgroundImage;
        public Sprite EXImage;

        [Range(1, 4)]
        public int choices = 1;

        public List<string> choiceText = new();
        public List<Button.ButtonClickedEvent> choiceEvent = new();

        private void OnValidate()
        {
            choices = Mathf.Clamp(choices, 1, 4);

            ResizeList(choiceText, choices, "");
            ResizeList(choiceEvent, choices, () => new Button.ButtonClickedEvent());
        }

        private void ResizeList<T>(List<T> list, int size, T defaultValue)
        {
            while (list.Count < size)
                list.Add(defaultValue);

            while (list.Count > size)
                list.RemoveAt(list.Count - 1);
        }

        private void ResizeList<T>(List<T> list, int size, System.Func<T> factory)
        {
            while (list.Count < size)
                list.Add(factory());

            while (list.Count > size)
                list.RemoveAt(list.Count - 1);
        }
    }
}