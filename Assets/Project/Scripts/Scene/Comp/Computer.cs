using Mirror;
using Project.Scripts.Character;
using Unity.Cinemachine;
using UnityEngine;

namespace Project.Scripts.Scene.Comp
{
    public class Computer : NetworkBehaviour, IInteractable
    {
        private const float InteractionDistance = 4f;
        [SerializeField] private UIComputer _uiComputer;
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private Transform _inputDisplay;
        [SerializeField] private Camera _playerCamera;

        private bool _isActive;
        private Player _player;

        private void Start()
        {
            _uiComputer.Init(_playerCamera);
        }

        private void Update()
        {
            if (_player == null) return;
            if (_isActive)
            {
                if (!Input.GetKeyDown(KeyCode.Escape)) return;
                Exit();
                return;
            }

            _inputDisplay.transform.LookAt(_playerCamera.transform.position);
            if (Vector3.Distance(_player.transform.position, transform.position) < InteractionDistance)
            {
                _inputDisplay.gameObject.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                    Use();
            }
            else
            {
                _inputDisplay.gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, InteractionDistance);
        }

        public void InitializeLocalPlayer(Player player)
        {
            _player = player;
        }

        private void Exit()
        {
            _camera.Priority = 0;
            _isActive = false;
            _player.EnablePlayerControls(true);
            _uiComputer.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Use()
        {
            _camera.Priority = 50;
            _isActive = true;
            _player.EnablePlayerControls(false);
            _uiComputer.SetActive(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}