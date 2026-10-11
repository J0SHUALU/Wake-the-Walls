using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    public abstract class LayeredMuralExperience : MuralExperience
    {
        [SerializeField] LayeredMuralRig rig;
        [SerializeField] LayerMotionSet motionSet;
        [SerializeField] LayerParallax parallax;

        [Header("Fades")]
        [SerializeField, Range(0.6f, 1.5f)] float fadeDuration = 1.2f;

        [Header("Drifting paint dust")]
        [SerializeField] PaintBurst dustPrefab;
        [Tooltip("Elements dust drifts from, for example the tree canopies.")]
        [SerializeField] string[] dustSources = new string[0];
        [Tooltip("Offset from each source's pivot to where the dust starts, in metres.")]
        [SerializeField] Vector3 dustOffset = new Vector3(0f, 0.25f, -0.02f);
        [SerializeField] Vector2 dustInterval = new Vector2(2.5f, 5f);

        [Header("Completion")]
        [Tooltip("How many different interactions the user must use before the mural completes.")]
        [SerializeField, Min(1)] int requiredInteractions = 2;

        readonly HashSet<string> usedInteractions = new HashSet<string>();
        Coroutine fade, dust;
        float alpha;

        public bool IsRunning { get; private set; }

        public LayeredMuralRig Rig => rig;

        public LayerMotionSet MotionSet => motionSet;

        public PaintBurst DustPrefab => dustPrefab;

        protected float FadeDuration => fadeDuration;

        protected override void OnInitialized()
        {
            rig.Build(Data.PhysicalSizeMeters);
            motionSet.Apply(rig);
            SetAlpha(0f);
            if (parallax != null) parallax.Active = false;
        }

        public override void OnFound() => StartRunning(false);

        public override void OnLost()
        {
            IsRunning = false;
            OnStopped();
            StopDust();
            motionSet.PauseAll();
            if (parallax != null) parallax.Active = false;
            FadeTo(0f);
        }

        public override void OnResume() => StartRunning(true);

        public override void ResetExperience()
        {
            base.ResetExperience();
            IsRunning = false;
            StopDust(true);
            if (fade != null) StopCoroutine(fade);
            motionSet.ResetAll();
            if (parallax != null) parallax.Active = false;
            usedInteractions.Clear();
            SetAlpha(0f);
            OnResetState();
        }

        public void ReportInteraction(string id)
        {
            usedInteractions.Add(id);
            if (usedInteractions.Count >= requiredInteractions) Complete();
        }

        protected virtual void OnStarted(bool resumed) { }

        protected virtual void OnStopped() { }

        protected virtual void OnResetState() { }

        void StartRunning(bool resumed)
        {
            IsRunning = true;
            FadeTo(1f);
            motionSet.PlayAll();
            if (parallax != null) parallax.Active = true;
            StopDust();
            if (dustPrefab != null && dustSources.Length > 0) dust = StartCoroutine(DriftDust());
            OnStarted(resumed);
        }

        IEnumerator DriftDust()
        {
            yield return new WaitForSeconds(fadeDuration);
            while (IsRunning)
            {
                string source = dustSources[Random.Range(0, dustSources.Length)];
                if (rig.TryGetLayer(source, out MuralLayer layer))
                {
                    Vector3 at = (Vector3)layer.PivotLocal + dustOffset + (Vector3)(Random.insideUnitCircle * 0.15f);
                    PaintBurst.Spawn(dustPrefab, transform, at, Data.Palette);
                }
                yield return new WaitForSeconds(Random.Range(dustInterval.x, dustInterval.y));
            }
        }

        void StopDust(bool clearBursts = false)
        {
            if (dust != null) StopCoroutine(dust);
            dust = null;
            if (!clearBursts) return;
            foreach (PaintBurst burst in GetComponentsInChildren<PaintBurst>()) Destroy(burst.gameObject);
        }

        void FadeTo(float target)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Tween.Value(alpha, target, SetAlpha, fadeDuration));
        }

        void SetAlpha(float value)
        {
            alpha = value;
            rig.SetAlpha(value);
        }
    }
}
