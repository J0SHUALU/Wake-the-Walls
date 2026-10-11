using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    public class StartScreen : UIScreen
    {
        [SerializeField] MuralLibrary library;

        [Tooltip("The scene's state machine. Found automatically if left empty.")]
        [SerializeField] AppStateMachine stateMachine;

        [Header("Content")]
        [SerializeField] TMP_Text tagLabel;
        [Tooltip("{0} is the number of murals in the library.")]
        [SerializeField] string tagFormat = "ALU KIGALI / {0} MURALS";
        [SerializeField] StampsRow stamps;

        [Header("Buttons")]
        [SerializeField] Button startButton;
        [SerializeField] Button howItWorksButton;

        [Tooltip("Raised by the How it works link. Wire it to the help overlay.")]
        [SerializeField] UnityEvent howItWorksRequested = new UnityEvent();

        protected override void Awake()
        {
            base.Awake();
            if (startButton != null) startButton.onClick.AddListener(StartScanning);
            if (howItWorksButton != null) howItWorksButton.onClick.AddListener(howItWorksRequested.Invoke);
        }

        public void StartScanning()
        {
            if (stateMachine == null) stateMachine = FindAnyObjectByType<AppStateMachine>();
            if (stateMachine != null) stateMachine.GoTo(AppState.Scanning);
        }

        protected override void OnShowing()
        {
            int muralCount = library != null ? library.Murals.Count : 0;
            if (tagLabel != null) tagLabel.text = string.Format(tagFormat, muralCount);

            if (stamps != null) stamps.Show(SessionProgress.Woken);
        }
    }
}
