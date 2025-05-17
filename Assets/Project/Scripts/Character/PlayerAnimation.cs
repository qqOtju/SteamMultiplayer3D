using System.Collections;
using UnityEngine;

namespace Project.Scripts.Character
{
    public class PlayerAnimation
    {
        private const float BlinkEyeCloseTime = 0.15f;
        private const float BlinkOpeningTime = 0.06f;
        private const float BlinkClosingTime = 0.1f;
        private const float BlinkInterval = 3.5f;
        private const float RotationSpeed = 10f;
        private const float MinYaw = -105f;
        private const float MaxYaw = 105f;
        private static readonly int SpeedAnimationHash = Animator.StringToHash("Speed");
        private static readonly int CrouchAnimationHash = Animator.StringToHash("Crouch");
        private readonly Animator _animator;
        private readonly int _blendShapeIndex;
        private readonly Transform _cameraTarget;

        private readonly SkinnedMeshRenderer _headMeshRenderer;
        private readonly Transform _lookAtTarget;
        private readonly PlayerInput _playerInput;
        private readonly Transform _viewTransform;

        private Vector3 _currentVelocitySmooth;

        public PlayerAnimation(SkinnedMeshRenderer headMeshRenderer, Animator animator,
            PlayerInput playerInput, Transform viewTransform,
            Transform lookAtTarget, Transform cameraTarget)
        {
            _headMeshRenderer = headMeshRenderer;
            _animator = animator;
            _playerInput = playerInput;
            _viewTransform = viewTransform;
            _lookAtTarget = lookAtTarget;
            _cameraTarget = cameraTarget;
            _blendShapeIndex = GetBlendShapeIndex(_headMeshRenderer,
                "eyes_closed.01");
            playerInput.OnCrouch += SetCrouchAnimation;
        }

        public void OnDestroy()
        {
            _playerInput.OnCrouch -= SetCrouchAnimation;
        }

        private int GetBlendShapeIndex(SkinnedMeshRenderer meshRenderer,
            string blendShapeName)
        {
            var mesh = meshRenderer.sharedMesh;
            var index = mesh.GetBlendShapeIndex(blendShapeName);
            return index;
        }

        private void SetCrouchAnimation(bool obj)
        {
            _animator.SetBool(CrouchAnimationHash, obj);
        }

        private IEnumerator Blink()
        {
            var time = 0f;
            while (time <= BlinkInterval)
            {
                time += Time.deltaTime;
                yield return null;
            }

            time = 0f;
            while (time <= BlinkClosingTime)
            {
                time += Time.deltaTime;
                var t = time / BlinkEyeCloseTime;
                _headMeshRenderer.SetBlendShapeWeight(_blendShapeIndex,
                    Mathf.Lerp(0f, 100f, t));
                yield return null;
            }

            _headMeshRenderer.SetBlendShapeWeight(_blendShapeIndex, 100f);
            time = 0f;
            while (time <= BlinkEyeCloseTime)
            {
                time += Time.deltaTime;
                yield return null;
            }

            time = 0f;
            while (time <= BlinkOpeningTime)
            {
                time += Time.deltaTime;
                var t = time / BlinkOpeningTime;
                _headMeshRenderer.SetBlendShapeWeight(_blendShapeIndex, Mathf.Lerp(100f, 0f, t));
                yield return null;
            }

            _headMeshRenderer.SetBlendShapeWeight(_blendShapeIndex, 0f);
        }

        public IEnumerator BlinkCoroutine()
        {
            while (true)
                yield return Blink();
        }

        public void SetAnimation(Vector3 currentVelocity)
        {
            var horizontalSpeed = new Vector3(currentVelocity.x, 0f,
                currentVelocity.z).magnitude;
            _animator.SetFloat(SpeedAnimationHash, horizontalSpeed / Player.WalkSpeed);
        }

        public void RotateView(Vector3 moveDirection)
        {
            if (moveDirection == Vector3.zero) return;
            var targetRotation = Quaternion.LookRotation(moveDirection);
            var rotation = Quaternion.Slerp(_viewTransform.rotation,
                targetRotation, RotationSpeed * Time.deltaTime);
            rotation.x = 0f;
            rotation.z = 0f;
            _viewTransform.rotation = rotation; //
        }

        public void SetIKTargets()
        {
            var cameraForward = _cameraTarget.forward;
            var flatForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            var forward = _viewTransform.forward;
            var viewForward = new Vector3(forward.x, 0f, forward.z).normalized;
            var yawAngle = Vector3.SignedAngle(viewForward, flatForward, Vector3.up);
            yawAngle = Mathf.Clamp(yawAngle, MinYaw, MaxYaw);
            var yawRotation = Quaternion.AngleAxis(yawAngle, Vector3.up);
            var clampedWorldDir = yawRotation * forward;
            var finalDir = clampedWorldDir.normalized;
            finalDir.y = cameraForward.y;
            var targetPosition = _cameraTarget.position + finalDir.normalized * 2f;
            _lookAtTarget.position =
                Vector3.SmoothDamp(_lookAtTarget.position, targetPosition, ref _currentVelocitySmooth, 0.1f);
        }
    }
}