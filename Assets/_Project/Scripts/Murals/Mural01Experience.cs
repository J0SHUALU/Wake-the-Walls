using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Mural 1: the painting wakes up. The layers fade in over the wall, the elements listed in the
    /// motion set start to sway one by one, and paint dust drifts down from the elements listed as
    /// dust sources. Completes once every required interaction has been used.
    /// </summary>
    public class Mural01Experience : MuralExperience
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

        /// <summary>True while the mural is on screen and running.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>The rig, for interaction scripts on this prefab.</summary>
        public LayeredMuralRig Rig => rig;

        /// <summary>The motion set, for interaction scripts on this prefab.</summary>
        public LayerMotionSet MotionSet => motionSet;

        /// <summary>The dust prefab, for interaction scripts on this prefab.</summary>
        public PaintBurst DustPrefab => dustPrefab;

        protected override void OnInitialized()
        {
            rig.Build(Data.PhysicalSizeMeters);
            motionSet.Apply(rig);
            SetAlpha(0f);
            if (parallax != null) parallax.Active = false;
        }

        /// <summary>Plays the intro: fade in, then the elements wake up one by one.</summary>
        public override void OnFound() => StartRunning();

        /// <summary>Pauses the motion and fades out. Used interactions are kept.</summary>
        public override void OnLost()
        {
            IsRunning = false;
            StopDust();
            motionSet.PauseAll();
            if (parallax != null) parallax.Active = false;
            FadeTo(0f);
        }

        /// <summary>Fades back in and carries on from where it paused.</summary>
        public override void OnResume() => StartRunning();

        /// <summary>Puts every layer back at rest, hidden, with no interactions used.</summary>
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
        }

        /// <summary>
        /// Interaction scripts call this when the user uses them. The mural completes once
        /// <see cref="requiredInteractions"/> different ids have been reported.
        /// </summary>
        public void ReportInteraction(string id)
        {
            usedInteractions.Add(id);
            if (usedInteractions.Count >= requiredInteractions) Complete();
        }

        void StartRunning()
        {
            IsRunning = true;
            FadeTo(1f);
            motionSet.PlayAll();
            if (parallax != null) parallax.Active = true;
            StopDust();
            if (dustPrefab != null && dustSources.Length > 0) dust = StartCoroutine(DriftDust());
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
