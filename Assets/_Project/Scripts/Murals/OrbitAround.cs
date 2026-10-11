using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    public class OrbitAround : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] string[] elements = new string[0];
        [SerializeField] float startDelay = 3f;
        [Tooltip("Size of the orbiting copies compared with the painted element.")]
        [SerializeField, Range(0.15f, 1f)] float scale = 0.45f;
        [SerializeField] float lapSeconds = 10f;
        [Tooltip("Orbit width as a share of the mural width.")]
        [SerializeField, Range(0.2f, 1f)] float widthShare = 0.6f;
        [Tooltip("Closest and furthest the loop comes out from the wall, in metres.")]
        [SerializeField] Vector2 depth = new Vector2(0.2f, 1.2f);
        [Tooltip("Up and down wave as a share of the mural height.")]
        [SerializeField, Range(0f, 0.5f)] float wave = 0.15f;
        [SerializeField] float spinDegrees = 20f;

        readonly List<MuralLayer> copies = new List<MuralLayer>();
        Coroutine fade;
        float alpha;

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
            Fade(1f, copies.Count == 0 ? startDelay : 0f);
        }

        void HandleLost(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            Fade(0f, 0f);
        }

        void Fade(float to, float delay)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(FadeRoutine(to, delay));
        }

        IEnumerator FadeRoutine(float to, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (copies.Count == 0 && to > 0f) Build();
            yield return Tween.Value(alpha, to, v => alpha = v, 1f);
        }

        void Build()
        {
            foreach (string element in elements)
            {
                if (experience.Rig.TryGetLayer(element, out var source)) copies.Add(LayerCopy.Make(source, experience.Rig.MuralSize, 70));
            }
        }

        void Update()
        {
            if (copies.Count == 0) return;
            Vector2 size = experience.Rig.MuralSize;
            for (int i = 0; i < copies.Count; i++)
            {
                var copy = copies[i];
                if (copy == null) continue;
                float angle = Mathf.PI * 2f * (Time.time / lapSeconds + i / (float)copies.Count);
                float x = Mathf.Cos(angle) * size.x * 0.5f * widthShare;
                float y = Mathf.Sin(angle * 2f) * size.y * 0.5f * wave;
                float z = -Mathf.Lerp(depth.x, depth.y, 0.5f + 0.5f * Mathf.Sin(angle));
                copy.Pivot.localPosition = new Vector3(x, y, z);
                copy.Pivot.localRotation = Quaternion.Euler(0f, 0f, spinDegrees * Mathf.Sin(angle * 3f));
                copy.Pivot.localScale = Vector3.one * scale;
                copy.SetAlpha(alpha);
            }
        }
    }
}
