using UnityEngine;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Murals
{
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class GoldLineTapInteraction : MonoBehaviour
    {
        [SerializeField] Mural04Experience experience;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.5f, 0.1f, 0.06f, 0.8f);
        [Tooltip("Where the light starts, 0 to 1 across the mural. Uses the centre of the area when left at 0,0.")]
        [SerializeField] Vector2 flowPoint;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnTapped() => experience.FlowFrom(flowPoint == Vector2.zero ? area.center : flowPoint);
    }
}
