using System.Collections;
using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Mural 3: the wall opens into the desert. After the shared fade in, sand starts blowing along the
    /// bottom, the clouds drift slowly across the sky, the plants peel a little off the wall and lean into
    /// the wind, and the desert floor spreads out past the bottom of the painting onto the ground in front of the viewer.
    /// </summary>
    public class Mural03Experience : LayeredMuralExperience
    {
        [Header("Wind")]
        [SerializeField] ParticleSystem sand;
        [Tooltip("Elements that peel off the wall and lean in the wind.")]
        [SerializeField] string[] leanElements = new string[0];
        [SerializeField, Range(0f, 0.3f)] float peelMeters = 0.05f;
        [Tooltip("Steady lean in degrees, added on top of the sway. Negative leans right.")]
        [SerializeField, Range(-10f, 10f)] float leanDegrees = -3f;

        [Header("Sky")]
        [Tooltip("Elements that drift slowly sideways, for example the clouds. They move right and back, never past their painted spot to the left.")]
        [SerializeField] string[] driftElements = new string[0];
        [SerializeField, Range(0f, 0.5f)] float driftMeters = 0.12f;
        [Tooltip("Full drift cycles per second. Keep it very slow.")]
        [SerializeField, Range(0.005f, 0.2f)] float driftSpeed = 0.03f;

        [Header("Floor")]
        [SerializeField] DesertFloor floor;

        Coroutine intro, lean;
        float driftTime;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (floor != null) floor.Build(Data.PhysicalSizeMeters);
            if (sand != null)
            {
                sand.transform.localPosition = new Vector3(-Data.PhysicalSizeMeters.x * 0.55f, -Data.PhysicalSizeMeters.y * 0.45f, -0.05f);
                sand.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        protected override void OnStarted(bool resumed)
        {
            StopRoutines();
            intro = StartCoroutine(WakeDesert(resumed));
        }

        protected override void OnStopped()
        {
            StopRoutines();
            if (sand != null) sand.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            ForEachLean(m => m.Return());
            lean = StartCoroutine(SetLean(0f, 0.6f));
            if (floor != null) floor.PullBack();
        }

        protected override void OnResetState()
        {
            StopRoutines();
            if (sand != null) sand.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ForEachLean(m => m.ExtraDegrees = 0f);
            driftTime = 0f;
            ForEach(driftElements, m => m.ExtraOffset = Vector2.zero);
            if (floor != null) floor.Hide();
        }

        // Clouds drift only while the mural is on screen, and pick up where they left off when it comes back.
        void Update()
        {
            if (!IsRunning || driftElements.Length == 0) return;
            driftTime += Time.deltaTime;
            float x = driftMeters * (0.5f - 0.5f * Mathf.Cos(Mathf.PI * 2f * driftSpeed * driftTime));
            ForEach(driftElements, m => m.ExtraOffset = new Vector2(x, 0f));
        }

        IEnumerator WakeDesert(bool resumed)
        {
            yield return new WaitForSeconds(resumed ? 0.2f : FadeDuration);
            if (sand != null) sand.Play(true);
            ForEachLean(m => m.Peel(peelMeters, 1.2f));
            lean = StartCoroutine(SetLean(leanDegrees, 1.5f));
            if (floor != null && !floor.IsOut)
            {
                yield return new WaitForSeconds(0.8f);
                floor.SpreadOut();
            }
        }

        // Eases the steady lean in or out on every lean element.
        IEnumerator SetLean(float target, float seconds)
        {
            float start = 0f;
            if (leanElements.Length > 0 && MotionSet.TryGetMotion(leanElements[0], out LayerMotion first)) start = first.ExtraDegrees;
            float time = 0f;
            while (time < seconds)
            {
                time += Time.deltaTime;
                float value = Mathf.Lerp(start, target, Core.Tween.EaseInOutCubic(Mathf.Clamp01(time / seconds)));
                ForEachLean(m => m.ExtraDegrees = value);
                yield return null;
            }
        }

        void ForEachLean(System.Action<LayerMotion> action) => ForEach(leanElements, action);

        void ForEach(string[] elements, System.Action<LayerMotion> action)
        {
            foreach (string element in elements)
                if (MotionSet.TryGetMotion(element, out LayerMotion motion)) action(motion);
        }

        void StopRoutines()
        {
            if (intro != null) StopCoroutine(intro);
            if (lean != null) StopCoroutine(lean);
            intro = lean = null;
        }
    }
}
