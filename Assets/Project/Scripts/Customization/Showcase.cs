using System;
using Mirror;
using UnityEngine;

namespace Project.Scripts.Customization
{
    public class Showcase: NetworkBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private NetworkCustomizationPlayer _player;

        private RenderTexture _renderTexture;
        
        public NetworkCustomizationPlayer Player => _player;
        public RenderTexture RenderTexture => _renderTexture;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _renderTexture = new RenderTexture(1024, 1024, 24)
            {
                depth = 24,
                name = "ShowcaseRenderTexture_" + GetInstanceID()
            };
            _renderTexture.Create();
            _camera.targetTexture = _renderTexture;
        }

        private void OnDestroy()
        {
            if(_renderTexture != null) _renderTexture.Release();
        }
    }
}