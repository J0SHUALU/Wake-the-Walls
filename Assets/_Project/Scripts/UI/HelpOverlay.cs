using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Core;

namespace WakeTheWalls.UI
{
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

        void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
            GameEvents.MuralLost += HandleMuralLost;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
            GameEvents.MuralLost -= HandleMuralLost;
        }

        void HandleMuralLost(Murals.MuralData mural) => Hide();

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
