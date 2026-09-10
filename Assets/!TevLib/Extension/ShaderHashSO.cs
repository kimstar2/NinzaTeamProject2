using System;
using UnityEngine;

namespace _TevLib.Extension
{
    [CreateAssetMenu(fileName = "Shader hash", menuName = "KJY/Hash/Shader hash", order = 0)]
    public class ShaderHashSO : ScriptableObject
    {
        [field:SerializeField] public string HashName { get; private set; }
        [field:SerializeField] public int HashValue { get; private set; }

        private void OnValidate()
        {
            HashValue = Shader.PropertyToID(HashName);
        }
    }
}