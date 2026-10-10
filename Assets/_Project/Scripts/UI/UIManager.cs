using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// Shows the screen that matches the current app state and hides the others.
    /// It only listens to <see cref="GameEvents.StateChanged"/>, so it never talks to tracking directly.
    /// Panels inside a state (info, help, paused) are handled by their own screens.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Tooltip("Shown in the Start state.")]
        [SerializeField] UIScreen startScreen;

        [Tooltip("Shown in the Scanning state.")]
        [SerializeField] UIScreen scanScreen;

        [Tooltip("Shown in the Experience state (the in-experience HUD).")]
        [SerializeField] UIScreen experienceScreen;

        [Tooltip("Shown in the Completion state.")]
        [SerializeField] UIScreen completionScreen;

        void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
        }

        void HandleStateChanged(AppState state)
        {
            UIScreen target = ScreenFor(state);

            // Hide first so two screens never take taps at the same time.
            foreach (UIScreen screen in AllScreens())
            {
                if (screen != null && screen != target) screen.Hide();
            }

            if (target != null) target.Show();
        }

        UIScreen ScreenFor(AppState state)
        {
            switch (state)
            {
                case AppState.Start: return startScreen;
                case AppState.Scanning: return scanScreen;
                case AppState.Experience: return experienceScreen;
                case AppState.Completion: return completionScreen;
                default: return null;
            }
        }

        UIScreen[] AllScreens() => new[] { startScreen, scanScreen, experienceScreen, completionScreen };
    }
}
