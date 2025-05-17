using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.Character
{
    public class PlayerInput
    {
        public PlayerInput()
        {
            InputActions = new InputSystem_Actions();
            InputActions.Player.Move.performed += SetMoveInput;
            InputActions.Player.Move.canceled += SetMoveInput;
            InputActions.Player.Look.performed += SetLookInput;
            InputActions.Player.Look.canceled += SetLookInput;
            InputActions.Player.Sprint.performed += SetRunInput;
            InputActions.Player.Sprint.canceled += SetRunInput;
            InputActions.Player.Crouch.performed += SetCrouchInput;
            InputActions.Player.Crouch.canceled += SetCrouchInput;
            InputActions.Player.Zoom.performed += SetZoomInput;
            InputActions.Player.Zoom.canceled += SetZoomInput;
        }

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsRunning { get; private set; }

        public bool IsCrouching { get; private set; }
        public Vector2 ZoomInput { get; private set; }

        public InputSystem_Actions InputActions { get; }

        public event Action<bool> OnCrouch;

        public void OnDestroy()
        {
            InputActions.Player.Move.performed -= SetMoveInput;
            InputActions.Player.Move.canceled -= SetMoveInput;
            InputActions.Player.Look.performed -= SetLookInput;
            InputActions.Player.Look.canceled -= SetLookInput;
            InputActions.Player.Sprint.performed -= SetRunInput;
            InputActions.Player.Sprint.canceled -= SetRunInput;
            InputActions.Player.Crouch.performed -= SetCrouchInput;
            InputActions.Player.Crouch.canceled -= SetCrouchInput;
        }

        private void SetMoveInput(InputAction.CallbackContext obj)
        {
            MoveInput = obj.ReadValue<Vector2>();
        }

        private void SetLookInput(InputAction.CallbackContext obj)
        {
            LookInput = obj.ReadValue<Vector2>();
        }

        private void SetRunInput(InputAction.CallbackContext obj)
        {
            IsRunning = obj.performed;
        }

        private void SetCrouchInput(InputAction.CallbackContext obj)
        {
            IsCrouching = obj.performed;
            OnCrouch?.Invoke(IsCrouching);
        }

        private void SetZoomInput(InputAction.CallbackContext obj)
        {
            ZoomInput = obj.ReadValue<Vector2>();
        }
    }
}