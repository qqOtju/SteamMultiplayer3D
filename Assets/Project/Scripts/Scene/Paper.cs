using Project.Scripts.Character;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

namespace Project.Scripts.Scene
{
    public class Paper: MonoBehaviour, IInteractable
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Transform _inputDisplay;
        
        private const float InteractionDistance = 2f;
        
        private Player _player;
        private bool _isActive;

        public void InitializeLocalPlayer(Player player)
        {
            Debug.Log("Initializing local player");
            _player = player;
        }

        private void Update()
        {
            if(_player == null) return;
            if (_isActive)
            {
                if (!Input.GetKeyDown(KeyCode.E)) return;
                _camera.Priority = 0;
                _isActive = false;
                _player.EnablePlayerControls(true);
                return;
            }
            _inputDisplay.transform.LookAt(_playerCamera.transform.position);
            if (Vector3.Distance(_player.transform.position,
                    transform.position) < InteractionDistance)
            {
                _inputDisplay.gameObject.SetActive(true);
                if (!Input.GetKeyDown(KeyCode.E)) return;
                _player.EnablePlayerControls(false);
                _camera.Priority = 50;
                _isActive = true;
            }
            else
                _inputDisplay.gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, InteractionDistance);
        }
    }
}