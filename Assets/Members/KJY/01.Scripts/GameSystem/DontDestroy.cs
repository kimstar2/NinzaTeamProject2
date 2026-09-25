using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.GameSystem
{
    public class DontDestroy : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}