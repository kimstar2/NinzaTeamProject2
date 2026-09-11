using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.Services
{
    [Serializable]
    public struct GetPlayerTrmStruct   
    {
        [field:SerializeField] public Transform TargetTrm {get; private set;}
        [field:SerializeField] public PlayerType PlayerType { get; private set; }
    }
    public class GetPlayerTrmService : MonoBehaviour , IGetPlayerTrmService
    {
        [SerializeField] private List<GetPlayerTrmStruct> targetTransforms;
        private void Awake()
        {
            ServiceLocator.Register<IGetPlayerTrmService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IGetPlayerTrmService>();
        }
        
        public Transform GetPlayerTrm(PlayerType playerType)
        {
            GetPlayerTrmStruct getPlayerTrm = targetTransforms.
                Where(s => s.PlayerType == playerType).
                First(s=>s.TargetTrm);
            if (getPlayerTrm.TargetTrm == null)
                Debug.Log("해당 플레이어는 존재하지 않습니다.");
            return getPlayerTrm.TargetTrm;
        }
    }
}