using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Project.Scripts.Scene.Comp
{
    public class UIComputer : NetworkBehaviour
    {
        private const int CursorUpdateInterval = 50;
        [SerializeField] private LabelWindow[] _labelWindows;
        [SerializeField] private RectTransform _uiCursor;
        [SerializeField] private Canvas _worldCanvas;

        private int _cursorUpdateIndex;
        private bool _isActive;
        private Camera _playerCamera;

        private void Awake()
        {
            for (var index = 0; index < _labelWindows.Length; index++)
            {
                var labelWindow = _labelWindows[index];
                var windowIndex = index;
                labelWindow.LabelButton.onClick.AddListener(() => CmdOpenWindow(windowIndex));
                labelWindow.Window.OnClose += () => CmdCloseWindow(windowIndex);
            }
        }

        private void Start()
        {
            _worldCanvas = GetComponent<Canvas>();
        }

        private void Update()
        {
            if (_isActive)
            {
                var point = UpdateCursor();
                _cursorUpdateIndex++;
                if (_cursorUpdateIndex >= CursorUpdateInterval)
                {
                    _cursorUpdateIndex = 0;
                    CmdUpdateCursor(point);
                }
            }
        }

        private void OnDestroy()
        {
            for (var index = 0; index < _labelWindows.Length; index++)
            {
                var labelWindow = _labelWindows[index];
                labelWindow.LabelButton.onClick.RemoveAllListeners();
                // labelWindow.Window.OnClose -= CmdCloseWindow;
            }
        }

        public void Init(Camera playerCamera)
        {
            _playerCamera = playerCamera;
        }

        private Vector2 UpdateCursor()
        {
            var mousePos = Mouse.current.position.ReadValue();
            if (_playerCamera == null || _uiCursor == null)
                return Vector2.zero;
            var cursorParent = _uiCursor.parent as RectTransform;
            if (cursorParent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    cursorParent,
                    mousePos,
                    _playerCamera,
                    out var localPoint
                );
                _uiCursor.localPosition = localPoint;
                return localPoint;
            }

            var worldPos =
                _playerCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _playerCamera.nearClipPlane));
            _uiCursor.position = worldPos;
            return worldPos;
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateCursor(Vector3 localPoint)
        {
            RpcUpdateCursor(localPoint);
        }

        [ClientRpc]
        private void RpcUpdateCursor(Vector3 localPoint)
        {
            _uiCursor.localPosition = localPoint;
        }

        [Command(requiresAuthority = false)]
        private void CmdOpenWindow(int windowIndex)
        {
            RpcOpenWindow(windowIndex);
        }

        [ClientRpc]
        private void RpcOpenWindow(int windowIndex)
        {
            _labelWindows[windowIndex].Window.Open();
        }

        [Command(requiresAuthority = false)]
        private void CmdCloseWindow(int windowIndex)
        {
            RpcCloseWindow(windowIndex);
        }

        [ClientRpc]
        private void RpcCloseWindow(int windowIndex)
        {
            StartCoroutine(_labelWindows[windowIndex].Window.CloseAnimation());
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;
        }
    }

    [Serializable]
    public struct LabelWindow
    {
        [SerializeField] private Button _labelButton;
        [SerializeField] private UIComputerWindow _window;

        public Button LabelButton => _labelButton;
        public UIComputerWindow Window => _window;
    }
}