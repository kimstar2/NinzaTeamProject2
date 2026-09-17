using System;
using DevLib.ServiceLocator;
using Unity.Cinemachine;
using UnityEngine;

namespace Members.KJY._01.Scripts.Service
{
    public class GetCurrentCam : MonoBehaviour , IGetCurrentCam
    {
        public int ID { get; private set; }
        public CinemachineCamera CineCam {get; private set;}

        private void Awake()
        {
            ID = transform.GetInstanceID();
            CineCam = GetComponent<CinemachineCamera>();
            ServiceLocator.Register<IGetCurrentCam>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IGetCurrentCam>(this);
        }

        public void SetValue(float v) => CineCam.Lens.OrthographicSize = v;
    }
}