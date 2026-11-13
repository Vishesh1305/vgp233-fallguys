using UnityEngine;

namespace ClockworkTempo
{
    /// <summary>
    /// Forces the standalone player into a fixed 1920x1080 window before the first scene loads.
    /// Safety net for Player Settings, which a standalone build can override with a window size
    /// cached in the registry from a previous run.
    /// </summary>
    public static class DisplayBootstrap
    {
        private const int TargetWidth = 1920;
        private const int TargetHeight = 1080;

        /// <summary>
        /// Called by Unity automatically before the first scene loads; needs no GameObject in the scene.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyResolution()
        {
            // The Game view controls resolution in the Editor, so this only matters in builds.
            if (Application.isEditor)
            {
                return;
            }

            Screen.SetResolution(TargetWidth, TargetHeight, FullScreenMode.Windowed);
        }
    }
}
