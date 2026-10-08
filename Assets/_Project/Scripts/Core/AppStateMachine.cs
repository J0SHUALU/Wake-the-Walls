using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Core
{
    /// <summary>
    /// Holds the current top level state of the app and announces every change through
    /// <see cref="GameEvents.StateChanged"/>. Moves to Experience when a mural is found
    /// and to Completion when a mural is finished. Starts in Start.
    /// </summary>
    public class AppStateMachine : MonoBehaviour
    {
        /// <summary>The state the app is in right now.</summary>
        public AppState Current { get; private set; } = AppState.Start;

        void OnEnable()
        {
            GameEvents.MuralFound += HandleMuralFound;
            GameEvents.MuralCompleted += HandleMuralCompleted;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleMuralFound;
            GameEvents.MuralCompleted -= HandleMuralCompleted;
        }

        void Start()
        {
            // Announce the starting state so screens can set themselves up.
            GameEvents.RaiseStateChanged(Current);
        }

        /// <summary>Moves to <paramref name="next"/> and raises StateChanged. Does nothing if already there.</summary>
        public void GoTo(AppState next)
        {
            if (next == Current) return;

            Current = next;
            GameEvents.RaiseStateChanged(Current);
        }

        void HandleMuralFound(MuralData mural) => GoTo(AppState.Experience);

        void HandleMuralCompleted(MuralData mural) => GoTo(AppState.Completion);
    }
}
