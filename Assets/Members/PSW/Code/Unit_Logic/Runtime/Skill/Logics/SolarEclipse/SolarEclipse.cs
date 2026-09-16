using System;
using System.Collections.Generic;
using DG.Tweening;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;
using UnityEngine.Events;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics.SolarEclipse
{
    public class SolarEclipse : AbstractSkillLogic
    {
        [Header("Test")]
        [SerializeField] private Vector3 targetPos;

        [Header("Item")] 
        [SerializeField] private SkillItem solar;
        [SerializeField] private SkillItem luna;
        [SerializeField] private SkillItem magicMap;
        [SerializeField] private SkillItem boomLight;
        public List<SkillApplyStat> applyStats;
        
        public UnityEvent onSkillFinished;

        private bool _magicMapRotate = false;
        private CalculateStat CurrentStat => _executor.Owner.GetModule<StatModule>().CurrentStat;
        
        private SkillExecutor _executor;
        private float _rotateSpeed;
        private AbstractSelector _target;

        
        
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
            
            solar.trm.localScale = new Vector3(2.5f, 2.5f, 1);
            luna.trm.localScale = new Vector3(2.6f, 2.6f, 1);
            magicMap.trm.localScale = Vector3.one;
            boomLight.trm.localScale = Vector3.zero;
            
            solar.obj.SetActive(true);
            luna.obj.SetActive(true);
            magicMap.obj.SetActive(true);
            boomLight.obj.SetActive(true);

            MagicMapRotate(false);
            _rotateSpeed = 0f;
        }

        #endregion
        
        private void Update()
        {
            if (_magicMapRotate)
            {
                magicMap.trm.rotation *= Quaternion.Euler(0, 0, _rotateSpeed);
                _rotateSpeed += 0.01f;
            }
        }

        #region Skill Code

        [ContextMenu("TestSkill")]
        public void TestSkill()
        {
            Sequence seq = DOTween.Sequence();
            StartFade(seq);
            seq.AppendCallback(MoveObject);
            seq.AppendInterval(2.1f);
            seq.AppendCallback(() =>
            {
                magicMap.sr.DOFade(1, 0.5f);
                MagicMapRotate(true);
            });
            seq.AppendInterval(1.5f);
            seq.AppendCallback(() => solar.obj.SetActive(false));
            seq.Append(boomLight.trm.DOScale(new Vector3(6f, 6f, 1), 0.6f));
            seq.AppendCallback(() => boomLight.trm.localScale = Vector3.zero);
            seq.AppendCallback(() => boomLight.obj.SetActive(false));
            seq.Append(luna.trm.DOScale(new Vector3(10, 10, 1), 1f).SetEase(Ease.OutQuart));
            seq.AppendCallback(() => magicMap.obj.SetActive(false));
            seq.Append(luna.trm.DOScale(Vector3.zero, 0.3f));
            seq.AppendCallback(() => luna.obj.SetActive(false));
        }
        
        private void StartFade(Sequence seq)
        {
            seq.AppendCallback(() =>
            {
                solar.sr.DOFade(1, 1f);
                luna.sr.DOFade(1, 1f);
            });
            
            seq.AppendInterval(1f);
        }

        private void MoveObject()
        {
            solar.trm.DOLocalMove(targetPos, 2f);
            luna.trm.DOLocalMove(targetPos, 2f);
        }

        private void MagicMapRotate(bool isRotate)
        {
            if(magicMap.trm != null)
                _magicMapRotate = isRotate;
        }

        #endregion

        public override void InitAndExecute(AbstractSelector attacker, AbstractSelector target)
        {
            ResetItem();
            _target= target;
            targetPos = target.DefaultPosition.position;
            
            Sequence seq = DOTween.Sequence();
            StartFade(seq);
            seq.AppendCallback(MoveObject);
            seq.AppendInterval(2.1f);
            seq.AppendCallback(() =>
            {
                magicMap.sr.DOFade(1, 0.5f);
                MagicMapRotate(true);
            });
            seq.AppendInterval(1.5f);
            seq.AppendCallback(() => solar.obj.SetActive(false));
            seq.Append(boomLight.trm.DOScale(new Vector3(6f, 6f, 1), 0.6f));
            seq.AppendCallback(() => boomLight.trm.localScale = Vector3.zero);
            seq.AppendCallback(() => boomLight.obj.SetActive(false));
            seq.Append(luna.trm.DOScale(new Vector3(10, 10, 1), 1f).SetEase(Ease.OutQuart));
            seq.AppendCallback(() => magicMap.obj.SetActive(false));

            seq.AppendCallback(() => ApplyStat());
            
            seq.Append(luna.trm.DOScale(Vector3.zero, 0.3f));
            seq.AppendCallback(() => luna.obj.SetActive(false));
            
            seq.AppendCallback(() => onSkillFinished?.Invoke());
        }

        public override void ApplyStat()
        {
            foreach (var applyStat in applyStats)
            {
                _target.ApplyStat(applyStat.ApplyStatType, applyStat.Value);
            }
        }
    }
}