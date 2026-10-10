using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// A tap area over Mural 5's rings. Tapping makes the 3D rings spin faster and spread apart,
    /// then slow down and settle back into their orbit.
    /// </summary>
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class RingTapInteraction : MonoBehaviour
    {
        [SerializeField] Mural05Experience experience;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.18f, 0.43f, 0.33f, 0.57f);
        [Tooltip("How many times faster the rings spin straight after a tap.")]
        [SerializeField, Range(1f, 15f)] float spinBoost = 6f;
        [Tooltip("How far apart neighbouring rings move, in metres.")]
        [SerializeField, Range(0f, 0.3f)] float spreadMeters = 0.1f;
        [SerializeField, Range(0.2f, 1.5f)] float spreadSeconds = 0.5f;
        [SerializeField, Range(1f, 5f)] float settleSeconds = 3f;
        [SerializeField] AudioClip sound;
        [SerializeField] string interactionId = "rings";
        [SerializeField] UnityEvent<string> used = new UnityEvent<string>();

        Coroutine spread;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnDisable() => spread = null;

        void OnTapped()
        {
            if (!experience.IsRunning) return;
            IReadOnlyList<OrbitRing> rings = experience.Rings;
            foreach (OrbitRing ring in rings) ring.Boost(spinBoost, settleSeconds);
            if (spread != null) StopCoroutine(spread);
            spread = StartCoroutine(SpreadAndSettle(rings));
            if (sound != null) AudioSource.PlayClipAtPoint(sound, transform.position);
            used.Invoke(interactionId);
        }

        // Rings above the middle move up, rings below move down, then all drift back.
        IEnumerator SpreadAndSettle(IReadOnlyList<OrbitRing> rings)
        {
            var from = new float[rings.Count];
            for (int i = 0; i < rings.Count; i++) from[i] = rings[i].Spread;
            float middle = (rings.Count - 1) * 0.5f;
            yield return Tween.Run(spreadSeconds, t =>
            {
                for (int i = 0; i < rings.Count; i++)
                    rings[i].Spread = Mathf.Lerp(from[i], (middle - i) * spreadMeters, t);
            }, Tween.EaseOutCubic);
            yield return Tween.Run(settleSeconds, t =>
            {
                for (int i = 0; i < rings.Count; i++)
                    rings[i].Spread = Mathf.Lerp((middle - i) * spreadMeters, 0f, t);
            }, Tween.EaseInOutCubic);
            spread = null;
        }
    }
}
