namespace ClockworkTempo
{
    /// <summary>Top level game flow states owned by GameManager.</summary>
    public enum GameState
    {
        /// <summary>Main menu is shown; the race has not started.</summary>
        MainMenu,

        /// <summary>3, 2, 1, GO before the race. Camera can look, player cannot move.</summary>
        Countdown,

        /// <summary>The race is running and the timer counts.</summary>
        Playing,

        /// <summary>Pause menu is open; Time.timeScale is 0.</summary>
        Paused,

        /// <summary>The player crossed the finish line; the timer is stopped.</summary>
        Finished
    }
}
