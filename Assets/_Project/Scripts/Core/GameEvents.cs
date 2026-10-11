using System;
using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Core
{
    public static class GameEvents
    {
        public static event Action<AppState> StateChanged;

        public static event Action<MuralData> MuralFound;

        public static event Action<MuralData> MuralLost;

        public static event Action<MuralData> MuralCompleted;

        public static event Action ResetRequested;

        public static event Action<string> InfoRequested;

        public static event Action InteractableTapped;

        public static void RaiseStateChanged(AppState state) => StateChanged?.Invoke(state);

        public static void RaiseMuralFound(MuralData mural) => MuralFound?.Invoke(mural);

        public static void RaiseMuralLost(MuralData mural) => MuralLost?.Invoke(mural);

        public static void RaiseMuralCompleted(MuralData mural) => MuralCompleted?.Invoke(mural);

        public static void RaiseResetRequested() => ResetRequested?.Invoke();

        public static void RaiseInfoRequested(string topic) => InfoRequested?.Invoke(topic);

        public static void RaiseInteractableTapped() => InteractableTapped?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearSubscribers()
        {
            StateChanged = null;
            MuralFound = null;
            MuralLost = null;
            MuralCompleted = null;
            ResetRequested = null;
            InfoRequested = null;
            InteractableTapped = null;
        }
    }
}
