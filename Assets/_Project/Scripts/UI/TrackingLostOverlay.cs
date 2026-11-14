using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
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

        [Header("Message")]
        [Tooltip("The big line on the overlay.")]
        [SerializeField] TMP_Text headline;
        [Tooltip("The smaller line under it.")]
        [SerializeField] TMP_Text body;
        [SerializeField] string lostTag = "MURAL {0:00} / PAUSED";
        [SerializeField] string lostHeadline = "The wall slipped\nout of view";
        [SerializeField] string lostBody = "Point back at the mural. It picks up right where you left it.";
        [SerializeField] string blockedTag = "MURAL {0:00} / BLOCKED";
        [SerializeField] string blockedHeadline = "Something is\nin the way";
        [SerializeField] string blockedBody = "Wait for a clear view of the mural, or move so you can see all of it.";
        [Tooltip("Widest the title can get before it wraps onto a second line.")]
        [SerializeField] float titleMaxWidth = 640f;

        [Header("Buttons")]
        [SerializeField] Button backOnItButton;
        [SerializeField] Button scanOtherButton;

        MuralData current;
        MuralData onScreen;

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
            GameEvents.MuralFocused += HandleMuralFocused;
            GameEvents.StateChanged += HandleStateChanged;
        }

        void OnDisable()
        {
            GameEvents.MuralLost -= HandleMuralLost;
            GameEvents.MuralFound -= HandleMuralFound;
            GameEvents.MuralFocused -= HandleMuralFocused;
            GameEvents.StateChanged -= HandleStateChanged;
        }

        public void Resume()
        {
            Hide();
            if (hud != null) hud.SetDimmed(false);
        }

        public void ScanOther()
        {
            if (stateMachine == null) stateMachine = FindAnyObjectByType<AppStateMachine>();
            if (stateMachine != null) stateMachine.GoTo(AppState.Scanning);
        }

        void HandleMuralLost(MuralData mural)
        {
            if (stateMachine == null) stateMachine = FindAnyObjectByType<AppStateMachine>();
            if (stateMachine != null && stateMachine.Current != AppState.Experience) return;
            if (onScreen != null && mural != onScreen) return;
            current = mural;
            if (hud != null) hud.SetDimmed(true);
            Show();
        }

        void HandleMuralFound(MuralData mural)
        {
            onScreen = mural;
            Resume();
        }

        void HandleMuralFocused(MuralData mural) => onScreen = mural;

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

            bool blocked = GameEvents.ViewBlocked;
            if (headline != null) headline.text = blocked ? blockedHeadline : lostHeadline;
            if (body != null) body.text = blocked ? blockedBody : lostBody;

            if (tagLabel != null)
            {
                tagLabel.gameObject.SetActive(index >= 0);
                string format = blocked ? blockedTag : lostTag;
                if (string.IsNullOrEmpty(format)) format = tagFormat;
                if (index >= 0) tagLabel.text = string.Format(format, index + 1);
            }

            if (tagTitle != null)
            {
                bool hasName = !string.IsNullOrWhiteSpace(current.DisplayName);
                tagTitle.gameObject.SetActive(hasName);
                if (hasName) TitleFit.Set(tagTitle, current.DisplayName.Trim(), titleMaxWidth);
            }
        }
    }
}
