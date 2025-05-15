using Mirror;
using Project.Scripts.Customization;
using Project.Scripts.Scene;
using Project.Scripts.UI.MainMenu;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Project.Scripts.Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _viewTransform;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private Transform _lookAtTarget;
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

        private const float CameraTargetRotationSpeed = 7f;
        private const float MinPitch = -40f;
        private const float MaxPitch = 80f;
        private const float RunSpeed = 3.7f;
        private const float CrouchSpeed = 1.2f;
        private const float MinZoom = 1.25f;
		private const float ZoomSpeed = 17.5f;
        
        public const float WalkSpeed = 1.6f;
        
        private PlayerAnimation _playerAnimation;
        private CinemachineBrain _cinemachineBrain;
        private CinemachineFollowZoom _cinemachineFollowZoom;
        private Coroutine _blinkCoroutine;
        private PlayerInput _playerInput;
        private Vector3 _currentVelocity;
        private Vector3 _moveDirection;
        private UIDoorLock _doorLock;
        private SkinData _skinData;
        private Rigidbody _rb;
        private float _pitch;
        private float _yaw;

        public UIDoorLock DoorLock => _doorLock;
        public Transform CameraTarget => _cameraTarget;

        [Inject]
        private void Construct(SkinData skinData, CinemachineBrain cinemachineBrain, CinemachineCamera cinemachineCamera)
        {
            _skinData  = skinData;
            _cinemachineBrain = cinemachineBrain;
            _cinemachineFollowZoom = cinemachineCamera.gameObject.GetComponent<CinemachineFollowZoom>();
            //Fix skin setup & syncronization, so I can delete this code and just use NetworkCustomizationPlayer, where all the skin setup will be done
            if (isOwned)
            {
                //I can call this here because OnStartAuthority is called before injection
                GetComponent<NetworkCustomizationPlayer>().ClientSetSkin(UILobbyPlayer.ConvertToDto(_skinData));
            }
            Debug.Log("Player constructed");
        }
        
        private void Awake()
        {            
            _playerInput = new PlayerInput();
        }

        private void OnEnable()
        {
            if(!isOwned) return;
            _playerInput.InputActions.Enable();//
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _playerAnimation = new PlayerAnimation(_skinnedMeshRenderer, 
                _animator, _playerInput, _viewTransform, _lookAtTarget, _cameraTarget);
            Cursor.lockState = CursorLockMode.Locked;
            _blinkCoroutine = StartCoroutine(_playerAnimation.BlinkCoroutine());
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            if(!isOwned)
            {
                Debug.Log("OnStartAuthority player is not owned");
                _playerInput.InputActions.Disable();
                return;
            }
            Debug.Log("OnStartAuthority player is owned");
            _playerInput.InputActions.Enable();
        }

        private void OnDisable()
        {
            if(!isOwned) return;
            _playerInput.InputActions.Disable();
            _playerAnimation.OnDestroy();
        }

        private void OnDestroy()
        {
            StopCoroutine(_blinkCoroutine);
            _playerInput.OnDestroy();
        }

        private void Update()
        {
            SetMoveDirection();
			if(isOwned)
            {
                _playerAnimation.SetAnimation(_currentVelocity);
                RotateCameraTarget();//
                Zoom();
            }
            _playerAnimation.SetIKTargets();
        }

        private void FixedUpdate()
        {
            _playerAnimation.RotateView(_moveDirection);
            if(!isOwned) return;
            Move();
        }
        
        private void LateUpdate()
        {
            if(!isOwned) return;
            if (_cinemachineBrain != null)
                _cinemachineBrain.ManualUpdate();//
        }

        private void RotateCameraTarget()
        {
            if(Cursor.lockState != CursorLockMode.Locked) return;
            _yaw += _playerInput.LookInput.x * CameraTargetRotationSpeed * Time.fixedDeltaTime;//Here was fixedDeltaTime, now just deltaTime
            _pitch -= _playerInput.LookInput.y * CameraTargetRotationSpeed * Time.fixedDeltaTime;
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
            _cameraTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void SetMoveDirection()
        {
            var forward = _cameraTarget.forward;
            var right = _cameraTarget.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            _moveDirection = (forward * _playerInput.MoveInput.y + right * _playerInput.MoveInput.x).normalized;
        }

        private void Move()
        {
            var targetVelocity = _moveDirection;
            if(_playerInput.IsRunning)
                targetVelocity *= RunSpeed;
            else if(_playerInput.IsCrouching)
                targetVelocity *= CrouchSpeed;
            else
                targetVelocity *= WalkSpeed;
            _currentVelocity = targetVelocity;
            _rb.MovePosition(_rb.position + _currentVelocity * Time.fixedDeltaTime);
        }

        private void Zoom()
        {
            if(_cinemachineFollowZoom.Width <= MinZoom) _cinemachineFollowZoom.Width = MinZoom;
            _cinemachineFollowZoom.Width -= _playerInput.ZoomInput.y * Time.deltaTime * ZoomSpeed;
        }
        
        public void EnablePlayerControls(bool enabledControls)
        {
            if (enabledControls)
                _playerInput.InputActions.Player.Enable();
            else
                _playerInput.InputActions.Player.Disable();
        }
        
        public void SetUIDoorLock(UIDoorLock doorLock)
        {
            _doorLock = doorLock;
        }
    }
}