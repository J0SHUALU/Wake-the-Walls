using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class InfoTapInteraction : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
        [Tooltip("Which info to show, passed to the info hotspot.")]
        [SerializeField] string topic = "info";

        [Header("Response")]
        [SerializeField] PaintBurst sparklePrefab;
        [SerializeField] Color[] sparkleColours = { new Color(1f, 0.95f, 0.8f), new Color(1f, 0.85f, 0.55f) };
        [Tooltip("Where the sparkle starts, 0 to 1 across the mural. Uses the centre of the area when left at 0,0.")]
        [SerializeField] Vector2 sparklePoint;
        [SerializeField] AudioClip sound;

        [SerializeField] string interactionId = "info";
        [SerializeField] UnityEvent<string> used = new UnityEvent<string>();
        [SerializeField] UnityEvent<string> infoRequested = new UnityEvent<string>();

        public UnityEvent<string> InfoRequested => infoRequested;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnTapped()
        {
            if (!experience.IsRunning) return;
            if (sparklePrefab != null)
            {
                Vector2 size = experience.Rig.MuralSize;
                Vector3 at = sparklePoint == Vector2.zero
                    ? TapArea.Centre(area, size)
                    : new Vector3((sparklePoint.x - 0.5f) * size.x, (sparklePoint.y - 0.5f) * size.y, -0.05f);
                PaintBurst.Spawn(sparklePrefab, experience.transform, at, sparkleColours);
            }
            if (sound != null) AudioSource.PlayClipAtPoint(sound, transform.position);
            infoRequested.Invoke(topic);
            GameEvents.RaiseInfoRequested(topic);
            used.Invoke(interactionId);
        }
    }
}
