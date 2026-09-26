using System;
using _TevLib.Extension.DoT;
using DG.Tweening;
using Members.KJY._01.Scripts.Dice;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.Title
{
    public class TitleDiceSetter : MonoBehaviour
    {
        [SerializeField] private TitleMenu startMenu , exitMenu, settingMenu, diceDictMenu;
        [SerializeField] private TweenStep tweenStep;
        [SerializeField] private TitleDice titleDice;
        [SerializeField] private float _destAngle;
        private int _id;
        private void Awake()
        {
            _id = transform.GetInstanceID();
        }

        public float DestAngle
        {
            get => _destAngle;
            set
            {
                DOTween.Kill(_id);
                if (Mathf.Approximately(value, 360) || Mathf.Approximately(value, -360))
                    value = 0;
                _destAngle = value;
                titleDice.Set(_destAngle switch
                {
                    0 => startMenu,
                    -90 or 270 => exitMenu,
                    90 or -270 => settingMenu,
                    180 or -180 => diceDictMenu,
                    _ => null
                });
                transform.DOLocalRotate(Vector2.up * _destAngle, tweenStep.Duration).SetEase(tweenStep.EaseType).SetId(_id);
            }
        }
        private void Update()
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                DestAngle += 90;
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                DestAngle -= 90;
        }
    }
}