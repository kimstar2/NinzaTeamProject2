using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Members.LYW.Scripts.Event
{
    public enum EventType
    {
        Say,
        MoveNextEvent
    }
    
    [CreateAssetMenu(fileName = "EventDataSO", menuName = "LYW/SO/EventDataSO")]
    public class EventDataSO : ScriptableObject
    {
        [Header("Event Data")]
        public string EventTitle;
        public string EventExplain;
        public Sprite backgroundImage;
        public Sprite EXImage;

        [Header("Image Data")]
        public float exImageScale;
        public Color exImageColor;
        public Color backGroundImageColor;
        public Material exImageMaterial;
        
        [Range(1, 4)]
        public int choices = 1;

        public List<string> choiceText = new();
        public List<EventType> choiceEvent = new();

        private void OnValidate()
        {
            choices = Mathf.Clamp(choices, 1, 4);

            ResizeList(choiceText, choices, "");
            ResizeList(choiceEvent, choices, new EventType());
        }

        private void ResizeList<T>(List<T> list, int size, T defaultValue)
        {
            while (list.Count < size)
                list.Add(defaultValue);

            while (list.Count > size)
                list.RemoveAt(list.Count - 1);
        }

        private void ResizeList<T>(List<T> list, int size, Func<T> factory)
        {
            while (list.Count < size)
                list.Add(factory());

            while (list.Count > size)
                list.RemoveAt(list.Count - 1);
        }
    }
}