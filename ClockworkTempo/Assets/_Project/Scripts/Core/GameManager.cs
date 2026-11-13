using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClockworkTempo
{
    /// <summary>
    /// Owns the game flow state machine (MainMenu, Countdown, Playing, Paused, Finished) and
    /// announces every change through static events. Other systems listen instead of being called directly.
    /// </summary>
    /// <remarks>
    /// Runs after other scripts (execution order 100) so the initial state is raised in Start,
    /// after every listener has subscribed in OnEnable and run its own Start.
    /// </remarks>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [Header("Flow")]
        [Tooltip("State the scene starts in. Countdown until the main menu exists in Phase 4.4.")]
        [SerializeField] private GameState _initialState = GameState.Countdown;

        [Tooltip("Length of the countdown in whole seconds (3 gives 3, 2, 1, GO).")]
        [SerializeField, Min(1)] private int _countdownSeconds = 3;

        [Header("References")]
        [Tooltip("Player input; its pause button toggles Paused.")]
        [SerializeField] private PlayerInputReader _playerInput;

        [Header("Debug")]
        [Tooltip("Log every state change to the Console.")]
        [SerializeField] private bool _logStateChanges;

        private float _countdownRemaining;
        private int _lastCountdownValue;

        /// <summary>The active GameManager in the loaded scene.</summary>
        public static GameManager Instance { get; private set; }

        /// <summary>Raised after every state change with (previous, current).</summary>
        public static event Action<GameState, GameState> StateChanged;

        /// <summary>Raised when the countdown shows a new number: 3, 2, 1, then 0 for GO.</summary>
        public static event Action<int> CountdownTicked;

        /// <summary>Current game state.</summary>
        public GameState State { get; private set; }

        // Statics survive Play mode when domain reload is disabled, so clear them before each session.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            StateChanged = null;
            CountdownTicked = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (_playerInput != null)
            {
                _playerInput.PausePressed += TogglePause;
            }
        }

        private void OnDisable()
        {
            if (_playerInput != null)
            {
                _playerInput.PausePressed -= TogglePause;
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;

            // A scene reload while paused must not leave the next scene frozen.
            Time.timeScale = 1f;
        }

        private void Start()
        {
            EnterState(_initialState);
        }

        private void Update()
        {
            if (State == GameState.Countdown)
            {
                TickCountdown();
            }
        }

        /// <summary>Starts the countdown from the main menu.</summary>
        public void StartRace()
        {
            if (State == GameState.MainMenu)
            {
                EnterState(GameState.Countdown);
            }
        }

        /// <summary>Pauses a running race or resumes a paused one. Ignored in other states.</summary>
        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                EnterState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                EnterState(GameState.Playing);
            }
        }

        /// <summary>Ends the race. Called by the finish line; ignored unless the race is running.</summary>
        public void FinishRace()
        {
            if (State == GameState.Playing)
            {
                EnterState(GameState.Finished);
            }
        }

        /// <summary>Reloads the course scene from the start.</summary>
        public void RestartRace()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void TickCountdown()
        {
            _countdownRemaining -= Time.deltaTime;

            if (_countdownRemaining <= 0f)
            {
                CountdownTicked?.Invoke(0);
                EnterState(GameState.Playing);
                return;
            }

            // Only raise the event when the displayed whole number changes, not every frame.
            int value = Mathf.CeilToInt(_countdownRemaining);
            if (value != _lastCountdownValue)
            {
                _lastCountdownValue = value;
                CountdownTicked?.Invoke(value);
            }
        }

        private void EnterState(GameState next)
        {
            GameState previous = State;
            State = next;

            // Only Paused freezes time; every other state runs at normal speed.
            Time.timeScale = next == GameState.Paused ? 0f : 1f;

            if (_logStateChanges)
            {
                Debug.Log($"{nameof(GameManager)}: {previous} -> {next}", this);
            }

            StateChanged?.Invoke(previous, next);

            if (next == GameState.Countdown)
            {
                _countdownRemaining = _countdownSeconds;
                _lastCountdownValue = _countdownSeconds;
                CountdownTicked?.Invoke(_countdownSeconds);
            }
        }
    }
}
