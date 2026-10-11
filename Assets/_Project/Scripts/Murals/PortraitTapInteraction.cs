using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Murals
{
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class PortraitTapInteraction : MonoBehaviour
    {
        [SerializeField] Mural04Experience experience;
        [Tooltip("Portrait element from the layer manifest.")]
        [SerializeField] string element;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
        [Tooltip("Which info to show, passed on through InfoRequested. Uses the element when left empty.")]
        [SerializeField] string topic;
        [SerializeField] UnityEvent<string> infoRequested = new UnityEvent<string>();

        public UnityEvent<string> InfoRequested => infoRequested;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnTapped()
        {
            if (!experience.OpenPortrait(element)) return;
            infoRequested.Invoke(string.IsNullOrEmpty(topic) ? element : topic);
        }
    }
}
