using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Recording
{
    public class RecordingFocus : MonoBehaviour
    {
        [SerializeField] ScreenRecorder recorder;

        [Tooltip("Controls that fade out while a recording is running.")]
        [SerializeField] CanvasGroup[] hiddenWhileRecording;

        [Tooltip("Seconds a fade takes. Keep it between 0.6 and 1.5.")]
        [SerializeField] float fadeDuration = 0.6f;

        [Tooltip("Name of a control created at runtime that fades out with the others.")]
        [SerializeField] string runtimeControl = "NarratorButton";

        readonly List<CanvasGroup> targets = new List<CanvasGroup>();
        Coroutine fade;

        void OnEnable()
        {
            if (recorder == null) return;
            recorder.Started += HandleStarted;
            recorder.Stopped += HandleStopped;
            recorder.Failed += HandleFailed;
        }

        void OnDisable()
        {
            if (recorder != null)
            {
                recorder.Started -= HandleStarted;
                recorder.Stopped -= HandleStopped;
                recorder.Failed -= HandleFailed;
            }
            fade = null;
            Apply(1f);
        }

        void HandleStarted() => FadeTo(0f);

        void HandleStopped(bool hasVideo) => FadeTo(1f);

        void HandleFailed(string reason) => FadeTo(1f);

        void FadeTo(float target)
        {
            if (fade != null) StopCoroutine(fade);
            CollectTargets();
            float from = targets.Count > 0 && targets[0] != null ? targets[0].alpha : 1f;
            SetInteractive(target > 0f);
            fade = StartCoroutine(Fade(from, target));
        }

        IEnumerator Fade(float from, float target)
        {
            yield return Tween.Value(from, target, Apply, fadeDuration, Tween.EaseInOutCubic);
            fade = null;
        }

        void CollectTargets()
        {
            targets.Clear();
            targets.AddRange(hiddenWhileRecording);
            Transform extra = transform.parent != null ? transform.parent.Find(runtimeControl) : null;
            if (extra == null) return;
            CanvasGroup group = extra.GetComponent<CanvasGroup>();
            if (group == null) group = extra.gameObject.AddComponent<CanvasGroup>();
            targets.Add(group);
        }

        void Apply(float alpha)
        {
            foreach (CanvasGroup group in targets)
            {
                if (group != null) group.alpha = alpha;
            }
        }

        void SetInteractive(bool on)
        {
            foreach (CanvasGroup group in targets)
            {
                if (group == null) continue;
                group.interactable = on;
                group.blocksRaycasts = on;
            }
        }
    }
}
