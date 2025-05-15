using UnityEngine;

namespace Project.Scripts.TEST
{
    public class PlayerMouseFollow: MonoBehaviour
    {
        [SerializeField] private Transform _headTarget;
        
        private Camera _camera;
        
        private void Start()
        {
            _camera = Camera.main;
        }
        
        private void Update()
        {
            if(_camera == null) return;
            var mousePos = Input.mousePosition;
            var screenPoint = new Vector3(mousePos.x, mousePos.y, _camera.nearClipPlane);
            var worldPoint = _camera.ScreenToWorldPoint(screenPoint);
            _headTarget.position = worldPoint;
        }
    }
}