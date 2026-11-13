using System;
using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Keeps the player's current respawn pose and teleports the player there when a KillZone reports a fall.
    /// The pose starts at the Start Point; checkpoints move it forward through SetRespawnPoint.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlayerController))]
    [DisallowMultipleComponent]
    public sealed class PlayerRespawner : MonoBehaviour
    {
        [Header("Spawn")]
        [Tooltip("Start Point used until the first checkpoint is reached. The player is placed here when the scene starts.")]
        [SerializeField] private Transform _startPoint;

        [Tooltip("Height above the spawn point the player appears at, so it never starts inside the floor.")]
        [SerializeField, Min(0f)] private float _spawnLift = 0.1f;

        private Rigidbody _rigidbody;
        private PlayerController _controller;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;

        /// <summary>Raised after the player has been moved back to the respawn point.</summary>
        public static event Action Respawned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Respawned = null;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _controller = GetComponent<PlayerController>();

            Transform spawn = _startPoint != null ? _startPoint : transform;
            SetRespawnPoint(spawn.position, spawn.rotation);
        }

        private void OnEnable()
        {
            KillZone.PlayerEntered += Respawn;
            CheckpointTracker.CheckpointReached += HandleCheckpointReached;
        }

        private void OnDisable()
        {
            KillZone.PlayerEntered -= Respawn;
            CheckpointTracker.CheckpointReached -= HandleCheckpointReached;
        }

        private void Start()
        {
            // The Start Point, not wherever the prefab was dropped in the scene, decides where the race begins.
            Teleport();
        }

        /// <summary>Sets where the next respawn happens. Only yaw is kept so the player always stands upright.</summary>
        /// <param name="position">Feet position of the respawn point.</param>
        /// <param name="rotation">Facing of the respawn point.</param>
        public void SetRespawnPoint(Vector3 position, Quaternion rotation)
        {
            _spawnPosition = position;
            _spawnRotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
        }

        /// <summary>Moves the player to the respawn point with no velocity and raises Respawned.</summary>
        public void Respawn()
        {
            Teleport();
            Respawned?.Invoke();
        }

        private void HandleCheckpointReached(Checkpoint checkpoint)
        {
            SetRespawnPoint(checkpoint.SpawnPosition, checkpoint.SpawnRotation);
        }

        private void Teleport()
        {
            Vector3 position = _spawnPosition + Vector3.up * _spawnLift;

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = position;

            // Move the transform as well so interpolation does not draw one frame between the old and new position.
            transform.position = position;

            _controller.PrepareForRespawn(_spawnRotation);
        }
    }
}
