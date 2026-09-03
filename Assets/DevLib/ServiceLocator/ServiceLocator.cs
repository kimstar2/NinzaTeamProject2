using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevLib.ServiceLocator
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        //엔진 서브시스템 등록 시점에서 수행.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void InitializeServiceLocator()
        {
            _services.Clear();
        }
        
        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service; //기존 서비스는 지워진다.
            Debug.Log($"[Service Locator] Register {typeof(T).Name} 등록됨 : {service.GetType().Name}");
        }

        public static void UnRegister<T>()
        {
            _services.Remove(typeof(T));
            Debug.Log($"[Service Locator] UnRegister - {typeof(T).Name}");
        }

        public static T Get<T>()
        {
            if(_services.TryGetValue(typeof(T), out object service))
                return (T)service;
            
            Debug.LogWarning($"[Service Locator] {typeof(T).Name} 이 등록되지 않음");
            return default;
        }
    }
}