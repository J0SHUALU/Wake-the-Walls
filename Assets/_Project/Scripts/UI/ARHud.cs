using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WakeTheWalls.Audio;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// The small corner HUD shown during a mural experience: the wall label tag at the top left
    /// (mural number and name) and four round buttons on the right. Replay restarts the mural;
    /// Info, Sound and Help raise events so the panels that own them can be wired in the Inspector.
    /// </summary>
    public class ARHud : UIScreen
    {
        [SerializeField] MuralLibrary library;

        [Header("Wall label")]
        [SerializeField] TMP_Text tagLabel;
        [Tooltip("{0} is the mural's number in the library.")]
        [SerializeField] string tagFormat = "MURAL {0:00} / AWAKE";
        [SerializeField] TMP_Text tagTitle;

        [Header("Buttons")]
        [SerializeField] Button infoButton;
        [SerializeField] Button soundButton;
        [SerializeField] Button replayButton;
        [SerializeField] Button helpButton;

        [Header("Sound icon")]
        [SerializeField] Image soundIcon;
        [SerializeField] Sprite soundOnSprite;
        [SerializeField] Sprite soundOffSprite;

        [Header("Events")]
        [SerializeField] UnityEvent infoRequested = new UnityEvent();
        [SerializeField] UnityEvent soundToggled = new UnityEvent();
        [SerializeField] UnityEvent helpRequested = new UnityEvent();

        /// <summary>The mural currently being experienced, or null.</summary>
        public MuralData CurrentMural { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (infoButton != null) infoButton.onClick.AddListener(infoRequested.Invoke);
            if (soundButton != null) soundButton.onClick.AddListener(soundToggled.Invoke);
            if (replayButton != null) replayButton.onClick.AddListener(GameEvents.RaiseResetRequested);
            if (helpButton != null) helpButton.onClick.AddListener(helpRequested.Invoke);
        }

        void OnEnable()
        {
            GameEvents.MuralFound += HandleMuralFound;
            AudioManager.MuteChanged += UpdateSoundIcon;
            UpdateSoundIcon(AudioManager.IsMuted);
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleMuralFound;
            AudioManager.MuteChanged -= UpdateSoundIcon;
        }

        void UpdateSoundIcon(bool muted)
        {
            if (soundIcon != null) soundIcon.sprite = muted ? soundOffSprite : soundOnSprite;
        }

        protected override void OnShowing() => Refresh();

        void HandleMuralFound(MuralData mural)
        {
            CurrentMural = mural;
            Refresh();
        }

        void Refresh()
        {
            if (CurrentMural == null) return;

            int index = -1;
            if (library != null)
            {
                for (int i = 0; i < library.Murals.Count; i++)
                {
                    if (library.Murals[i] == CurrentMural) index = i;
                }
            }

            if (tagLabel != null)
            {
                tagLabel.gameObject.SetActive(index >= 0);
                if (index >= 0) tagLabel.text = string.Format(tagFormat, index + 1);
            }

            // Names come only from MuralData. An empty name hides the title instead of showing filler.
            if (tagTitle != null)
            {
                bool hasName = !string.IsNullOrWhiteSpace(CurrentMural.DisplayName);
                tagTitle.gameObject.SetActive(hasName);
                if (hasName) tagTitle.text = CurrentMural.DisplayName.ToUpperInvariant();
            }
        }
    }
}
