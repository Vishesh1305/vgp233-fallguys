using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Tuning values for PlayerController. Stored as an asset so changes made during Play mode persist.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Clockwork Tempo/Player Movement Settings")]
    public sealed class PlayerMovementSettings : ScriptableObject
    {
        [Header("Ground Movement")]
        [Tooltip("Top horizontal speed in metres per second.")]
        [SerializeField, Min(0f)] private float _maxSpeed = 8f;

        [Tooltip("How fast horizontal speed rises toward the input direction, in m/s per second.")]
        [SerializeField, Min(0f)] private float _acceleration = 60f;

        [Tooltip("How fast horizontal speed falls to zero with no input, in m/s per second.")]
        [SerializeField, Min(0f)] private float _deceleration = 70f;

        [Tooltip("Degrees per second the model turns toward the move direction.")]
        [SerializeField, Min(0f)] private float _turnSpeed = 720f;

        [Header("Air Movement")]
        [Tooltip("Multiplier on acceleration and deceleration while airborne. Lower keeps momentum from knockbacks.")]
        [SerializeField, Range(0f, 1f)] private float _airControl = 0.4f;

        [Header("Jump")]
        [Tooltip("Peak jump height in metres under normal gravity.")]
        [SerializeField, Min(0f)] private float _jumpHeight = 2.2f;

        [Tooltip("Seconds after leaving a ledge during which a jump is still allowed.")]
        [SerializeField, Min(0f)] private float _coyoteTime = 0.12f;

        [Tooltip("Seconds a jump press is remembered before landing.")]
        [SerializeField, Min(0f)] private float _jumpBufferTime = 0.12f;

        [Header("Gravity")]
        [Tooltip("Base gravity multiplier while airborne. Shortens airtime without changing jump height.")]
        [SerializeField, Min(1f)] private float _gravityScale = 2.5f;

        [Tooltip("Extra gravity multiplier while falling, on top of Gravity Scale. Above 1 makes jumps less floaty.")]
        [SerializeField, Min(1f)] private float _fallGravityMultiplier = 1.6f;

        [Tooltip("Extra gravity multiplier while rising after jump is released early, on top of Gravity Scale. Gives variable jump height.")]
        [SerializeField, Min(1f)] private float _lowJumpGravityMultiplier = 2f;

        [Tooltip("Maximum downward speed in metres per second.")]
        [SerializeField, Min(0f)] private float _maxFallSpeed = 30f;

        [Header("Ground Check")]
        [Tooltip("Layers the player can stand on.")]
        [SerializeField] private LayerMask _groundMask = ~0;

        [Tooltip("Extra distance below the feet that still counts as grounded.")]
        [SerializeField, Min(0f)] private float _groundCheckDistance = 0.15f;

        [Tooltip("Steepest slope in degrees that counts as ground.")]
        [SerializeField, Range(0f, 89f)] private float _maxSlopeAngle = 45f;

        /// <summary>Top horizontal speed in m/s.</summary>
        public float MaxSpeed => _maxSpeed;

        /// <summary>Horizontal acceleration in m/s per second.</summary>
        public float Acceleration => _acceleration;

        /// <summary>Horizontal deceleration in m/s per second.</summary>
        public float Deceleration => _deceleration;

        /// <summary>Model turn rate in degrees per second.</summary>
        public float TurnSpeed => _turnSpeed;

        /// <summary>Airborne multiplier on acceleration and deceleration.</summary>
        public float AirControl => _airControl;

        /// <summary>Peak jump height in metres.</summary>
        public float JumpHeight => _jumpHeight;

        /// <summary>Coyote time window in seconds.</summary>
        public float CoyoteTime => _coyoteTime;

        /// <summary>Jump buffer window in seconds.</summary>
        public float JumpBufferTime => _jumpBufferTime;

        /// <summary>Base airborne gravity multiplier.</summary>
        public float GravityScale => _gravityScale;

        /// <summary>Gravity multiplier while falling.</summary>
        public float FallGravityMultiplier => _fallGravityMultiplier;

        /// <summary>Gravity multiplier while rising with jump released.</summary>
        public float LowJumpGravityMultiplier => _lowJumpGravityMultiplier;

        /// <summary>Maximum downward speed in m/s.</summary>
        public float MaxFallSpeed => _maxFallSpeed;

        /// <summary>Layers that count as ground.</summary>
        public LayerMask GroundMask => _groundMask;

        /// <summary>Ground probe distance below the feet in metres.</summary>
        public float GroundCheckDistance => _groundCheckDistance;

        /// <summary>Steepest walkable slope in degrees.</summary>
        public float MaxSlopeAngle => _maxSlopeAngle;
    }
}
