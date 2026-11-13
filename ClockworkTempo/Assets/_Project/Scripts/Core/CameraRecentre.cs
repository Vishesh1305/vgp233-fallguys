using Unity.Cinemachine;
using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// After a respawn, swings the orbit camera behind the player's new facing and cancels camera damping,
    /// so the view cuts to the new spot instead of sliding across the level.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow))]
    [DisallowMultipleComponent]
    public sealed class CameraRecentre : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform that carries the player's facing (Player/Body). Its yaw decides where the camera ends up.")]
        [SerializeField] private Transform _facingSource;

        [Header("Behaviour")]
        [Tooltip("Also reset the camera pitch to the orbit's centre value.")]
        [SerializeField] private bool _resetPitch = true;

        private CinemachineCamera _camera;
        private CinemachineOrbitalFollow _orbit;

        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
            _orbit = GetComponent<CinemachineOrbitalFollow>();

            if (_facingSource == null)
            {
                Debug.LogError($"{nameof(CameraRecentre)} on {name} has no Facing Source assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            PlayerRespawner.Respawned += HandleRespawned;
        }

        private void OnDisable()
        {
            PlayerRespawner.Respawned -= HandleRespawned;
        }

        private void HandleRespawned()
        {
            // World Space binding: the horizontal axis is a world yaw, and 0 puts the camera behind +Z.
            // DeltaAngle maps any yaw into -180 to 180, the range the axis wraps in.
            _orbit.HorizontalAxis.Value = _orbit.HorizontalAxis.ClampValue(Mathf.DeltaAngle(0f, _facingSource.eulerAngles.y));

            if (_resetPitch)
            {
                _orbit.VerticalAxis.Value = _orbit.VerticalAxis.Center;
            }

            // Makes the next camera update ignore last frame's state, which cancels the position damping.
            _camera.PreviousStateIsValid = false;
        }
    }
}
