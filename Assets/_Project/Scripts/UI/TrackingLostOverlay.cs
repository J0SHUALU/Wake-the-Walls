using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// The paused state. When the mural is lost it dims the HUD and shows the paused message
    /// with "I'm back on it" and "Scan a different mural". It hides again when the mural is found.
    /// </summary>
    public class TrackingLostOverlay : UIScreen
    {
        [SerializeField] MuralLibrary library;
        [SerializeField] ARHud hud;

        [Tooltip("The scene's state machine. Found automatically if left empty.")]
        [SerializeField] AppStateMachine stateMachine;

        [Header("Wall label")]
        [SerializeField] TMP_Text tagLabel;
        [Tooltip("{0} is the mural's number in the library.")]
        [SerializeField] string tagFormat = "MURAL {0:00} / PAUSED";
        [SerializeField] TMP_Text tagTitle;

        [Header("Buttons")]
        [SerializeField] Button backOnItButton;
        [SerializeField] Button scanOtherButton;

        MuralData current;

        protected override void Awake()
        {
            base.Awake();
            if (backOnItButton != null) backOnItButton.onClick.AddListener(Resume);
            if (scanOtherButton != null) scanOtherButton.onClick.AddListener(ScanOther);
        }

        void OnEnable()
        {
            GameEvents.MuralLost += HandleMuralLost;
            GameEvents.MuralFound += HandleMuralFound;
            GameEvents.StateChanged += HandleStateChanged;
        }

        void OnDisable()
        {
            GameEvents.MuralLost -= HandleMuralLost;
            GameEvents.MuralFound -= HandleMuralFound;
            GameEvents.StateChanged -= HandleStateChanged;
        }

        /// <summary>Closes the message and brings the HUD back while the user re-aims.</summary>
        public void Resume()
        {
            Hide();
            if (hud != null) hud.SetDimmed(false);
        }

        /// <summary>Leaves this mural and goes back to scanning.</summary>
        public void ScanOther()
        {
            if (stateMachine == null) stateMachine = FindAnyObjectByType<AppStateMachine>();
            if (stateMachine != null) stateMachine.GoTo(AppState.Scanning);
        }

        void HandleMuralLost(MuralData mural)
        {
            current = mural;
            if (hud != null) hud.SetDimmed(true);
            Show();
        }

        void HandleMuralFound(MuralData mural) => Resume();

        void HandleStateChanged(AppState state)
        {
            if (state != AppState.Experience) Hide();
        }

        protected override void OnShowing()
        {
            if (current == null) return;

            int index = -1;
            if (library != null)
            {
                for (int i = 0; i < library.Murals.Count; i++)
                {
                    if (library.Murals[i] == current) index = i;
                }
            }

            if (tagLabel != null)
            {
                tagLabel.gameObject.SetActive(index >= 0);
                if (index >= 0) tagLabel.text = string.Format(tagFormat, index + 1);
            }

            if (tagTitle != null)
            {
                bool hasName = !string.IsNullOrWhiteSpace(current.DisplayName);
                tagTitle.gameObject.SetActive(hasName);
                if (hasName) tagTitle.text = current.DisplayName.ToUpperInvariant();
            }
        }
    }
}
