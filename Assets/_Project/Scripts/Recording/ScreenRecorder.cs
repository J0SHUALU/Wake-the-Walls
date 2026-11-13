using System;
using UnityEngine;

namespace WakeTheWalls.Recording
{
    public abstract class ScreenRecorder : MonoBehaviour
    {
        public const string PhotosDenied = "photos";

        public event Action Started;

        public event Action<bool> Stopped;

        public event Action<string> Failed;

        public abstract bool IsAvailable { get; }

        public abstract bool IsRecording { get; }

        public abstract bool IsBusy { get; }

        public virtual bool SavesToPhotos => false;

        public abstract void Begin();

        public abstract void End();

        public abstract void Show();

        protected void RaiseStarted() => Started?.Invoke();

        protected void RaiseStopped(bool hasVideo) => Stopped?.Invoke(hasVideo);

        protected void RaiseFailed(string reason) => Failed?.Invoke(reason);
    }
}
