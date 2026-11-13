using System;
using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Trigger volume under the course. Announces when the player falls into it;
    /// PlayerRespawner listens and moves the player back to the latest checkpoint.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent]
    public sealed class KillZone : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [Header("Debug")]
        [Tooltip("Colour of the volume outline drawn in the Scene view.")]
        [SerializeField] private Color _gizmoColour = new Color(0.75f, 0.22f, 0.17f, 0.6f);

        /// <summary>Raised when the player's collider enters any kill zone.</summary>
        public static event Action PlayerEntered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            PlayerEntered = null;
        }

        private void Awake()
        {
            // A solid kill zone would catch the player instead of detecting the fall, so force trigger mode.
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PlayerTag))
            {
                PlayerEntered?.Invoke();
            }
        }

        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
            {
                return;
            }

            Gizmos.color = _gizmoColour;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
