using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Members.KJY.T.TestScripts
{
    [Serializable]
    public struct DiceData
    {
        public List<DiceSurfaceDataSO> diceSurface;
    }

    public class DiceSystem : MonoBehaviour
    {
        public static DiceSystem Instance; // 싹다 테스트용임
        [field:SerializeField] public DiceData DefaultDiceData {get; private set;}
        [field:SerializeField] public DiceData RunTimeDiceData { get; private set; } // 확인용
        [SerializeField] private DiceListUI diceListUI;
        
        private void Awake()
        {
            Instance = this;
            MakeRuntime();
        }
        public void OnViewSurfaceList()
        {
            diceListUI.SetView(RunTimeDiceData);
            diceListUI.OnView();
        }
        
        public void OffViewSurfaceList()
        {
            diceListUI.OffView();
        }
        
        private void MakeRuntime()
        {
            RunTimeDiceData = new DiceData
            {
                diceSurface = DefaultDiceData.diceSurface
            };
        }

        public void SetSurface(int surfaceIndex , DiceSurfaceDataSO changeSurfaceData)
        {
            if (surfaceIndex > RunTimeDiceData.diceSurface.Count)
                return;
            RunTimeDiceData.diceSurface[surfaceIndex] = changeSurfaceData;
        }
    }
}