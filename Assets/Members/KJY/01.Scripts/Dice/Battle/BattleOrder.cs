using _TevLib.Extension.DoT;
using Members.KJY._01.Scripts.Flags;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class BattleOrder : MonoBehaviour , IRequirePooling
    {
        [field: SerializeField] public TweenSequencer AddSeq {get; private set;}
        [field: SerializeField] public TweenSequencer RemoveSeq {get; private set;}
        [field: SerializeField] public UIMonoImage Image {get; private set;}
        
        public void Remove() => Destroy(gameObject);
    }
}