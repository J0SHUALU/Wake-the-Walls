using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Core;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// A short how-to opened from the HUD Help button or the start screen's How it works link:
    /// scan the whole wall, tap the parts that move, and what the four buttons do.
    /// The sheet fades and slides up over a soft dark backdrop; tapping the backdrop or Got it closes it.
    /// </summary>
    public class HelpOverlay : UIScreen
    {
        [SerializeField] RectTransform sheet;
        [SerializeField] float slideDistance = 120f;
        [SerializeField] Button closeButton;
        [Tooltip("Optional full-screen button behind the sheet that closes the overlay.")]
        [SerializeField] Button backdropButton;

        Vector2 sheetRestPosition;
        Coroutine slide;

        protected override void Awake()
        {
            base.Awake();
            if (sheet != null) sheetRestPosition = sheet.anchoredPosition;
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (backdropButton != null) backdropButton.onClick.AddListener(Hide);
        }

        void OnEnable() => GameEvents.StateChanged += HandleStateChanged;

        void OnDisable() => GameEvents.StateChanged -= HandleStateChanged;

        // The how-to belongs to the screen it was opened from, so close it when the app moves on.
        void HandleStateChanged(AppState state) => Hide();

        protected override void OnShowing()
        {
            if (sheet == null) return;
            if (slide != null) StopCoroutine(slide);
            sheet.anchoredPosition = sheetRestPosition - new Vector2(0f, slideDistance);
            slide = StartCoroutine(Tween.Run(0.6f,
                t => sheet.anchoredPosition = sheetRestPosition - new Vector2(0f, slideDistance * (1f - t)),
                Tween.EaseOutCubic));
        }
    }
}
