using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    public class Mural04Experience : LayeredMuralExperience
    {
        [Header("Gold lines")]
        [SerializeField] MaskGlow goldLines;
        [Tooltip("Steady glow on the lines while the mural is running, 0 to 1.")]
        [SerializeField, Range(0f, 1f)] float restingGlow = 0.25f;
        [SerializeField, Range(0.8f, 4f)] float flowSeconds = 2f;
        [Tooltip("Light runs out along the lines from here on its own, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Vector2 flowCentre = new Vector2(0.705f, 0.51f);
        [SerializeField, Min(0.5f)] float flowEvery = 3f;

        [Header("Portraits")]
        [Tooltip("Portrait elements, in the same order as the rings below.")]
        [SerializeField] string[] portraits = new string[0];
        [Tooltip("Ring round each face: x and y are its centre (0 to 1, 0,0 bottom left), z is its radius as a share of the mural width.")]
        [SerializeField] Vector3[] rings = new Vector3[0];
        [SerializeField] Color ringColour = new Color(1f, 0.84f, 0.47f, 1f);
        [SerializeField, Range(0.002f, 0.03f)] float ringWidth = 0.012f;

        [Header("Mask eyes")]
        [SerializeField] MaskGlow maskEyes;
        [Tooltip("Seconds the eyes take to light up once every face has been tapped.")]
        [SerializeField, Range(0.5f, 4f)] float eyesSeconds = 2f;

        readonly HashSet<string> opened = new HashSet<string>();
        readonly List<FaceRing> faceRings = new List<FaceRing>();
        Coroutine flow, wake;
        bool eyesAwake;

        public MaskGlow GoldLines => goldLines;

        public string[] Portraits => portraits;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Vector2 size = Data.PhysicalSizeMeters;
            if (goldLines != null) goldLines.Build(size);
            if (maskEyes != null) maskEyes.Build(size);
            for (int i = 0; i < rings.Length; i++)
            {
                Vector3 r = rings[i];
                var centre = new Vector3((r.x - 0.5f) * size.x, (r.y - 0.5f) * size.y, -0.004f);
                faceRings.Add(FaceRing.Create(transform, centre, r.z * size.x, ringWidth * size.x * 0.4f, ringColour));
            }
        }

        protected override void OnStarted(bool resumed)
        {
            if (goldLines != null)
            {
                goldLines.FadeAlpha(1f, FadeDuration);
                goldLines.FadeIntensity(restingGlow, FadeDuration);
            }
            if (eyesAwake && maskEyes != null) maskEyes.FadeAlpha(1f, FadeDuration);
            for (int i = 0; i < portraits.Length && i < faceRings.Count; i++)
                if (opened.Contains(portraits[i])) faceRings[i].Show(0.4f);
            if (flow != null) StopCoroutine(flow);
            flow = StartCoroutine(Flow());
        }

        protected override void OnStopped()
        {
            if (flow != null) StopCoroutine(flow);
            flow = null;
            if (goldLines != null) goldLines.FadeAlpha(0f, FadeDuration);
            if (maskEyes != null) maskEyes.FadeAlpha(0f, FadeDuration);
            foreach (FaceRing ring in faceRings) ring.Hide();
        }

        protected override void OnResetState()
        {
            if (flow != null) StopCoroutine(flow);
            if (wake != null) StopCoroutine(wake);
            flow = wake = null;
            opened.Clear();
            eyesAwake = false;
            if (goldLines != null) goldLines.ResetGlow();
            if (maskEyes != null) maskEyes.ResetGlow();
            foreach (FaceRing ring in faceRings) ring.ResetRing();
        }

        // Lights a face where it is painted: a ring draws round it and light runs out along the gold lines.
        public bool OpenPortrait(string element)
        {
            if (!IsRunning) return false;
            int index = System.Array.IndexOf(portraits, element);
            if (index >= 0 && index < faceRings.Count && !faceRings[index].IsShown) faceRings[index].Show();
            if (index >= 0 && index < rings.Length) FlowFrom(new Vector2(rings[index].x, rings[index].y));
            if (opened.Add(element) && opened.Count >= portraits.Length && !eyesAwake) wake = StartCoroutine(WakeEyes());
            return true;
        }

        public void FlowFrom(Vector2 pointOnMural)
        {
            if (goldLines != null && IsRunning) goldLines.Flow(pointOnMural, flowSeconds);
        }

        IEnumerator Flow()
        {
            yield return new WaitForSeconds(FadeDuration + 1f);
            while (IsRunning)
            {
                FlowFrom(flowCentre);
                yield return new WaitForSeconds(flowEvery);
            }
            flow = null;
        }

        IEnumerator WakeEyes()
        {
            eyesAwake = true;
            if (maskEyes != null)
            {
                maskEyes.FadeAlpha(1f, FadeDuration);
                maskEyes.FadeIntensity(1f, eyesSeconds);
            }
            yield return new WaitForSeconds(eyesSeconds);
            wake = null;
            Complete();
        }
    }
}
