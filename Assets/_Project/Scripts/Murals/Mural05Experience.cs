using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Mural 5: the rings come apart and rebuild in 3D. After the shared fade in, each painted ring is
    /// swapped for a real 3D ring that lifts off the wall and starts orbiting the figure's head, while
    /// the rays behind her pulse gently.
    /// </summary>
    public class Mural05Experience : LayeredMuralExperience
    {
        /// <summary>One painted ring and where it sits on the mural.</summary>
        [Serializable]
        public class RingSpec
        {
            [Tooltip("Ring element from the layer manifest.")]
            public string element;
            [Tooltip("Box around the painted ellipse, 0 to 1 across the mural (0,0 bottom left).")]
            public Rect painted = new Rect(0.3f, 0.5f, 0.2f, 0.05f);
        }

        [Header("Painted layers")]
        [Tooltip("Layers that stay hidden, for example the combined rings layer the single ring layers replace.")]
        [SerializeField] string[] hiddenElements = new string[0];

        [Header("3D rings")]
        [SerializeField] List<RingSpec> rings = new List<RingSpec>();
        [Tooltip("Material using the Painterly shader.")]
        [SerializeField] Material ringMaterial;
        [SerializeField] Color ringColour = new Color(0.91f, 0.6f, 0.27f, 1f);
        [Tooltip("Radius of the ring's band, in metres.")]
        [SerializeField, Range(0.005f, 0.08f)] float ringThickness = 0.025f;
        [Tooltip("How deep each ring is compared with its width. Keeps big rings within reach of the wall.")]
        [SerializeField, Range(0.05f, 1f)] float depthRatio = 0.25f;
        [SerializeField, Range(0f, 90f)] float spinSpeed = 14f;
        [SerializeField, Range(0f, 10f)] float rockDegrees = 3f;
        [SerializeField, Range(0.5f, 3f)] float liftSeconds = 1.6f;
        [SerializeField, Min(0f)] float ringStagger = 0.45f;

        [Header("Rays")]
        [SerializeField] MaskGlow[] rays = new MaskGlow[0];
        [SerializeField] Vector2 rayGlowRange = new Vector2(0.04f, 0.28f);
        [SerializeField, Range(0.8f, 5f)] float rayPulseSeconds = 2.4f;

        readonly List<OrbitRing> live = new List<OrbitRing>();
        readonly HashSet<int> released = new HashSet<int>();
        Coroutine intro;
        int nextRing;

        /// <summary>The 3D rings, for interaction scripts on this prefab.</summary>
        public IReadOnlyList<OrbitRing> Rings => live;

        /// <summary>The ray glows, for interaction scripts on this prefab.</summary>
        public IReadOnlyList<MaskGlow> Rays => rays;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            foreach (string element in hiddenElements)
                if (Rig.TryGetLayer(element, out MuralLayer hidden)) hidden.gameObject.SetActive(false);
            Vector2 size = Data.PhysicalSizeMeters;
            for (int i = 0; i < rings.Count; i++)
            {
                Rect box = rings[i].painted;
                float width = box.width * size.x, height = box.height * size.y;
                float depth = Rig.TryGetLayer(rings[i].element, out MuralLayer layer) ? layer.RestDepth : 0.003f;
                var centre = new Vector3((box.center.x - 0.5f) * size.x, (box.center.y - 0.5f) * size.y, -depth);
                var ring = new GameObject("OrbitRing" + i).AddComponent<OrbitRing>();
                ring.transform.SetParent(transform, false);
                ring.Build(ringMaterial, ringColour, centre, width * 0.5f - ringThickness, ringThickness,
                    (height - ringThickness * 2f) / Mathf.Max(0.01f, width - ringThickness * 2f),
                    depthRatio, spinSpeed, rockDegrees, (float)i / Mathf.Max(1, rings.Count));
                live.Add(ring);
            }
            foreach (MaskGlow ray in rays) if (ray != null) ray.Build(size);
        }

        protected override void OnStarted(bool resumed)
        {
            StopIntro();
            foreach (int i in released)
            {
                live[i].Show(FadeDuration);
                if (!live[i].IsOut) live[i].LiftOut(liftSeconds);
                live[i].IsPlaying = true;
            }
            for (int i = 0; i < rays.Length; i++)
            {
                if (rays[i] == null) continue;
                rays[i].FadeAlpha(1f, FadeDuration);
                rays[i].Breathe(rayGlowRange.x, rayGlowRange.y, rayPulseSeconds, (float)i / rays.Length);
            }
            if (nextRing < live.Count) intro = StartCoroutine(Intro());
        }

        protected override void OnStopped()
        {
            StopIntro();
            foreach (OrbitRing ring in live)
            {
                ring.IsPlaying = false;
                ring.Hide(FadeDuration * 0.5f);
            }
            foreach (MaskGlow ray in rays) if (ray != null) ray.FadeAlpha(0f, FadeDuration);
        }

        protected override void OnResetState()
        {
            StopIntro();
            nextRing = 0;
            released.Clear();
            foreach (OrbitRing ring in live) ring.ResetRing();
            foreach (RingSpec spec in rings)
                if (Rig.TryGetLayer(spec.element, out MuralLayer layer)) layer.Renderer.enabled = true;
            foreach (MaskGlow ray in rays) if (ray != null) ray.ResetGlow();
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(FadeDuration);
            for (; nextRing < live.Count; nextRing++)
            {
                OrbitRing ring = live[nextRing];
                yield return ring.Show(0.35f);
                if (Rig.TryGetLayer(rings[nextRing].element, out MuralLayer layer)) layer.Renderer.enabled = false;
                released.Add(nextRing);
                ring.IsPlaying = true;
                ring.LiftOut(liftSeconds);
                yield return new WaitForSeconds(ringStagger);
            }
            intro = null;
        }

        void StopIntro()
        {
            if (intro != null) StopCoroutine(intro);
            intro = null;
        }
    }
}
