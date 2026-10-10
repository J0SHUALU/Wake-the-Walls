using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Mural 4: the gold lines carry the story. After the shared fade in, the gold lines start to glow,
    /// then each portrait lifts off the wall in turn while light flows out from it along the lines.
    /// Tapping a portrait lifts it again. Once every portrait has been opened, the mask's eyes light up
    /// and the mural completes. Portraits only ever move as whole layers.
    /// </summary>
    public class Mural04Experience : LayeredMuralExperience
    {
        [Header("Gold lines")]
        [SerializeField] MaskGlow goldLines;
        [Tooltip("Steady glow on the lines while the mural is running, 0 to 1.")]
        [SerializeField, Range(0f, 1f)] float restingGlow = 0.25f;
        [SerializeField, Range(0.8f, 4f)] float flowSeconds = 2f;

        [Header("Portraits")]
        [Tooltip("Portrait elements, in the order they lift during the intro.")]
        [SerializeField] string[] portraits = new string[0];
        [Tooltip("How far a portrait lifts off the wall, in metres.")]
        [SerializeField, Range(0.02f, LayerMotion.MaxPeelMeters)] float peelDistance = 0.12f;
        [SerializeField, Min(0.2f)] float holdSeconds = 1.4f;
        [SerializeField, Min(0f)] float gapSeconds = 0.4f;

        [Header("Mask eyes")]
        [SerializeField] MaskGlow maskEyes;
        [Tooltip("Seconds the eyes take to light up before the mural completes.")]
        [SerializeField, Range(0.5f, 4f)] float eyesSeconds = 2f;

        readonly HashSet<string> opened = new HashSet<string>();
        Coroutine intro, hold, wake;
        LayerMotion lifted;
        int nextPortrait;
        bool introDone, eyesAwake;

        /// <summary>The gold line glow, for interaction scripts on this prefab.</summary>
        public MaskGlow GoldLines => goldLines;

        /// <summary>Portrait elements on this mural, for interaction scripts on this prefab.</summary>
        public string[] Portraits => portraits;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (goldLines != null) goldLines.Build(Data.PhysicalSizeMeters);
            if (maskEyes != null) maskEyes.Build(Data.PhysicalSizeMeters);
        }

        protected override void OnStarted(bool resumed)
        {
            StopIntro();
            if (goldLines != null)
            {
                goldLines.FadeAlpha(1f, FadeDuration);
                goldLines.FadeIntensity(restingGlow, FadeDuration);
            }
            if (eyesAwake && maskEyes != null) maskEyes.FadeAlpha(1f, FadeDuration);
            if (!introDone) intro = StartCoroutine(Intro());
        }

        protected override void OnStopped()
        {
            StopIntro();
            if (goldLines != null) goldLines.FadeAlpha(0f, FadeDuration);
            if (maskEyes != null) maskEyes.FadeAlpha(0f, FadeDuration);
        }

        protected override void OnResetState()
        {
            StopIntro();
            if (wake != null) StopCoroutine(wake);
            wake = null;
            opened.Clear();
            nextPortrait = 0;
            introDone = eyesAwake = false;
            if (goldLines != null) goldLines.ResetGlow();
            if (maskEyes != null) maskEyes.ResetGlow();
        }

        /// <summary>Lifts a portrait when it is tapped, ending the intro. Returns false if it cannot.</summary>
        public bool OpenPortrait(string element)
        {
            if (!IsRunning || !MotionSet.TryGetMotion(element, out LayerMotion motion)) return false;
            StopIntro();
            introDone = true;
            hold = StartCoroutine(Hold(motion));
            if (opened.Add(element) && opened.Count >= portraits.Length && !eyesAwake) wake = StartCoroutine(WakeEyes());
            return true;
        }

        /// <summary>Sends light along the gold lines from a point, 0 to 1 across the mural.</summary>
        public void FlowFrom(Vector2 pointOnMural)
        {
            if (goldLines != null && IsRunning) goldLines.Flow(pointOnMural, flowSeconds);
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(FadeDuration);
            for (; nextPortrait < portraits.Length; nextPortrait++)
            {
                if (!MotionSet.TryGetMotion(portraits[nextPortrait], out LayerMotion motion)) continue;
                yield return Hold(motion);
                yield return new WaitForSeconds(gapSeconds);
            }
            introDone = true;
            intro = null;
        }

        // Lifts one portrait, sends light out from it, holds, then settles it back.
        IEnumerator Hold(LayerMotion motion)
        {
            lifted = motion;
            motion.Peel(peelDistance);
            MuralLayer layer = motion.Layer;
            if (layer.Entry != null) FlowFrom(new Vector2(layer.Entry.pivotX, layer.Entry.pivotY));
            yield return new WaitForSeconds(holdSeconds);
            motion.Return();
            lifted = null;
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

        void StopIntro()
        {
            if (intro != null) StopCoroutine(intro);
            if (hold != null) StopCoroutine(hold);
            intro = hold = null;
            if (lifted != null) lifted.Return();
            lifted = null;
        }
    }
}
