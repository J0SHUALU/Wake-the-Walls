using System;
using System.Collections;
using UnityEngine;

namespace WakeTheWalls.Recording
{
    public class ReplayKitRecorder : ScreenRecorder
    {
        [Tooltip("Seconds of running time to wait for recording to begin before treating the request as declined.")]
        [SerializeField] float startTimeout = 8f;

        [Tooltip("Seconds of running time to wait for the finished video after stopping.")]
        [SerializeField] float stopTimeout = 6f;

        const float MaxFrameStep = 0.1f;

        bool active;
        float startWaited;
        Coroutine starting;
        Coroutine finishing;

        public override bool IsAvailable => ReplayKitBridge.Available;

        public override bool IsRecording => ReplayKitBridge.IsRecording;

        public override bool IsBusy => starting != null || finishing != null;

        public override void Begin()
        {
            if (!IsAvailable || IsBusy || IsRecording) return;
            try
            {
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
                if (IsRecording) ReplayKitBridge.Stop();
            }
            catch (Exception)
            {
            }
            finishing = StartCoroutine(WaitForVideo());
        }

        public override void Show()
        {
            try
            {
                if (ReplayKitBridge.HasRecording) ReplayKitBridge.Preview();
            }
            catch (Exception)
            {
                RaiseStopped(false);
            }
        }

        void Update()
        {
            if (active || IsBusy || !IsRecording) return;
            active = true;
            RaiseStarted();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) return;
            startWaited = 0f;
            if (active && finishing == null && !IsRecording) finishing = StartCoroutine(WaitForVideo());
        }

        IEnumerator WaitForStart()
        {
            while (!IsRecording && startWaited < startTimeout)
            {
                startWaited += Mathf.Min(Time.unscaledDeltaTime, MaxFrameStep);
                yield return null;
            }

            starting = null;
            if (IsRecording)
            {
                active = true;
                RaiseStarted();
            }
            else
            {
                RaiseFailed(ReplayKitBridge.LastError);
            }
        }

        IEnumerator WaitForVideo()
        {
            float waited = 0f;
            while ((IsRecording || !ReplayKitBridge.HasRecording) && waited < stopTimeout)
            {
                waited += Mathf.Min(Time.unscaledDeltaTime, MaxFrameStep);
                yield return null;
            }

            bool hasVideo = !IsRecording && ReplayKitBridge.HasRecording;
            active = false;
            finishing = null;
            RaiseStopped(hasVideo);
        }
    }
}
