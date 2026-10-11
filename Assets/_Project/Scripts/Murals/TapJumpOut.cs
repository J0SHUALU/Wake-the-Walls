using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// When a tap area is tapped, copies of the linked elements jump out of the wall at the viewer,
    /// spin, hang in the air for a moment and fly back into their painted spot.
    /// Which tap area throws which elements is set on the prefab, so the script holds no mural names.
    /// </summary>
    public class TapJumpOut : MonoBehaviour
    {
        [Serializable]
        public class Link
        {
            public Interactable tap;
            public string[] elements = new string[0];
        }

        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] Link[] links = new Link[0];
        [Tooltip("How far towards the viewer the elements jump, in metres.")]
        [SerializeField, Range(0.3f, 1.5f)] float outDistance = 0.9f;
        [SerializeField] float rise = 0.15f;
        [SerializeField] float outSeconds = 0.6f;
        [SerializeField] float holdSeconds = 0.9f;
        [SerializeField] float backSeconds = 0.8f;
        [SerializeField] float spinDegrees = 360f;
        [Tooltip("Size while out, 1.3 is 30 percent bigger.")]
        [SerializeField, Range(1f, 2f)] float grow = 1.3f;
        [Tooltip("Seconds between each element of one tap leaving the wall.")]
        [SerializeField] float stagger = 0.12f;

        readonly List<MuralLayer> live = new List<MuralLayer>();

        void Start()
        {
            foreach (var link in links)
            {
                if (link.tap == null) continue;
                var current = link;
                link.tap.Tapped.AddListener(() => Jump(current));
            }
        }

        void OnEnable() => GameEvents.MuralLost += HandleLost;
        void OnDisable() => GameEvents.MuralLost -= HandleLost;

        void HandleLost(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            StopAllCoroutines();
            foreach (var copy in live) if (copy != null) Destroy(copy.gameObject);
            live.Clear();
        }

        void Jump(Link link)
        {
            if (experience == null || !experience.IsRunning) return;
            float delay = 0f;
            foreach (string element in link.elements)
            {
                if (!experience.Rig.TryGetLayer(element, out var source)) continue;
                var copy = LayerCopy.Make(source, experience.Rig.MuralSize, 60);
                live.Add(copy);
                StartCoroutine(Fly(copy, delay));
                delay += stagger;
            }
        }

        IEnumerator Fly(MuralLayer copy, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            Vector3 rest = copy.Pivot.localPosition;
            float side = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            float total = outSeconds + holdSeconds + backSeconds;

            for (float time = 0f; time < total && copy != null; time += Time.deltaTime)
            {
                float out01 = time < outSeconds ? Tween.EaseOutBack(time / outSeconds)
                    : time < outSeconds + holdSeconds ? 1f
                    : 1f - Tween.EaseInOutCubic((time - outSeconds - holdSeconds) / backSeconds);
                float bob = time > outSeconds && time < outSeconds + holdSeconds ? 0.03f * Mathf.Sin(time * 8f) : 0f;

                copy.Pivot.localPosition = rest + new Vector3(0f, rise * out01 + bob, -outDistance * out01);
                copy.Pivot.localRotation = Quaternion.Euler(0f, side * spinDegrees * Tween.EaseInOutCubic(time / total), 8f * side * out01);
                copy.Pivot.localScale = Vector3.one * Mathf.LerpUnclamped(1f, grow, out01);
                copy.SetAlpha(Mathf.Min(1f, time / 0.08f, (total - time) / 0.15f));
                yield return null;
            }

            if (copy != null)
            {
                live.Remove(copy);
                Destroy(copy.gameObject);
            }
        }
    }
}
