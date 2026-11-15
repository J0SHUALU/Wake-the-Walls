using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
    public abstract class MuralExperience : MonoBehaviour
    {
        public MuralData Data { get; private set; }

        public bool IsComplete { get; private set; }

        public void Initialize(MuralData data)
        {
            Data = data;
            OnInitialized();
        }

        protected virtual void OnInitialized() { }

        public abstract void OnFound();

        public virtual void OnLost() { }

        public virtual void OnResume() { }

        public virtual void ResetExperience()
        {
            IsComplete = false;
        }

        protected void Complete()
        {
            if (IsComplete) return;
            IsComplete = true;
            GameEvents.RaiseMuralCompleted(Data);
        }
    }
}
