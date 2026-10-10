using System;
using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Core
{
    /// <summary>
    /// App wide events. Systems talk through these instead of holding references to each other,
    /// so tracking, UI, audio and the mural experiences stay independent.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Raised after the app enters a new state.</summary>
        public static event Action<AppState> StateChanged;

        /// <summary>Raised when a mural is detected, including when it is found again after being lost.</summary>
        public static event Action<MuralData> MuralFound;

        /// <summary>Raised when tracking of a mural has been lost for longer than the grace period.</summary>
        public static event Action<MuralData> MuralLost;

        /// <summary>Raised by a mural experience when the user has finished it.</summary>
        public static event Action<MuralData> MuralCompleted;

        /// <summary>Raised by the UI when the user asks to restart the current mural.</summary>
        public static event Action ResetRequested;

        /// <summary>Raised whenever the user taps something tappable in AR, for example to play a tap sound.</summary>
        public static event Action InteractableTapped;

        /// <summary>Announces a new app state.</summary>
        public static void RaiseStateChanged(AppState state) => StateChanged?.Invoke(state);

        /// <summary>Announces that a mural is being tracked.</summary>
        public static void RaiseMuralFound(MuralData mural) => MuralFound?.Invoke(mural);

        /// <summary>Announces that a mural is no longer tracked.</summary>
        public static void RaiseMuralLost(MuralData mural) => MuralLost?.Invoke(mural);

        /// <summary>Announces that a mural experience is finished.</summary>
        public static void RaiseMuralCompleted(MuralData mural) => MuralCompleted?.Invoke(mural);

        /// <summary>Asks the active mural to restart.</summary>
        public static void RaiseResetRequested() => ResetRequested?.Invoke();

        /// <summary>Announces that something in AR was tapped.</summary>
        public static void RaiseInteractableTapped() => InteractableTapped?.Invoke();

        // Static events survive between play sessions when domain reload is off, so clear them on start.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearSubscribers()
        {
            StateChanged = null;
            MuralFound = null;
            MuralLost = null;
            MuralCompleted = null;
            ResetRequested = null;
            InteractableTapped = null;
        }
    }
}
