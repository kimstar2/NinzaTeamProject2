using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using DG.Tweening;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public abstract class AbstractDice : MonoBehaviour
    {
        [Header("Dice Setting")]
        [SerializeField] protected EventChannelSO eventChannel;
        [SerializeField] protected List<DiceFaceStruct> diceFaces;
        [Header("Dice Seq Setting")]
        [SerializeField] protected TweenStep rollingStep, fallingStep;
        [SerializeField] protected Vector3 minSpinVec, maxSpinVec;
        [SerializeField] protected float minRollingPower, maxRollingPower;
        [SerializeField] protected Transform destTrm;
        protected bool isLock;
        
        protected Sequence roll;
        protected int currenRan;
        protected DiceFaceType crtFaceType;

        protected void Roll(Vector3 localEndPosition, Vector3 resultEuler)
        {
            roll?.Kill();

            float x = Random.Range(minSpinVec.x, maxSpinVec.x);
            float y = Random.Range(minSpinVec.y, maxSpinVec.y);
            float z = Random.Range(minSpinVec.z, maxSpinVec.z);

            roll = DOTween.Sequence().OnComplete(CompleteRoll);
            
            float power = Random.Range(minRollingPower, maxRollingPower);
            Vector3 spinEnd = isLock ? transform.localEulerAngles : resultEuler + new Vector3(x, y, z);
            Vector3 maxPos = new Vector3(localEndPosition.x, isLock ? localEndPosition.y : power , localEndPosition.z);
            
            roll.Append(
                transform.DOLocalMove(maxPos, rollingStep.Duration).SetEase(rollingStep.EaseType));
            roll.Append(
                transform.DOLocalMove(destTrm.localPosition, Mathf.Abs(power)/fallingStep.Duration).SetEase(fallingStep.EaseType)
            );
            roll.Insert(0f,
                transform.DOLocalRotate(
                        spinEnd, 0.9f, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
            );
        }
        protected Vector3 GetRandom()
        {
            currenRan = PickFace();
            return diceFaces[currenRan].Range;
        }

        private int PickFace()
        {
            float total = 0f;
            _faceWeights.Clear();
            foreach (DiceFaceStruct face in diceFaces)
            {
                float weight = Mathf.Max(0f, GetFaceWeight(face.Type));
                _faceWeights.Add(weight);
                total += weight;
            }
            if (total <= 0f) return Random.Range(0, diceFaces.Count);

            float pick = Random.value * total;
            for (int i = 0; i < _faceWeights.Count; i++)
            {
                pick -= _faceWeights[i];
                if (pick < 0f) return i;
            }
            return _faceWeights.Count - 1;
        }

        private readonly List<float> _faceWeights = new();
        protected virtual float GetFaceWeight(DiceFaceType faceType) => 1f;
        
        protected abstract void CompleteRoll();
    }
}