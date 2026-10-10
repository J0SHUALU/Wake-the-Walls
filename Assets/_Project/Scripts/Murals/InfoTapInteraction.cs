using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// A tap area over part of a mural that asks for an info panel about it, for example a building.
    /// It shows a small burst of light so the tap is visibly answered, then raises <see cref="InfoRequested"/>
    /// and <see cref="GameEvents.InfoRequested"/> with a topic id, so the UI's info panel can open.
    /// This script holds no facts itself.
    /// </summary>
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

        /// <summary>Raised with the topic id when the area is tapped. Connect the info hotspot here.</summary>
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
