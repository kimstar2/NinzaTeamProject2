using System;
using DG.Tweening;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics.SolarEclipse
{
    public class SolarEclipse : MonoBehaviour, ISkillLogic
    {
        [Header("Test")]
        [SerializeField] private Vector3 targetPos;

        [Header("Item")] 
        [SerializeField] private SkillItem solar;
        [SerializeField] private SkillItem luna;
        [SerializeField] private SkillItem magicMap;
        [SerializeField] private SkillItem boomLight;
        
        public event Action<CalculateStat, SkillSO, GameObject> OnCalculate;
        public event Action OnSkillFinished;

        private bool _magicMapRotate = false;
        private CalculateStat CurrentStat => _executor.Owner.GetModule<StatModule>().CurrentStat;
        
        private SkillExecutor _executor;

        #region Component Init

        private void GetCompo()
        {
            if(solar.obj != null)
                solar.trm = solar.obj.transform;
            if(luna.obj != null)
                luna.trm = luna.obj.transform;
            if(magicMap.obj != null)
                magicMap.trm = magicMap.obj.transform;
            if(boomLight.obj != null)
                boomLight.trm = boomLight.obj.transform;
            
            solar.sr = FindRenderer(solar.obj);
            luna.sr = FindRenderer(luna.obj);
            magicMap.sr = FindRenderer(magicMap.obj);
            boomLight.sr = FindRenderer(boomLight.obj);
        }

        private static SpriteRenderer FindRenderer(GameObject target)
        {
            if(target != null && target.TryGetComponent(out SpriteRenderer renderer))
                return renderer;

            return null;
        }
        
        private void OnValidate()
        {
            GetCompo();
        }

        #endregion

        #region Reset Item
        
        [ContextMenu("Reset Item")]
        public void ResetItem()
        {
            solar.sr.color = new Color(0, 0, 0, 0);
            luna.sr.color = new Color(1, 1, 1, 0);
            magicMap.sr.color = new Color(1, 1, 1, 0);
            boomLight.sr.color = new Color(0, 0, 0, 1);

            solar.trm.localPosition = targetPos - Vector3.left * 5;
            luna.trm.localPosition = targetPos - Vector3.right * 2;
            magicMap.trm.localPosition = targetPos;
            boomLight.trm.localPosition = targetPos;
            
            solar.trm.localScale = new Vector3(2, 2, 1);
            luna.trm.localScale = new Vector3(2.1f, 2.1f, 1);
            magicMap.trm.localScale = Vector3.one;
            boomLight.trm.localScale = Vector3.zero;
            
            solar.obj.SetActive(true);
            luna.obj.SetActive(true);
            magicMap.obj.SetActive(true);
            boomLight.obj.SetActive(true);

            _magicMapRotate = false;
        }
        
        public void Init(SkillExecutor executor)
        {
            _executor = executor;
            ResetItem();
        }

        #endregion
        
        private void Update()
        {
            if(_magicMapRotate)
                magicMap.trm.rotation *= Quaternion.Euler(0, 0, 0.5f);
        }

        [ContextMenu("TestSkill")]
        public void TestSkill()
        {
            Sequence seq = DOTween.Sequence();
            StartFade(seq);
            seq.AppendCallback(MoveObject);
            seq.AppendInterval(1.1f);
            seq.AppendCallback(() =>
            {
                magicMap.sr.DOFade(1, 1f);
                MagicMapRotate(true);
            });
            seq.AppendInterval(2f);
            seq.AppendCallback(() => solar.obj.SetActive(false));
            seq.Append(boomLight.trm.DOScale(new Vector3(4, 4, 1), 0.6f));
            seq.AppendCallback(() => boomLight.trm.localScale = Vector3.zero);
            seq.AppendCallback(() => boomLight.obj.SetActive(false));
            seq.Append(luna.trm.DOScale(new Vector3(10, 10, 1), 1f).SetEase(Ease.OutQuart));
            seq.AppendCallback(() => magicMap.obj.SetActive(false));
            seq.Append(luna.trm.DOScale(Vector3.zero, 0.3f));
            seq.AppendCallback(() => luna.obj.SetActive(false));
        }
        
        public void PlaySkill(SkillSO skill, GameObject target)
        {
            Sequence seq = DOTween.Sequence();
            
            StartFade(seq);
            seq.AppendCallback(MoveObject);
            seq.AppendInterval(1.4f);
            seq.AppendCallback(() =>
            {
                magicMap.sr.DOFade(1, 1f);
                MagicMapRotate(true);
            });
            seq.AppendInterval(2f);
            seq.Append(boomLight.trm.DOScale(new Vector3(3, 3, 1), 1f).SetEase(Ease.OutElastic));
            
            seq.AppendCallback(() => OnCalculate?.Invoke(CurrentStat, skill, target));
            seq.AppendCallback(() => OnSkillFinished?.Invoke());
        }

        private void StartFade(Sequence seq)
        {
            seq.AppendCallback(() =>
            {
                solar.sr.DOFade(1, 0.7f);
                luna.sr.DOFade(1, 0.7f);
            });
            
            seq.AppendInterval(0.5f);
        }

        private void MoveObject()
        {
            solar.trm.DOMove(targetPos, 1f);
            luna.trm.DOMove(targetPos, 1f);
        }

        private void MagicMapRotate(bool isRotate)
        {
            if(magicMap.trm != null)
                _magicMapRotate = isRotate;
        }
    }
}