using UnityEngine;

namespace Members.LYW.Scripts.Event
{
    [CreateAssetMenu(fileName = "ChangeStatusDataSO", menuName = "LYW/SO/ChangeStatusDataSO", order = 0)]
    public class ChangeStatusDataSO : ScriptableObject
    {
        public int health;
        [Range(0f, 1f)] public float healMaxHealthRatio; // 0.5 = 50%
        // 등등 능력치 값 얼마나 증감 할것들 전부 추가
    }
}