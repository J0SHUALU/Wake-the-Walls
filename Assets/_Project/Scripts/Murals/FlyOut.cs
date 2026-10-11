using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Sends pieces of a layered mural out of the wall and around the viewer while it is in view.
    /// Copies of chosen elements leave their painted spot, spin and drift towards the user, then fade.
    /// A trail of paint sparks can also travel between chosen elements, arcing out of the wall.
    /// Which elements are used is set on the prefab, so the script holds no mural names.
    /// </summary>
    public class FlyOut : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] float startDelay = 2.5f;

        [Header("Flying copies")]
        [SerializeField] string[] copyElements = new string[0];
        [SerializeField] Vector2 interval = new Vector2(1.2f, 2.4f);
        [SerializeField] int maxFlying = 4;
        [SerializeField] float flightSeconds = 4f;
        [Tooltip("How far towards the viewer a copy travels, in metres (1.5 at most).")]
        [SerializeField, Range(0.3f, 1.5f)] float outDistance = 1f;
        [Tooltip("Sideways swing as a share of the mural width.")]
        [SerializeField, Range(0f, 0.5f)] float swing = 0.25f;
        [SerializeField] float rise = 0.4f;
        [SerializeField] float spinDegrees = 300f;
        [SerializeField] float endScale = 0.6f;

        [Header("Spark trail")]
        [SerializeField] string[] sparkPath = new string[0];
        [SerializeField] PaintBurst sparkPrefab;
        [SerializeField] Color[] sparkColours = { new Color(1f, 0.82f, 0.35f), new Color(1f, 0.95f, 0.75f) };
        [SerializeField] float secondsPerHop = 1.2f;
        [SerializeField] float sparkLift = 0.45f;
        [SerializeField] float sparkSpacing = 0.07f;
        [SerializeField] float trailPause = 2f;

        readonly List<MuralLayer> flying = new List<MuralLayer>();
        Coroutine copies, sparks;

        void OnEnable()
        {
            GameEvents.MuralFound += HandleFound;
            GameEvents.MuralLost += HandleLost;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleFound;
            GameEvents.MuralLost -= HandleLost;
        }

        void HandleFound(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            StopAll();
            if (copyElements.Length > 0) copies = StartCoroutine(SendCopies());
            if (sparkPath.Length > 1 && sparkPrefab != null) sparks = StartCoroutine(RunSparks());
        }

        void HandleLost(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            StopAll();
            foreach (var copy in flying) if (copy != null) Destroy(copy.gameObject);
            flying.Clear();
        }

        void StopAll()
        {
            if (copies != null) StopCoroutine(copies);
            if (sparks != null) StopCoroutine(sparks);
            copies = sparks = null;
        }

        IEnumerator SendCopies()
        {
            yield return new WaitForSeconds(startDelay);
            while (experience.IsRunning)
            {
                flying.RemoveAll(c => c == null);
                if (flying.Count < maxFlying)
                {
                    string element = copyElements[Random.Range(0, copyElements.Length)];
                    if (experience.Rig.TryGetLayer(element, out var source)) StartCoroutine(Fly(CopyOf(source)));
                }
                yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
            }
        }

        MuralLayer CopyOf(MuralLayer source)
        {
            var copy = LayerCopy.Make(source, experience.Rig.MuralSize);
            flying.Add(copy);
            return copy;
        }

        IEnumerator Fly(MuralLayer copy)
        {
            Vector2 start = copy.PivotLocal;
            float width = experience.Rig.MuralSize.x;
            float side = Random.value < 0.5f ? -1f : 1f;
            float phase = Random.Range(0f, Mathf.PI * 2f);
            for (float time = 0f; time < flightSeconds && copy != null; time += Time.deltaTime)
            {
                float t = time / flightSeconds;
                float x = side * swing * width * Mathf.Sin(Mathf.PI * 1.2f * t + phase * 0.1f) * t;
                float y = rise * t + 0.06f * Mathf.Sin(phase + t * 6f);
                float z = -outDistance * Tween.EaseOutCubic(Mathf.Min(1f, t * 1.4f));
                copy.Pivot.localPosition = new Vector3(start.x + x, start.y + y, z);
                copy.Pivot.localRotation = Quaternion.Euler(25f * Mathf.Sin(phase + t * 5f), side * 40f * t, side * spinDegrees * t);
                copy.Pivot.localScale = Vector3.one * Mathf.Lerp(1f, endScale, t);
                copy.SetAlpha(Mathf.Min(Mathf.Clamp01(t / 0.12f), Mathf.Clamp01((1f - t) / 0.25f)));
                yield return null;
            }
            if (copy != null)
            {
                flying.Remove(copy);
                Destroy(copy.gameObject);
            }
        }

        IEnumerator RunSparks()
        {
            yield return new WaitForSeconds(startDelay);
            while (experience.IsRunning)
            {
                for (int i = 0; i < sparkPath.Length - 1 && experience.IsRunning; i++)
                {
                    if (!experience.Rig.TryGetLayer(sparkPath[i], out var from) || !experience.Rig.TryGetLayer(sparkPath[i + 1], out var to)) continue;
                    float next = 0f;
                    for (float time = 0f; time < secondsPerHop; time += Time.deltaTime)
                    {
                        float u = time / secondsPerHop;
                        if (time >= next)
                        {
                            Vector2 flat = Vector2.Lerp(from.PivotLocal, to.PivotLocal, Tween.EaseInOutCubic(u));
                            var at = new Vector3(flat.x, flat.y, -0.02f - sparkLift * Mathf.Sin(Mathf.PI * u));
                            PaintBurst.Spawn(sparkPrefab, experience.transform, at, sparkColours);
                            next += sparkSpacing;
                        }
                        yield return null;
                    }
                }
                yield return new WaitForSeconds(trailPause);
            }
        }
    }
}
