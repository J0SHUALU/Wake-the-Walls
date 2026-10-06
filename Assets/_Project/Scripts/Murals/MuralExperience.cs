using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Base class for every mural's AR experience. The tracking manager spawns the prefab
    /// and calls these hooks; each mural overrides them with its own story.
    /// </summary>
    /// <remarks>
    /// Local space of the prefab: origin at the centre of the mural, +X to the right,
    /// +Y up the wall, and -Z out of the wall towards the viewer. One unit is one metre,
    /// so build content at the mural's real size.
    /// </remarks>
    public abstract class MuralExperience : MonoBehaviour
    {
        /// <summary>The mural this experience belongs to.</summary>
        public MuralData Data { get; private set; }

        /// <summary>True once the user has finished this mural.</summary>
        public bool IsComplete { get; private set; }

        /// <summary>Called once by the tracking manager right after the prefab is spawned.</summary>
        public void Initialize(MuralData data)
        {
            Data = data;
            OnInitialized();
        }

        /// <summary>Optional setup after <see cref="Data"/> is assigned.</summary>
        protected virtual void OnInitialized() { }

        /// <summary>The mural was detected for the first time. Start the intro here.</summary>
        public abstract void OnFound();

        /// <summary>Tracking was lost. Pause and fade out, but keep progress.</summary>
        public virtual void OnLost() { }

        /// <summary>The mural was found again after being lost. Continue from where it paused.</summary>
        public virtual void OnResume() { }

        /// <summary>Put everything back to the state it was in before <see cref="OnFound"/>.</summary>
        public virtual void ResetExperience()
        {
            IsComplete = false;
        }

        /// <summary>Call when the user has finished the mural. Only raises the event once.</summary>
        protected void Complete()
        {
            if (IsComplete) return;
            IsComplete = true;
            GameEvents.RaiseMuralCompleted(Data);
        }
    }
}
