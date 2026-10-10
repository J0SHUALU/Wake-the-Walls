using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// The scanning screen: a back button, the stamps count, the gold scan frame with a softly
    /// pulsing centre ring, and a bottom card with the instructions. If no mural is found within
    /// a few seconds an extra hint fades in. Space for the hint is reserved so nothing jumps.
    /// </summary>
    public class ScanScreen : UIScreen
    {
        [SerializeField] MuralLibrary library;

        [Tooltip("The scene's state machine. Found automatically if left empty.")]
        [SerializeField] AppStateMachine stateMachine;

        [Header("Content")]
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text countLabel;
        [Tooltip("{0} is murals woken, {1} is murals in the library.")]
        [SerializeField] string countFormat = "{0} OF {1} AWAKE";

        [Header("Hint")]
        [SerializeField] CanvasGroup hint;
        [SerializeField] float hintDelay = 8f;

        [Header("Reticle")]
        [SerializeField] RectTransform reticle;
        [SerializeField] float pulseAmount = 0.12f;
        [SerializeField] float pulseSpeed = 2f;

        Coroutine hintRoutine;

        protected override void Awake()
        {
            base.Awake();
            if (backButton != null) backButton.onClick.AddListener(GoBack);
            if (hint != null) hint.alpha = 0f;
        }

        /// <summary>Returns to the start screen.</summary>
        public void GoBack()
        {
            if (stateMachine == null) stateMachine = FindAnyObjectByType<AppStateMachine>();
            if (stateMachine != null) stateMachine.GoTo(AppState.Start);
        }

        protected override void OnShowing()
        {
            int total = library != null ? library.Murals.Count : 0;
            if (countLabel != null) countLabel.text = string.Format(countFormat, SessionProgress.Count, total);

            if (hint != null) hint.alpha = 0f;
            StopHint();
            hintRoutine = StartCoroutine(ShowHintLater());
        }

        protected override void OnHiding() => StopHint();

        void Update()
        {
            if (!IsVisible || reticle == null) return;
            float scale = 1f + pulseAmount * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI));
            reticle.localScale = new Vector3(scale, scale, 1f);
        }

        IEnumerator ShowHintLater()
        {
            yield return new WaitForSeconds(hintDelay);
            if (hint != null) yield return Tween.Value(hint.alpha, 1f, a => hint.alpha = a, 0.8f, Tween.EaseInOutCubic);
            hintRoutine = null;
        }

        void StopHint()
        {
            if (hintRoutine != null) StopCoroutine(hintRoutine);
            hintRoutine = null;
        }
    }
}
