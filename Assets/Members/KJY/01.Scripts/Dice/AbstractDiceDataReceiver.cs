using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Mono;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Dice.Data;
using _TevLib.Extension;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public abstract class AbstractDiceDataReceiver : MonoBehaviour
    {
        [SerializeField] protected EventChannelSO eventChannel;
        [SerializeField] private MeshRenderer borderRenderer;
        [Header("Border Shader Hash")]
        [SerializeField] private ShaderHashSO useFaceColorsHash;
        [SerializeField] private ShaderHashSO frontColorHash, backColorHash;
        [SerializeField] private ShaderHashSO leftColorHash, rightColorHash;
        [SerializeField] private ShaderHashSO topColorHash, bottomColorHash;
        [field:SerializeField] public MonoSprite FrontImage { get; private set; }
        [field:SerializeField] public MonoSprite BackImage { get; private set; }
        [field:SerializeField] public MonoSprite LeftImage { get; private set; }
        [field:SerializeField] public MonoSprite RightImage { get; private set; }
        [field:SerializeField] public MonoSprite TopImage { get; private set; }
        [field:SerializeField] public MonoSprite BottomImage { get; private set; }
        
        
        private MaterialPropertyBlock _borderProperties;

        protected void ApplyDiceData(DiceDataListSO list, AgentAttackType attackType)
        {
            FrontImage.SetSprite(list.Front.GetIcon(attackType));
            BackImage.SetSprite(list.Back.GetIcon(attackType));
            LeftImage.SetSprite(list.Left.GetIcon(attackType));
            RightImage.SetSprite(list.Right.GetIcon(attackType));
            TopImage.SetSprite(list.Top.GetIcon(attackType));
            BottomImage.SetSprite(list.Bottom.GetIcon(attackType));
            if (borderRenderer == null) return;

            _borderProperties ??= new MaterialPropertyBlock();
            borderRenderer.GetPropertyBlock(_borderProperties);
            _borderProperties.SetFloat(useFaceColorsHash.HashValue, 1f);
            _borderProperties.SetColor(frontColorHash.HashValue, list.Front.DiceGrade.GradeColor);
            _borderProperties.SetColor(backColorHash.HashValue, list.Back.DiceGrade.GradeColor);
            _borderProperties.SetColor(leftColorHash.HashValue, list.Left.DiceGrade.GradeColor);
            _borderProperties.SetColor(rightColorHash.HashValue, list.Right.DiceGrade.GradeColor);
            _borderProperties.SetColor(topColorHash.HashValue, list.Top.DiceGrade.GradeColor);
            _borderProperties.SetColor(bottomColorHash.HashValue, list.Bottom.DiceGrade.GradeColor);
            borderRenderer.SetPropertyBlock(_borderProperties);
        }

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
