using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public class SkillCalculater : MonoBehaviour
    {
        private void Calculate()
        {
            
        }

        private float AttackCalc(float current, float value)
        {
            float result = current;

            result *= value;
            
            return result;
        }
    }
}