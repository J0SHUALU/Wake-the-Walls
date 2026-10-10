using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// A tap area that makes one or more layers swell open from their pivot and settle back, while a
    /// burst of paint (petals, sparks) flies out towards the viewer in chosen colours.
    /// </summary>
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class PulseInteraction : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
        [SerializeField] string[] elements = new string[0];
        [SerializeField, Range(1f, 1.3f)] float openScale = 1.12f;
        [SerializeField, Range(0.3f, 1.5f)] float openSeconds = 0.6f;
        [SerializeField, Range(0.6f, 1.5f)] float settleSeconds = 1.2f;

        [Header("Burst")]
        [SerializeField] PaintBurst burstPrefab;
        [Tooltip("Colours for the burst. Uses the mural palette when empty.")]
        [SerializeField] Color[] burstColours = new Color[0];
        [Tooltip("Burst start, from the centre of the tap area, in metres. Negative Z is towards the viewer.")]
        [SerializeField] Vector3 burstOffset = new Vector3(0f, 0f, -0.25f);
        [SerializeField] AudioClip sound;

        [SerializeField] string interactionId = "pulse";
        [SerializeField] UnityEvent<string> used = new UnityEvent<string>();

        Coroutine pulse;
        AudioSource audioSource;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnTapped()
        {
            if (!experience.IsRunning) return;
            if (pulse != null) StopCoroutine(pulse);
            pulse = StartCoroutine(Pulse());
            if (burstPrefab != null)
            {
                Color[] colours = burstColours.Length > 0 ? burstColours : experience.Data.Palette;
                Vector3 at = TapArea.Centre(area, experience.Rig.MuralSize) + burstOffset;
                PaintBurst.Spawn(burstPrefab, experience.transform, at, colours);
            }
            if (sound != null)
            {
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.PlayOneShot(sound);
            }
            used.Invoke(interactionId);
        }

        IEnumerator Pulse()
        {
            foreach (string element in elements)
                if (experience.Rig.TryGetLayer(element, out MuralLayer layer))
                    StartCoroutine(Tween.Scale(layer.Pivot, Vector3.one * openScale, openSeconds, Tween.EaseOutBack));
            yield return new WaitForSeconds(openSeconds);
            foreach (string element in elements)
                if (experience.Rig.TryGetLayer(element, out MuralLayer layer))
                    StartCoroutine(Tween.Scale(layer.Pivot, Vector3.one, settleSeconds, Tween.EaseInOutCubic));
            pulse = null;
        }
    }
}
