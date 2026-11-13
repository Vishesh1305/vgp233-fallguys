using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Full screen black overlay that hides the respawn teleport. Fades to black when PlayerRespawner
    /// announces a fall and back out once the player has been moved.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public sealed class ScreenFader : MonoBehaviour
    {
        [Header("Timing")]
        [Tooltip("Seconds to fade back from black after the player has been moved.")]
        [SerializeField, Min(0f)] private float _fadeInDuration = 0.25f;

        [Header("Debug")]
        [Tooltip("Log every fade request to the Console.")]
        [SerializeField] private bool _logFades = true;

        private CanvasGroup _group;
        private float _targetAlpha;
        private float _fadeSpeed;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0f;

            // The overlay is purely visual; it must never swallow clicks meant for menus.
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            PlayerRespawner.RespawnStarted += HandleRespawnStarted;
            PlayerRespawner.Respawned += HandleRespawned;
        }

        private void OnDisable()
        {
            PlayerRespawner.RespawnStarted -= HandleRespawnStarted;
            PlayerRespawner.Respawned -= HandleRespawned;
        }

        private void Update()
        {
            if (Mathf.Approximately(_group.alpha, _targetAlpha))
            {
                return;
            }

            _group.alpha = Mathf.MoveTowards(_group.alpha, _targetAlpha, _fadeSpeed * Time.unscaledDeltaTime);
        }

        private void HandleRespawnStarted(float fadeOutSeconds)
        {
            FadeTo(1f, fadeOutSeconds);
        }

        private void HandleRespawned()
        {
            FadeTo(0f, _fadeInDuration);
        }

        private void FadeTo(float alpha, float duration)
        {
            if (_logFades)
            {
                Debug.Log($"{nameof(ScreenFader)}: fade to {alpha} over {duration}s (alpha now {_group.alpha})", this);
            }

            _targetAlpha = alpha;

            if (duration <= 0f)
            {
                _group.alpha = alpha;
                return;
            }

            // Speed is derived from the current alpha, so an interrupted fade still takes about `duration`.
            _fadeSpeed = Mathf.Abs(alpha - _group.alpha) / duration;
        }
    }
}
