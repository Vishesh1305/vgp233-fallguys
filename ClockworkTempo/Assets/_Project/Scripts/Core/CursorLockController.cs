using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Locks and hides the mouse cursor during gameplay and restores it when the window regains focus.
    /// Other systems change the state through SetLocked.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CursorLockController : MonoBehaviour
    {
        [Header("Startup")]
        [Tooltip("Lock and hide the cursor when the scene starts.")]
        [SerializeField] private bool _lockOnStart = true;

        // The state we want, kept separately because the OS or Editor can release the lock on its own.
        private bool _wantsLock;

        /// <summary>True when the cursor is meant to be locked and hidden.</summary>
        public bool WantsLock => _wantsLock;

        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;
        }

        private void Start()
        {
            SetLocked(_lockOnStart);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Alt+Tab or clicking away releases the lock; put it back when focus returns.
            if (hasFocus && _wantsLock)
            {
                ApplyCursorState(true);
            }
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;
            ApplyCursorState(false);
        }

        /// <summary>Locks and hides the cursor, or frees and shows it.</summary>
        /// <param name="locked">True for gameplay, false for menus.</param>
        public void SetLocked(bool locked)
        {
            _wantsLock = locked;
            ApplyCursorState(locked);
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            // Locked while the player controls the camera; free whenever a menu or screen needs the mouse.
            SetLocked(current == GameState.Countdown || current == GameState.Playing);
        }

        private static void ApplyCursorState(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
