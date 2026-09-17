using Unity.Cinemachine;

namespace Members.KJY._01.Scripts.Service
{
    public interface IGetCurrentCam
    {
        int ID {get; }
        public CinemachineCamera CineCam {get; }
        public void SetValue(float v);
    }
}