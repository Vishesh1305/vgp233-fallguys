using System;
using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Tracks the furthest checkpoint the player has reached. Progress only moves forward:
    /// entering a checkpoint with an index at or below the current one does nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CheckpointTracker : MonoBehaviour
    {
        [Header("Checkpoints")]
        [Tooltip("Every checkpoint in the course. List order does not matter; each checkpoint's Index decides progress.")]
        [SerializeField] private Checkpoint[] _checkpoints = Array.Empty<Checkpoint>();

        /// <summary>Raised when the player reaches a checkpoint further than any before it.</summary>
        public static event Action<Checkpoint> CheckpointReached;

        /// <summary>Raised at start and on every new checkpoint with (current index, total checkpoints).</summary>
        public static event Action<int, int> ProgressChanged;

        /// <summary>Index of the furthest checkpoint reached; 0 means only the Start Point.</summary>
        public int CurrentIndex { get; private set; }

        /// <summary>Number of checkpoints in the course.</summary>
        public int TotalCheckpoints => _checkpoints.Length;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            CheckpointReached = null;
            ProgressChanged = null;
        }

        private void OnEnable()
        {
            foreach (Checkpoint checkpoint in _checkpoints)
            {
                if (checkpoint != null)
                {
                    checkpoint.PlayerEntered += HandlePlayerEntered;
                }
            }
        }

        private void OnDisable()
        {
            foreach (Checkpoint checkpoint in _checkpoints)
            {
                if (checkpoint != null)
                {
                    checkpoint.PlayerEntered -= HandlePlayerEntered;
                }
            }
        }

        private void Start()
        {
            ProgressChanged?.Invoke(CurrentIndex, TotalCheckpoints);
        }

        private void HandlePlayerEntered(Checkpoint checkpoint)
        {
            // Forward only: walking back onto an earlier pad never moves the respawn point backwards.
            if (checkpoint.Index <= CurrentIndex)
            {
                return;
            }

            CurrentIndex = checkpoint.Index;
            checkpoint.Activate();
            CheckpointReached?.Invoke(checkpoint);
            ProgressChanged?.Invoke(CurrentIndex, TotalCheckpoints);
        }
    }
}
