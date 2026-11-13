using System;
using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Owns the generated PlayerControls instance and exposes gameplay input as per frame state.
    /// Other scripts read input through this component and never touch the Input System directly.
    /// Look is not exposed here: the Cinemachine camera reads it straight from the actions asset.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private PlayerControls _controls;

        /// <summary>Movement input, each axis in [-1, 1]. X is strafe, Y is forward.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>True only on the frame the jump button went down.</summary>
        public bool JumpPressed { get; private set; }

        /// <summary>True on every frame the jump button is held.</summary>
        public bool JumpHeld { get; private set; }

        /// <summary>Raised on the frame the pause button goes down.</summary>
        public event Action PausePressed;

        private void Awake()
        {
            _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            _controls.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _controls.Gameplay.Disable();

            // Clear cached state so nothing reads a stale held input while this component is off.
            MoveInput = Vector2.zero;
            JumpPressed = false;
            JumpHeld = false;
        }

        private void OnDestroy()
        {
            _controls.Dispose();
        }

        private void Update()
        {
            PlayerControls.GameplayActions gameplay = _controls.Gameplay;

            MoveInput = gameplay.Move.ReadValue<Vector2>();
            JumpPressed = gameplay.Jump.WasPressedThisFrame();
            JumpHeld = gameplay.Jump.IsPressed();

            if (gameplay.Pause.WasPressedThisFrame())
            {
                PausePressed?.Invoke();
            }
        }
    }
}
