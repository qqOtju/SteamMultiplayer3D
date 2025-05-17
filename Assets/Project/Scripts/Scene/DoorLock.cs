using System.Collections;
using Mirror;
using Project.Scripts.Character;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Scene
{
    public class DoorLock : NetworkBehaviour, IInteractable
    {
        private const float InteractionDistance = 2f;
        [SerializeField] private TMP_Text _codeText;
        [SerializeField] private Transform _inputDisplay;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Transform _doorTransform;
        [SerializeField] private Transform _doorTarget;

        public readonly int[] CorrectCode = { 1, 2, 3, 4 };

        private bool _interactable = true;
        private bool _isActive;
        private Player _player;

        private void Update()
        {
            if (_isActive || !_interactable || _player == null) return;
            _inputDisplay.transform.LookAt(_playerCamera.transform.position);
            if (Vector3.Distance(_player.transform.position, transform.position) < InteractionDistance)
            {
                _inputDisplay.gameObject.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    _player.DoorLock.Connect(this);
                    _isActive = true;
                    _player.DoorLock.OnCodeChanged += OnCodeChanged;
                    _player.DoorLock.OnClose += OnClose;
                }
            }
            else
            {
                _inputDisplay.gameObject.SetActive(false);
            }
        }

        public void InitializeLocalPlayer(Player player)
        {
            _player = player;
        }

        private void OnCodeChanged(string obj)
        {
            _codeText.text = obj;
        }

        private IEnumerator OpenDoor()
        {
            while (Vector3.Distance(_doorTransform.position, _doorTarget.position) > 0.1f)
            {
                _doorTransform.position = Vector3.MoveTowards(_doorTransform.position,
                    _doorTarget.position, Time.deltaTime);
                yield return null;
            }
        }

        [ClientRpc]
        public void RpcOpen()
        {
            StartCoroutine(OpenDoor());
            _interactable = false;
            _inputDisplay.gameObject.SetActive(false);
        }

        private void OnClose()
        {
            _isActive = false;
            _player.DoorLock.OnCodeChanged -= OnCodeChanged;
            _player.DoorLock.OnClose -= OnClose;
            _inputDisplay.gameObject.SetActive(false);
        }
    }
}