using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace _TevLib.Extension.DoT
{
    [Serializable]
    public struct RandomTweenStep
    {
        public List<TweenStepSO> steps;

        public TweenStepSO GetRandomStep()
        {
            int r = Random.Range(0, steps.Count);
            return steps[r];
        }
    }
}