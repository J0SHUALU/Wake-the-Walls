using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Core
{
    public class AppStateMachine : MonoBehaviour
    {
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
            GameEvents.RaiseStateChanged(Current);
        }

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
