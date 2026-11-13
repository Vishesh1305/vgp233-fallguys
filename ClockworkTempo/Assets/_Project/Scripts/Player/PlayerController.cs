using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Rigidbody based third person movement: camera relative input, acceleration, ground check,
    /// jump with coyote time and buffering, reduced air control and scaled airborne gravity.
    /// Input is read in Update; all forces are applied in FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerInputReader))]
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        // Probe sphere is slightly smaller than the capsule so it never starts inside a wall.
        private const float GroundProbeRadiusScale = 0.9f;

        // Probe starts this far above the bottom of the capsule so it never starts inside the floor.
        private const float GroundProbeLift = 0.05f;

        // After a jump, ground hits are ignored for this long so the jump is not cancelled on frame one.
        private const float JumpGroundLockout = 0.1f;

        // Squared input magnitude below which the stick or keys count as released.
        private const float MinMoveInputSqr = 0.0001f;

        [Header("References")]
        [Tooltip("Movement and jump tuning asset.")]
        [SerializeField] private PlayerMovementSettings _settings;

        [Tooltip("Child transform holding the meshes. Rotated to face the move direction.")]
        [SerializeField] private Transform _visual;

        [Tooltip("Camera used for camera relative movement. Uses the Main Camera when empty.")]
        [SerializeField] private Transform _cameraTransform;

        private Rigidbody _rigidbody;
        private CapsuleCollider _capsule;
        private PlayerInputReader _input;

        // Written in Update from input, consumed in FixedUpdate.
        private Vector3 _moveDirection;
        private bool _jumpHeld;
        private float _lastJumpPressedTime = float.NegativeInfinity;

        // Physics state, only touched in FixedUpdate.
        private float _lastGroundedTime = float.NegativeInfinity;
        private float _groundLockoutEndTime = float.NegativeInfinity;
        private Vector3 _groundNormal = Vector3.up;
        private bool _isJumpRising;

        /// <summary>True when the ground probe found walkable ground on the last physics step.</summary>
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();
            _input = GetComponent<PlayerInputReader>();

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_settings == null)
            {
                Debug.LogError($"{nameof(PlayerController)} on {name} has no {nameof(PlayerMovementSettings)} assigned.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            _moveDirection = GetCameraRelativeDirection(_input.MoveInput);
            _jumpHeld = _input.JumpHeld;

            if (_input.JumpPressed)
            {
                _lastJumpPressedTime = Time.time;
            }

            UpdateFacing();
        }

        private void FixedUpdate()
        {
            // Time.time returns the fixed step time inside FixedUpdate.
            float time = Time.time;
            float deltaTime = Time.fixedDeltaTime;

            UpdateGrounded(time);
            ApplyHorizontalMovement(deltaTime);
            TryJump(time);
            ApplyGravityModifiers();
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (_cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            // Flatten the camera axes so looking up or down does not slow movement.
            Vector3 forward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;

            // Clamp instead of normalize so a half pushed stick still walks at half speed.
            return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        }

        private void UpdateFacing()
        {
            if (_visual == null || _moveDirection.sqrMagnitude < MinMoveInputSqr)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
            _visual.rotation = Quaternion.RotateTowards(_visual.rotation, targetRotation, _settings.TurnSpeed * Time.deltaTime);
        }

        private void UpdateGrounded(float time)
        {
            IsGrounded = false;
            _groundNormal = Vector3.up;

            if (time < _groundLockoutEndTime)
            {
                return;
            }

            // Rotation is frozen and the root is unscaled, so the capsule centre is a plain offset.
            float probeRadius = _capsule.radius * GroundProbeRadiusScale;
            Vector3 bottomSphereCentre = _rigidbody.position + _capsule.center
                + Vector3.down * (_capsule.height * 0.5f - _capsule.radius);
            Vector3 origin = bottomSphereCentre + Vector3.up * GroundProbeLift;

            // Gap between the bottom of the probe sphere and the bottom of the capsule.
            float startGap = GroundProbeLift + (_capsule.radius - probeRadius);
            float castDistance = startGap + _settings.GroundCheckDistance;

            if (!Physics.SphereCast(origin, probeRadius, Vector3.down, out RaycastHit hit, castDistance,
                    _settings.GroundMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (Vector3.Angle(hit.normal, Vector3.up) > _settings.MaxSlopeAngle)
            {
                return;
            }

            IsGrounded = true;
            _groundNormal = hit.normal;
            _lastGroundedTime = time;
            _isJumpRising = false;
        }

        private void ApplyHorizontalMovement(float deltaTime)
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 target = _moveDirection * _settings.MaxSpeed;

            bool hasInput = _moveDirection.sqrMagnitude >= MinMoveInputSqr;
            float rate = hasInput ? _settings.Acceleration : _settings.Deceleration;
            if (!IsGrounded)
            {
                rate *= _settings.AirControl;
            }

            Vector3 newHorizontal = Vector3.MoveTowards(horizontal, target, rate * deltaTime);

            // VelocityChange adds only the difference, so contact pushes from obstacles are kept, not overwritten.
            _rigidbody.AddForce(newHorizontal - horizontal, ForceMode.VelocityChange);

            if (IsGrounded)
            {
                // The collider has zero friction, so cancel the part of gravity that would slide it down slopes.
                Vector3 slopeSlide = Vector3.ProjectOnPlane(Physics.gravity, _groundNormal);
                _rigidbody.AddForce(-slopeSlide, ForceMode.Acceleration);
            }
        }

        private void TryJump(float time)
        {
            bool jumpBuffered = time - _lastJumpPressedTime <= _settings.JumpBufferTime;
            bool withinCoyoteTime = time - _lastGroundedTime <= _settings.CoyoteTime;
            if (!jumpBuffered || !withinCoyoteTime)
            {
                return;
            }

            // v = sqrt(2gh) with the scaled rising gravity, so the peak is still exactly JumpHeight.
            float risingGravity = -Physics.gravity.y * _settings.GravityScale;
            float jumpSpeed = Mathf.Sqrt(2f * risingGravity * _settings.JumpHeight);
            Vector3 velocity = _rigidbody.linearVelocity;
            _rigidbody.AddForce(Vector3.up * (jumpSpeed - velocity.y), ForceMode.VelocityChange);

            // Consume both windows so one press gives one jump.
            _lastJumpPressedTime = float.NegativeInfinity;
            _lastGroundedTime = float.NegativeInfinity;
            _groundLockoutEndTime = time + JumpGroundLockout;
            _isJumpRising = true;
            IsGrounded = false;
        }

        private void ApplyGravityModifiers()
        {
            if (IsGrounded)
            {
                return;
            }

            float verticalSpeed = _rigidbody.linearVelocity.y;
            float multiplier = _settings.GravityScale;

            if (verticalSpeed < 0f)
            {
                multiplier *= _settings.FallGravityMultiplier;
                _isJumpRising = false;
            }
            else if (_isJumpRising && !_jumpHeld)
            {
                // Only cut our own jumps short, never upward launches from bumpers.
                multiplier *= _settings.LowJumpGravityMultiplier;
            }

            // The Rigidbody already applies 1x gravity, so only the extra part is added here.
            if (multiplier > 1f)
            {
                _rigidbody.AddForce(Physics.gravity * (multiplier - 1f), ForceMode.Acceleration);
            }

            if (verticalSpeed < -_settings.MaxFallSpeed)
            {
                _rigidbody.AddForce(Vector3.up * (-_settings.MaxFallSpeed - verticalSpeed), ForceMode.VelocityChange);
            }
        }

        private void OnDrawGizmosSelected()
        {
            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                return;
            }

            float probeRadius = capsule.radius * GroundProbeRadiusScale;
            Vector3 bottomSphereCentre = transform.position + capsule.center
                + Vector3.down * (capsule.height * 0.5f - capsule.radius);
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(bottomSphereCentre + Vector3.up * GroundProbeLift, probeRadius);
        }
    }
}
