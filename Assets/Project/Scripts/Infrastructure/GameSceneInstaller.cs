using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Project.Scripts.Infrastructure
{
    public class GameSceneInstaller: MonoInstaller
    {
        [SerializeField] private CinemachineBrain _cinemachineBrain;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        
        public override void InstallBindings()
        {
            BindCinemachineBrain();
            BindCinemachineCamera();
        }

        private void BindCinemachineBrain()
        {
            Container.Bind<CinemachineBrain>().FromInstance(_cinemachineBrain).AsSingle();
        }

        private void BindCinemachineCamera()
        {
            Container.Bind<CinemachineCamera>().FromInstance(_cinemachineCamera).AsSingle();
        }
    }
}