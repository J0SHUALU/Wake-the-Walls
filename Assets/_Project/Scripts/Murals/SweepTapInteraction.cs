using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Murals
{
    // Tapping this part of the wall sends light through one or more masked details, such as the beams up a tower.
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class SweepTapInteraction : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
        [SerializeField] MaskSweep[] sweeps = new MaskSweep[0];
        [SerializeField] string interactionId = "sweep";
        [SerializeField] UnityEvent<string> used = new UnityEvent<string>();

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnTapped()
        {
            if (!experience.IsRunning) return;
            foreach (MaskSweep sweep in sweeps) if (sweep != null) sweep.Play();
            used.Invoke(interactionId);
        }
    }
}
