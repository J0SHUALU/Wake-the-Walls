using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// Shown when a mural is finished: the big tilted wall stamp with the mural's number, a title,
    /// how many walls are still asleep, the stamps row for this session, and the ways onward:
    /// Find the next wall, Replay, About this mural and Home.
    /// </summary>
    public class CompletionScreen : UIScreen
    {
        [SerializeField] MuralLibrary library;

        [Tooltip("The scene's state machine. Found automatically if left empty.")]
        [SerializeField] AppStateMachine stateMachine;

        [Header("Content")]
        [SerializeField] TMP_Text label;
        [SerializeField] string labelFormat = "MURAL {0:00} / COMPLETE";
        [SerializeField] TMP_Text stampNumber;
        [SerializeField] TMP_Text title;
        [Tooltip("{0} is the mural's name. Used when the mural has a name.")]
        [SerializeField] string titleWithName = "YOU WOKE {0}";
        [Tooltip("{0} is the mural's number. Used when the name is still empty.")]
        [SerializeField] string titleWithoutName = "WALL {0:00} IS AWAKE";
        [SerializeField] TMP_Text remaining;
        [SerializeField] StampsRow stamps;

        [Header("Buttons")]
        [SerializeField] Button nextButton;
        [SerializeField] Button replayButton;
        [SerializeField] Button aboutButton;
        [SerializeField] Button homeButton;
        [Tooltip("Raised by About this mural. Wire it to the info panel.")]
        [SerializeField] UnityEvent aboutRequested = new UnityEvent();

        static readonly string[] Words = { "No", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };

        MuralData current;

        protected override void Awake()
        {
            base.Awake();
            if (nextButton != null) nextButton.onClick.AddListener(() => GoTo(AppState.Scanning));
            if (homeButton != null) homeButton.onClick.AddListener(() => GoTo(AppState.Start));
            if (replayButton != null) replayButton.onClick.AddListener(Replay);
            if (aboutButton != null) aboutButton.onClick.AddListener(aboutRequested.Invoke);
        }

        void OnEnable() => GameEvents.MuralCompleted += HandleCompleted;

        void OnDisable() => GameEvents.MuralCompleted -= HandleCompleted;

        void HandleCompleted(MuralData mural) => current = mural;

        /// <summary>Goes back into the experience and restarts the mural.</summary>
        public void Replay()
        {
            GoTo(AppState.Experience);
            GameEvents.RaiseResetRequested();
        }

        void GoTo(AppState state)
        {
            if (stateMachine == null) stateMachine = FindAnyObjectByType<AppStateMachine>();
            if (stateMachine != null) stateMachine.GoTo(state);
        }

        protected override void OnShowing()
        {
            int total = library != null ? library.Murals.Count : 0;
            int number = IndexOf(current) + 1;

            if (label != null) label.text = string.Format(labelFormat, number);
            if (stampNumber != null) stampNumber.text = number.ToString("00");

            if (title != null)
            {
                string name = current != null ? current.DisplayName : null;
                title.text = string.IsNullOrWhiteSpace(name)
                    ? string.Format(titleWithoutName, number)
                    : string.Format(titleWithName, name.Trim().ToUpperInvariant());
            }

            if (remaining != null) remaining.text = RemainingText(Mathf.Max(0, total - SessionProgress.Count));
            if (stamps != null) stamps.Show(SessionProgress.Woken);
        }

        static string RemainingText(int left)
        {
            if (left == 0) return "Every wall on campus is awake.";
            string count = left < Words.Length ? Words[left] : left.ToString();
            return left == 1
                ? $"{count} more wall is still asleep around campus."
                : $"{count} more walls are still asleep around campus.";
        }

        int IndexOf(MuralData mural)
        {
            if (library == null || mural == null) return 0;
            for (int i = 0; i < library.Murals.Count; i++)
            {
                if (library.Murals[i] == mural) return i;
            }
            return 0;
        }
    }
}
