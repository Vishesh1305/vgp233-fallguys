using System;
using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Trigger volume over a checkpoint pad. Reports when the player enters and shows whether it is active.
    /// CheckpointTracker decides if entering it counts (forward only).
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent]
    public sealed class Checkpoint : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [Header("Order")]
        [Tooltip("Position along the course. 1 is the first checkpoint after the Start Point; higher is further along.")]
        [SerializeField, Min(1)] private int _index = 1;

        [Header("Spawn")]
        [Tooltip("Where the player respawns after reaching this checkpoint. Uses this object when empty.")]
        [SerializeField] private Transform _spawnPoint;

        [Header("Feedback")]
        [Tooltip("Renderer whose material shows the checkpoint state.")]
        [SerializeField] private Renderer _indicator;

        [Tooltip("Material while the checkpoint has not been reached.")]
        [SerializeField] private Material _inactiveMaterial;

        [Tooltip("Material once the checkpoint has been reached.")]
        [SerializeField] private Material _activeMaterial;

        /// <summary>Raised when the player's collider enters this checkpoint's trigger.</summary>
        public event Action<Checkpoint> PlayerEntered;

        /// <summary>Position along the course; higher means further.</summary>
        public int Index => _index;

        /// <summary>True once this checkpoint has been reached.</summary>
        public bool IsActive { get; private set; }

        /// <summary>World position the player respawns at.</summary>
        public Vector3 SpawnPosition => SpawnTransform.position;

        /// <summary>World rotation the player faces after respawning.</summary>
        public Quaternion SpawnRotation => SpawnTransform.rotation;

        private Transform SpawnTransform => _spawnPoint != null ? _spawnPoint : transform;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            ApplyVisual(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PlayerTag))
            {
                PlayerEntered?.Invoke(this);
            }
        }

        /// <summary>Marks the checkpoint as reached and switches its indicator. Safe to call more than once.</summary>
        public void Activate()
        {
            if (IsActive)
            {
                return;
            }

            IsActive = true;
            ApplyVisual(true);
        }

        private void ApplyVisual(bool active)
        {
            Material material = active ? _activeMaterial : _inactiveMaterial;
            if (_indicator != null && material != null)
            {
                // sharedMaterial swaps which asset the renderer uses without creating a material copy.
                _indicator.sharedMaterial = material;
            }
        }

        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
            {
                return;
            }

            Gizmos.color = new Color(0.25f, 0.65f, 0.84f, 0.6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);

            // Arrow showing where and which way the player respawns.
            Gizmos.matrix = Matrix4x4.identity;
            Vector3 spawn = SpawnPosition;
            Gizmos.DrawLine(spawn, spawn + SpawnRotation * Vector3.forward * 2f);
        }
    }
}
