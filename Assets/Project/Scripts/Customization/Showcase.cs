using Mirror;
using UnityEngine;

namespace Project.Scripts.Customization
{
    public class Showcase : NetworkBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private NetworkCustomizationPlayer _player;

        public NetworkCustomizationPlayer Player => _player;
        public RenderTexture RenderTexture { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (RenderTexture != null) RenderTexture.Release();
        }

        private void Initialize()
        {
            RenderTexture = new RenderTexture(1024, 1024, 24)
            {
                depth = 24,
                name = "ShowcaseRenderTexture_" + GetInstanceID()
            };
            RenderTexture.Create();
            _camera.targetTexture = RenderTexture;
        }
    }
}