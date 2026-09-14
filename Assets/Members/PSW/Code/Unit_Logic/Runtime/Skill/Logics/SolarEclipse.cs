using System;
using DG.Tweening;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics
{
    public class SolarEclipse : MonoBehaviour, ISkillLogic
    {
        [Header("Item")]
        [SerializeField] private Transform solar;
        [SerializeField] private Transform luna;
        [SerializeField] private Transform magicMap;
        [SerializeField] private Transform boomLight;
        [SerializeField] private Vector3 centerPos;

        
        public event Action<CalculateStat, SkillSO, GameObject> OnCalculate;
        public event Action OnSkillFinished;

        private SpriteRenderer _solarSr;
        private SpriteRenderer _lunaSr;
        private SpriteRenderer _magicMapSr;

        private bool _magicMapRotate = false;
        
        private CalculateStat CurrentStat => _executor.Owner.GetModule<StatModule>().CurrentStat;
        
        private SkillExecutor _executor;
        
        private void Update()
        {
            if(_magicMapRotate)
                magicMap.transform.rotation *= Quaternion.Euler(0, 0, 1);
        }

        [ContextMenu("Test Init")]
        public void TestInit()
        {
            _solarSr = solar.GetComponent<SpriteRenderer>();
            _lunaSr = luna.GetComponent<SpriteRenderer>();
            _magicMapSr = magicMap.GetComponent<SpriteRenderer>();
            
            _solarSr.color = new Color(0, 0, 0, 0);
            _lunaSr.color = new Color(1, 1, 1, 0);
            _magicMapSr.color = new Color(1, 1, 1, 0);

            magicMap.transform.localPosition = centerPos;
            boomLight.transform.localPosition = centerPos;
            boomLight.transform.localScale = Vector3.zero;
        }
        
        public void Init(SkillExecutor executor)
        {
            _executor = executor;
            
            _solarSr = solar.GetComponent<SpriteRenderer>();
            _lunaSr = luna.GetComponent<SpriteRenderer>();
            _magicMapSr = magicMap.GetComponent<SpriteRenderer>();
            
            _solarSr.color = new Color(0, 0, 0, 0);
            _lunaSr.color = new Color(1, 1, 1, 0);
            _magicMapSr.color = new Color(1, 1, 1, 0);
            
            magicMap.transform.localPosition = centerPos;
            boomLight.transform.localPosition = centerPos;
            boomLight.transform.localScale = Vector3.zero;
        }

        [ContextMenu("TestSkill")]
        public void TestSkill()
        {
            Sequence seq = DOTween.Sequence();
            StartFade(seq);
            seq.AppendCallback(MoveObject);
            seq.AppendInterval(1.4f);
            seq.AppendCallback(() =>
            {
                _magicMapSr.DOFade(1, 1f);
                MagicMapRotate(true);
            });
            seq.AppendInterval(2f);
            seq.AppendCallback(() => solar.gameObject.SetActive(false));
            seq.Append(boomLight.DOScale(new Vector3(5, 5, 1), 0.5f).SetEase(Ease.OutElastic));
            seq.Append(luna.DOScale(new Vector3(20, 20, 1), 2f).SetEase(Ease.Linear));
        }
        
        public void PlaySkill(SkillSO skill, GameObject target)
        {
            Sequence seq = DOTween.Sequence();
            
            StartFade(seq);
            seq.AppendCallback(MoveObject);
            seq.AppendInterval(1.4f);
            seq.AppendCallback(() =>
            {
                _magicMapSr.DOFade(1, 1f);
                MagicMapRotate(true);
            });
            seq.AppendInterval(2f);
            seq.Append(boomLight.DOScale(new Vector3(5, 5, 1), 1f).SetEase(Ease.OutElastic));
            
            seq.AppendCallback(() => OnCalculate?.Invoke(CurrentStat, skill, target));
            seq.AppendCallback(() => OnSkillFinished?.Invoke());
        }

        private void StartFade(Sequence seq)
        {
            seq.AppendCallback(() =>
            {
                _solarSr.DOFade(1, 0.7f);
                _lunaSr.DOFade(1, 0.7f);
            });
            
            seq.AppendInterval(0.5f);
        }

        private void MoveObject()
        {
            solar.DOMove(centerPos, 1f);
            luna.DOMove(centerPos, 1f);
        }

        private void MagicMapRotate(bool isRotate)
        {
            if(magicMap != null)
                _magicMapRotate = isRotate;
        }
    }
}