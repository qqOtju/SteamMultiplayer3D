using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Scene.Comp
{
    public class UIComputerWindowGif: UIComputerWindow
    {
        [SerializeField] private Image _uiImage;
        [SerializeField] private Sprite[] _gifFrames;
        [SerializeField] private float _frameRate = 10f;

        private int _currentFrame;
        private float _timer;

        private void Update()
        {
            if (_gifFrames == null || _gifFrames.Length == 0 || _uiImage == null)
                return;
            _timer += Time.deltaTime;
            if (_timer >= 1f / _frameRate)
            {
                _timer -= 1f / _frameRate;
                _currentFrame = (_currentFrame + 1) % _gifFrames.Length;
                _uiImage.sprite = _gifFrames[_currentFrame];
            }
        }
    }
}