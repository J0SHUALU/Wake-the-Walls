using System;
using System.Collections;
using UnityEngine;

namespace WakeTheWalls.Recording
{
    public class ReplayKitRecorder : ScreenRecorder
    {
        [Tooltip("Seconds of running time to wait for recording to begin before treating the request as declined.")]
        [SerializeField] float startTimeout = 8f;

        [Tooltip("Seconds of running time to wait for the video to be written and saved to Photos.")]
        [SerializeField] float saveTimeout = 30f;

        const float MaxFrameStep = 0.1f;

        bool active;
        float startWaited;
        Coroutine starting;
        Coroutine finishing;

        public override bool IsAvailable => ReplayKitBridge.Available;

        public override bool IsRecording => ReplayKitBridge.IsRecording;

        public override bool IsBusy => starting != null || finishing != null;

        public override bool SavesToPhotos => true;

        public override void Begin()
        {
            if (!IsAvailable || IsBusy || IsRecording) return;
            try
            {
                ReplayKitBridge.Reset();
                ReplayKitBridge.Start();
            }
            catch (Exception)
            {
                RaiseFailed(string.Empty);
                return;
            }
            startWaited = 0f;
            starting = StartCoroutine(WaitForStart());
        }

        public override void End()
        {
            if (!active || finishing != null) return;
            try
            {
                ReplayKitBridge.Stop();
            }
            catch (Exception)
            {
                active = false;
                RaiseStopped(false);
                return;
            }
            finishing = StartCoroutine(WaitForSave());
        }

        public override void Show()
        {
        }

        void Update()
        {
            if (active || IsBusy || ReplayKitBridge.State != RecorderState.Recording) return;
            active = true;
            RaiseStarted();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) return;
            startWaited = 0f;
            if (active && finishing == null && !IsRecording) End();
        }

        IEnumerator WaitForStart()
        {
            while (ReplayKitBridge.State == RecorderState.Starting && startWaited < startTimeout)
            {
                startWaited += Mathf.Min(Time.unscaledDeltaTime, MaxFrameStep);
                yield return null;
            }

            starting = null;
            if (ReplayKitBridge.State == RecorderState.Recording)
            {
                active = true;
                RaiseStarted();
            }
            else
            {
                RaiseFailed(string.Empty);
            }
        }

        IEnumerator WaitForSave()
        {
            float waited = 0f;
            while (ReplayKitBridge.State == RecorderState.Saving && waited < saveTimeout)
            {
                waited += Mathf.Min(Time.unscaledDeltaTime, MaxFrameStep);
                yield return null;
            }

            RecorderState result = ReplayKitBridge.State;
            active = false;
            finishing = null;
            if (result == RecorderState.PhotosDenied) RaiseFailed(PhotosDenied);
            else RaiseStopped(result == RecorderState.Saved);
        }
    }
}
