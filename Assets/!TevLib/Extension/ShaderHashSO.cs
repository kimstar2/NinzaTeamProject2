using System;
using UnityEngine;

namespace _TevLib.Extension
{
    [CreateAssetMenu(fileName = "Shader hash", menuName = "KJY/Hash/Shader hash", order = 0)]
    public class ShaderHashSO : ScriptableObject
    {
        [field:SerializeField] public string HashName { get; private set; }
        [field:SerializeField] public int HashValue { get; private set; }

        // Shader ID는 실행 환경마다 달라질 수 있어 에셋을 읽을 때도 갱신한다.
        private void OnEnable() => RefreshHash();
        private void OnValidate() => RefreshHash();

        private void RefreshHash()
        {
            HashValue = string.IsNullOrEmpty(HashName) ? 0 : Shader.PropertyToID(HashName);
        }
    }
}
