using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Core;

namespace WakeTheWalls.Recording
{
    public class RecordButton : MonoBehaviour
    {
        [SerializeField] ScreenRecorder recorder;

        [Header("Button")]
        [Tooltip("Parent of the button, timer and message. Hidden when this device cannot record.")]
        [SerializeField] GameObject controls;
        [SerializeField] Button button;
        [SerializeField] Image background;
        [SerializeField] Image icon;
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite recordingSprite;
        [SerializeField] Color idleBackground = new Color(0f, 0.1333f, 0.3098f, 1f);
        [SerializeField] Color recordingBackground = new Color(0.8157f, 0.051f, 0.1765f, 1f);
        [SerializeField] Color idleIcon = new Color(0.8157f, 0.051f, 0.1765f, 1f);
        [SerializeField] Color recordingIcon = Color.white;

        [Header("Timer")]
        [Tooltip("Shows a running timer while recording. Off by default because the timer ends up in the video.")]
        [SerializeField] bool showTimer;
        [SerializeField] CanvasGroup timerGroup;
        [SerializeField] TMP_Text timerLabel;
        [Tooltip("{0} is minutes and {1} is seconds.")]
        [SerializeField] string timerFormat = "REC {0:00}:{1:00}";

        [Header("Message")]
        [SerializeField] CanvasGroup messageGroup;
        [SerializeField] TMP_Text messageLabel;
        [SerializeField] string declinedMessage = "Recording did not start. Allow screen recording to save a video.";
        [SerializeField] string missingMessage = "No video was saved.";
        [SerializeField] string savedMessage = "Video saved to Photos.";
        [SerializeField] string photosMessage = "Allow Wake the Walls to add to Photos in Settings to save your videos.";
        [SerializeField] float messageSeconds = 4f;

        [Tooltip("Seconds a fade takes. Keep it between 0.6 and 1.5.")]
        [SerializeField] float fadeDuration = 0.6f;

        Coroutine timer;
        Coroutine timerFade;
        Coroutine message;

        void Awake()
        {
            if (button != null) button.onClick.AddListener(HandleClick);
            SetLook(false);
            if (timerGroup != null) timerGroup.alpha = 0f;
            if (messageGroup != null) messageGroup.alpha = 0f;
        }

        void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
            if (recorder != null)
            {
                recorder.Started += HandleStarted;
                recorder.Stopped += HandleStopped;
                recorder.Failed += HandleFailed;
            }
            Refresh();
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
            if (recorder != null)
            {
                recorder.Started -= HandleStarted;
                recorder.Stopped -= HandleStopped;
                recorder.Failed -= HandleFailed;
            }
        }

        void Refresh()
        {
            if (controls != null) controls.SetActive(recorder != null && recorder.IsAvailable);
        }

        void HandleStateChanged(AppState state)
        {
            Refresh();
            if (state != AppState.Experience && recorder != null && recorder.IsRecording) recorder.End();
        }

        void HandleClick()
        {
            if (recorder == null || recorder.IsBusy) return;
            button.interactable = false;
            if (recorder.IsRecording) recorder.End();
            else recorder.Begin();
            if (!recorder.IsBusy && !recorder.IsRecording) button.interactable = true;
        }

        void HandleStarted()
        {
            button.interactable = true;
            SetLook(true);
            ShowTimer(showTimer);
        }

        void HandleStopped(bool hasVideo)
        {
            button.interactable = true;
            SetLook(false);
            ShowTimer(false);
            if (!hasVideo) ShowMessage(missingMessage);
            else if (recorder.SavesToPhotos) ShowMessage(savedMessage);
            else recorder.Show();
        }

        void HandleFailed(string reason)
        {
            button.interactable = true;
            SetLook(false);
            ShowTimer(false);
            ShowMessage(reason == ScreenRecorder.PhotosDenied ? photosMessage : declinedMessage);
        }

        void SetLook(bool recording)
        {
            if (background != null) background.color = recording ? recordingBackground : idleBackground;
            if (icon == null) return;
            icon.sprite = recording ? recordingSprite : idleSprite;
            icon.color = recording ? recordingIcon : idleIcon;
        }

        void ShowTimer(bool on)
        {
            if (timerGroup == null) return;
            if (timer != null) StopCoroutine(timer);
            if (timerFade != null) StopCoroutine(timerFade);
            timer = on ? StartCoroutine(RunTimer()) : null;
            timerFade = StartCoroutine(Tween.Value(timerGroup.alpha, on ? 1f : 0f, a => timerGroup.alpha = a, fadeDuration, Tween.EaseInOutCubic));
        }

        IEnumerator RunTimer()
        {
            float began = Time.unscaledTime;
            int shown = -1;
            while (true)
            {
                int seconds = Mathf.FloorToInt(Time.unscaledTime - began);
                if (seconds != shown && timerLabel != null)
                {
                    shown = seconds;
                    timerLabel.text = string.Format(timerFormat, seconds / 60, seconds % 60);
                }
                yield return null;
            }
        }

        void ShowMessage(string text)
        {
            if (messageGroup == null || messageLabel == null) return;
            if (message != null) StopCoroutine(message);
            messageLabel.text = text;
            message = StartCoroutine(RunMessage());
        }

        IEnumerator RunMessage()
        {
            yield return Tween.Value(messageGroup.alpha, 1f, a => messageGroup.alpha = a, fadeDuration, Tween.EaseInOutCubic);
            yield return new WaitForSecondsRealtime(messageSeconds);
            yield return Tween.Value(1f, 0f, a => messageGroup.alpha = a, fadeDuration, Tween.EaseInOutCubic);
            message = null;
        }
    }
}
