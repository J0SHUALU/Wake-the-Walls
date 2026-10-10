using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// Bottom sheet with the current mural's number and size, its title, where it is, and its story.
    /// Everything is read from the MuralData sent with MuralFound. Empty fields are hidden, never
    /// shown as placeholders. The sheet fades and slides up when opened from the HUD.
    /// </summary>
    public class InfoPanel : UIScreen
    {
        [SerializeField] MuralLibrary library;

        [Header("Content")]
        [SerializeField] TMP_Text label;
        [Tooltip("{0} is the mural number, {1} and {2} are its width and height in metres.")]
        [SerializeField] string labelFormat = "MURAL {0:00} / {1} × {2} M";
        [SerializeField] TMP_Text title;
        [SerializeField] GameObject whereGroup;
        [SerializeField] TMP_Text whereValue;
        [SerializeField] TMP_Text story;
        [SerializeField] GameObject dividerAfterTitle;
        [SerializeField] GameObject dividerAfterFacts;

        [Header("Sheet")]
        [SerializeField] RectTransform sheet;
        [SerializeField] float slideDistance = 120f;
        [SerializeField] Button closeButton;
        [SerializeField] Button backButton;

        MuralData current;
        Vector2 sheetRestPosition;
        Coroutine slide;

        protected override void Awake()
        {
            base.Awake();
            if (sheet != null) sheetRestPosition = sheet.anchoredPosition;
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (backButton != null) backButton.onClick.AddListener(Hide);
        }

        void OnEnable()
        {
            GameEvents.MuralFound += HandleMuralFound;
            GameEvents.StateChanged += HandleStateChanged;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleMuralFound;
            GameEvents.StateChanged -= HandleStateChanged;
        }

        void HandleMuralFound(MuralData mural) => current = mural;

        // The panel only belongs to the experience, so close it when the app moves on.
        void HandleStateChanged(AppState state)
        {
            if (state != AppState.Experience) Hide();
        }

        protected override void OnShowing()
        {
            Fill();
            if (sheet == null) return;
            if (slide != null) StopCoroutine(slide);
            sheet.anchoredPosition = sheetRestPosition - new Vector2(0f, slideDistance);
            slide = StartCoroutine(Tween.Run(0.6f,
                t => sheet.anchoredPosition = sheetRestPosition - new Vector2(0f, slideDistance * (1f - t)),
                Tween.EaseOutCubic));
        }

        void Fill()
        {
            if (current == null) return;

            int index = IndexOf(current);
            Vector2 size = current.PhysicalSizeMeters;
            if (label != null)
            {
                label.text = string.Format(CultureInfo.InvariantCulture, labelFormat, index + 1,
                    size.x.ToString("0.#", CultureInfo.InvariantCulture), size.y.ToString("0.#", CultureInfo.InvariantCulture));
            }

            SetText(title, current.DisplayName?.ToUpperInvariant());
            bool hasWhere = SetText(whereValue, current.Location);
            if (whereGroup != null) whereGroup.SetActive(hasWhere);
            bool hasStory = SetText(story, current.Story);

            if (dividerAfterTitle != null) dividerAfterTitle.SetActive(hasWhere || hasStory);
            if (dividerAfterFacts != null) dividerAfterFacts.SetActive(hasWhere && hasStory);

            if (sheet != null) LayoutRebuilder.ForceRebuildLayoutImmediate(sheet);
        }

        // Shows the text if there is any, otherwise hides its object. Returns whether it is shown.
        static bool SetText(TMP_Text target, string value)
        {
            bool has = !string.IsNullOrWhiteSpace(value);
            if (target == null) return has;
            target.gameObject.SetActive(has);
            if (has) target.text = value.Trim();
            return has;
        }

        int IndexOf(MuralData mural)
        {
            if (library == null) return 0;
            for (int i = 0; i < library.Murals.Count; i++)
            {
                if (library.Murals[i] == mural) return i;
            }
            return 0;
        }
    }
}
