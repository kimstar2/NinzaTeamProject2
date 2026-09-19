using Members.KJY._01.Scripts.Util;
using UnityEditor.Animations;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Player
{
    [CreateAssetMenu(fileName = "Player data", menuName = "KJY/Agent/Player data", order = 0)]
    public class PlayerDataSO : ScriptableObject
    {
        [field:SerializeField] public PlayerType PlayerType {get; private set;}
        [field:SerializeField] public AnimatorOverrideController AnimCon {get; private set;}
        [field:SerializeField] public Sprite PlayerImage {get; private set;}
        [field:SerializeField] public ColorSO ImageColor {get; private set;}
    }
}