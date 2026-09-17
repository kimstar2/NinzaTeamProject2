using System;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Mono
{
    public class MonoParticle : MonoBehaviour
    {
        public ParticleSystem ParticleSystem { get; private set; }
        public ParticleSystem.MainModule Main {get; private set;}
        
        private void Awake()
        {
            ParticleSystem = GetComponent<ParticleSystem>();
            Main = ParticleSystem.main;
        }

        public void SetParticleGradient(GradientSO gradient)
        {
            ParticleSystem.MainModule tempMain = Main;
            tempMain.startColor = gradient.GetGradient();;
            Main = tempMain;
        }

        public void SetParticleColor(ColorSO color)
        {
            ParticleSystem.MainModule tempMain = Main;
            tempMain.startColor = color.GetColor();
            Main = tempMain;
        }
        
        public void SetParticleColor(Color color)
        {
            ParticleSystem.MainModule tempMain = Main;
            tempMain.startColor = color;
            Main = tempMain;
        }
        
        public void PlayParticle() => ParticleSystem.Play();
    }
}