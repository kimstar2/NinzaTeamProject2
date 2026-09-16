using System;
using System.Collections.Generic;

namespace Members.LYW.Scripts.System
{
    public static class EventBus
    {
        private static Dictionary<Type, Delegate> events = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> callback)
        {
            Type type = typeof(T);

            if (events.ContainsKey(type))
            {
                events[type] = Delegate.Combine(events[type], callback);
            }
            else
            {
                events[type] = callback;
            }
        }
    
        public static void UnSubscribe<T>(Action<T> callback)
        {
            Type type = typeof(T);

            if (events.ContainsKey(type))
            {
                events[type] = Delegate.Remove(events[type], callback);
            }
        }

        public static void Publish<T>(T value)
        {
            Type type = typeof(T);

            if (events.TryGetValue(type, out Delegate handler))
            {
                ((Action<T>)handler)?.Invoke(value);
            }
        }
    
        public static void Clear()
        {
            events.Clear();
        }
    }
}