using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public abstract class AbstractDiceDataReceiver : MonoBehaviour
    {
        [SerializeField] protected EventChannelSO eventChannel;
        [field:SerializeField] public MonoSprite FrontImage { get; private set; }
        [field:SerializeField] public MonoSprite BackImage { get; private set; }
        [field:SerializeField] public MonoSprite LeftImage { get; private set; }
        [field:SerializeField] public MonoSprite RightImage { get; private set; }
        [field:SerializeField] public MonoSprite TopImage { get; private set; }
        [field:SerializeField] public MonoSprite BottomImage { get; private set; }
        
        
        public void SetFaceColor(Color color)
        {
            FrontImage.SetColor(color); 
            BackImage.SetColor(color); 
            LeftImage.SetColor(color); 
            RightImage.SetColor(color); 
            TopImage.SetColor(color); 
            BottomImage.SetColor(color);
        }
    }
}