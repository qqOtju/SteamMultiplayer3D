using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Scene.Comp
{
    public class UIComputerWindow : MonoBehaviour
    {
        private const float AnimationSpeed = 2f;
        [SerializeField] private Button _closeButton;
        [SerializeField] private RectTransform _windowTransform;

        private bool _isActive;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Close);
        }

        private void Start()
        {
            _isActive = gameObject.activeSelf;
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
        }

        public event Action OnClose;

        private void Close()
        {
            if (!_isActive) return;
            _isActive = false;
            OnClose?.Invoke();
        }

        public IEnumerator CloseAnimation()
        {
            _isActive = false;
            while (_windowTransform.localScale.x > 0)
            {
                _windowTransform.localScale = Vector3.MoveTowards(
                    _windowTransform.localScale, Vector3.zero,
                    Time.deltaTime * AnimationSpeed);
                yield return null;
            }

            _windowTransform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        public void Open()
        {
            if (_isActive) return;
            _isActive = true;
            gameObject.SetActive(true);
            StartCoroutine(OpenAnimation());
        }

        private IEnumerator OpenAnimation()
        {
            _windowTransform.localScale = Vector3.zero;
            while (_windowTransform.localScale.x < 1)
            {
                _windowTransform.localScale = Vector3.MoveTowards(
                    _windowTransform.localScale, Vector3.one,
                    Time.deltaTime * AnimationSpeed);
                yield return null;
            }

            _windowTransform.localScale = Vector3.one;
        }
    }
}