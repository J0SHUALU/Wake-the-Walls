using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Mural 2: the flowers are the way in. After the shared fade in, the flower layers grow and open,
    /// then a few blooms grow out of the wall and float towards the viewer in 3D, in the painting's own colours.
    /// </summary>
    public class Mural02Experience : LayeredMuralExperience
    {
        /// <summary>One 3D bloom: where it grows from, its colours, and where it floats to.</summary>
        [Serializable]
        public class BloomSpec
        {
            [Tooltip("Spot on the wall it grows from, 0 to 1 across the mural (0,0 bottom left).")]
            public Vector2 wallPoint = new Vector2(0.5f, 0.5f);
            public Color petalColour = Color.white;
            public Color centreColour = new Color(0.9f, 0.75f, 0.3f);
            [Tooltip("Where it hovers, in metres from its wall spot. Keep Z between -0.3 and -1.5.")]
            public Vector3 floatOffset = new Vector3(0f, 0.1f, -0.6f);
            [Tooltip("Width when fully open, in metres.")]
            public float diameter = 0.25f;
        }

        [Header("Growing flowers")]
        [SerializeField] string[] growElements = new string[0];
        [SerializeField, Range(1f, 1.2f)] float growScale = 1.06f;
        [SerializeField, Range(0.6f, 1.5f)] float growSeconds = 1.2f;

        [Header("Floating blooms")]
        [SerializeField] Material bloomMaterial;
        [SerializeField] List<BloomSpec> blooms = new List<BloomSpec>();
        [SerializeField, Min(0f)] float bloomStagger = 0.7f;

        readonly List<FloatingBloom> live = new List<FloatingBloom>();
        readonly List<Coroutine> grows = new List<Coroutine>();
        Coroutine intro;
        bool introDone;

        /// <summary>The 3D blooms, for interaction scripts on this prefab.</summary>
        public IReadOnlyList<FloatingBloom> Blooms => live;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Vector2 size = Data.PhysicalSizeMeters;
            for (int i = 0; i < blooms.Count; i++)
            {
                BloomSpec spec = blooms[i];
                var home = new Vector3((spec.wallPoint.x - 0.5f) * size.x, (spec.wallPoint.y - 0.5f) * size.y, -0.01f);
                Vector3 offset = spec.floatOffset;
                offset.z = Mathf.Clamp(offset.z, -1.5f, -0.05f);
                var bloom = new GameObject("Bloom" + i).AddComponent<FloatingBloom>();
                bloom.transform.SetParent(transform, false);
                bloom.Build(bloomMaterial, spec.petalColour, spec.centreColour, spec.diameter, home, home + offset);
                live.Add(bloom);
            }
        }

        protected override void OnStarted(bool resumed)
        {
            StopIntro();
            if (!introDone) intro = StartCoroutine(Intro());
            else foreach (FloatingBloom bloom in live) bloom.ComeBack();
        }

        protected override void OnStopped()
        {
            StopIntro();
            foreach (FloatingBloom bloom in live) if (bloom.IsOut) bloom.FadeAway();
        }

        protected override void OnResetState()
        {
            StopIntro();
            introDone = false;
            foreach (FloatingBloom bloom in live) bloom.Hide();
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(FadeDuration);
            foreach (string element in growElements)
                if (Rig.TryGetLayer(element, out MuralLayer layer)) grows.Add(StartCoroutine(GrowPulse(layer.Pivot)));
            yield return new WaitForSeconds(growSeconds);
            foreach (FloatingBloom bloom in live)
            {
                bloom.FloatOut();
                yield return new WaitForSeconds(bloomStagger);
            }
            introDone = true;
        }

        // Swells the flowers from their pivot and lets them settle, as if they are opening.
        IEnumerator GrowPulse(Transform pivot)
        {
            yield return Tween.Scale(pivot, Vector3.one * growScale, growSeconds, Tween.EaseOutCubic);
            yield return Tween.Scale(pivot, Vector3.one, growSeconds, Tween.EaseInOutCubic);
        }

        void StopIntro()
        {
            if (intro != null) StopCoroutine(intro);
            intro = null;
            foreach (Coroutine grow in grows) if (grow != null) StopCoroutine(grow);
            grows.Clear();
            foreach (string element in growElements)
                if (Rig.TryGetLayer(element, out MuralLayer layer)) layer.Pivot.localScale = Vector3.one;
        }
    }
}
