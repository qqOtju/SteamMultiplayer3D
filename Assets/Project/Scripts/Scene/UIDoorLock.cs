using System;
using System.Collections.Generic;
using Mirror;
using Project.Scripts.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Scene
{
    [RequireComponent(typeof(Canvas))]
    public class UIDoorLock : NetworkBehaviour, IInteractable
    {
        [SerializeField] private TMP_Text _displayText;
        [SerializeField] private Button[] _buttons;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _enterButton;
        [SerializeField] private Button _backButton;
        private Canvas _canvas;

        private DoorLock _currentDoorLock;

        private readonly List<int> _inputCode = new();
        private Player _player;

        private void Awake()
        {
            for (var index = 0; index < _buttons.Length; index++)
            {
                var button = _buttons[index];
                button.GetComponentInChildren<TMP_Text>().text = index.ToString();
                var index1 = index;
                button.onClick.AddListener(() => OnButtonClick(index1));
            }

            _clearButton.onClick.AddListener(OnClearButtonClick);
            _enterButton.onClick.AddListener(OnEnterButtonClick);
            _backButton.onClick.AddListener(Close);
        }

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }


        private void OnDestroy()
        {
            foreach (var button in _buttons)
                button.onClick.RemoveAllListeners();
            _clearButton.onClick.RemoveAllListeners();
            _enterButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }

        public void InitializeLocalPlayer(Player player)
        {
            _player = player;
        }

        public event Action<string> OnCodeChanged;
        public event Action OnClose;

        private void OnButtonClick(int index)
        {
            if (_inputCode.Count >= 4) return;
            _displayText.text += $"{index} ";
            OnCodeChanged?.Invoke(_displayText.text);
            _inputCode.Add(index);
        }

        private void OnClearButtonClick()
        {
            _displayText.text = string.Empty;
            _inputCode.Clear();
        }

        private void OnEnterButtonClick()
        {
            if (IsCodeCorrect())
            {
                Debug.Log("Door Unlocked!");
                CmdOpenDoor(_currentDoorLock);
                // Add logic to unlock the door
            }
            else
            {
                Debug.Log("Incorrect Code");
                OnClearButtonClick();
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdOpenDoor(NetworkBehaviour doorLock)
        {
            doorLock.GetComponent<DoorLock>().RpcOpen();
        }

        private void Close()
        {
            _canvas.enabled = false;
            _player.EnablePlayerControls(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            OnClose?.Invoke();
        }

        private bool IsCodeCorrect()
        {
            for (var i = 0; i < _currentDoorLock.CorrectCode.Length; i++)
                if (_inputCode[i] != _currentDoorLock.CorrectCode[i])
                    return false;
            return true;
        }

        public void Connect(DoorLock doorLock)
        {
            _canvas.enabled = true;
            _currentDoorLock = doorLock;
            _player.EnablePlayerControls(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}