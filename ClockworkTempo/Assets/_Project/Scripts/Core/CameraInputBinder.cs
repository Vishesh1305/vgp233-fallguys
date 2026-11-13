using Unity.Cinemachine;
using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Turns camera look on during Countdown and Playing and off in menus, pause and the finish screen.
    /// Needed because the Cinemachine axis controller reads Look straight from the actions asset.
    /// </summary>
    [RequireComponent(typeof(CinemachineInputAxisController))]
    [DisallowMultipleComponent]
    public sealed class CameraInputBinder : MonoBehaviour
    {
        private CinemachineInputAxisController _axisController;

        private void Awake()
        {
            _axisController = GetComponent<CinemachineInputAxisController>();
        }

        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            _axisController.enabled = current == GameState.Countdown || current == GameState.Playing;
        }
    }
}
