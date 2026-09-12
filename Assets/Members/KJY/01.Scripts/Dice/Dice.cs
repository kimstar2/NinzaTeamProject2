using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using DG.Tweening;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Members.KJY._01.Scripts.Dice
{
    public class Dice : MonoBehaviour // 테스트 용 코드임
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private PlayerType playerType;
        [SerializeField] private SetDiceFaceSO setDiceFace;
        [SerializeField] private List<DiceFaceStruct> diceFaces;
        [Header("DiceSeqSetting")]
        [SerializeField] private TweenStep rollingStep, fallingStep;
        [SerializeField] private Vector3 minSpinVec, maxSpinVec;
        [SerializeField] private float minRollingPower, maxRollingPower;
        [SerializeField] private Transform destTrm;
        private bool _isLock;
        
        private Sequence _roll;
        private int _currenRan;
        private DiceFaceType _crtFaceType;
        
        private void OnEnable()
        {
            eventChannel.AddListener<OnRoll>(HandleRoll);
            eventChannel.AddListener<OnDiceLock>(HandleDiceLock);
        }
        private void OnDisable()
        {
            _roll?.Kill();
            _roll = null;
            eventChannel.RemoveListener<OnRoll>(HandleRoll);
        }
        
        private void HandleRoll(OnRoll obj) => Roll(destTrm.localPosition,GetRandom());
        private void HandleDiceLock(OnDiceLock obj)
        {
            if (obj.PlayerType != playerType) return;
            _isLock = obj.IsLock;
        }


        public void Roll(Vector3 localEndPosition, Vector3 resultEuler)
        {
            _roll?.Kill();

            float x = Random.Range(minSpinVec.x, maxSpinVec.x);
            float y = Random.Range(minSpinVec.y, maxSpinVec.y);
            float z = Random.Range(minSpinVec.z, maxSpinVec.z);

            _roll = DOTween.Sequence().OnComplete(CompleteRoll);
            
            float power = Random.Range(minRollingPower, maxRollingPower);
            Vector3 spinEnd = _isLock ? transform.localEulerAngles : resultEuler + new Vector3(x, y, z);
            Vector3 maxPos = new Vector3(localEndPosition.x, _isLock ? localEndPosition.y : power , localEndPosition.z);
            
            _roll.Append(
                transform.DOLocalMove(maxPos, rollingStep.Duration).SetEase(rollingStep.EaseType));
            _roll.Append(
                transform.DOLocalMove(destTrm.localPosition, power/fallingStep.Duration).SetEase(fallingStep.EaseType)
            );
            _roll.Insert(0f,
                transform.DOLocalRotate(
                        spinEnd, 0.9f, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
            );
        }
        
        private Vector3 GetRandom()
        {
            _currenRan = Random.Range(0, diceFaces.Count);
            return diceFaces[_currenRan].Range;
        }
        
        private void CompleteRoll()
        {
             _crtFaceType = _isLock ? _crtFaceType : diceFaces[_currenRan].Type;
            eventChannel.RaiseEvent(new OnRollEnd(_crtFaceType, playerType));
        }
    }
}