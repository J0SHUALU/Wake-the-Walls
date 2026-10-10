using System.Collections;
using UnityEngine;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Mural 4: the gold lines carry the story. After the shared fade in, the gold lines start to glow,
    /// then each portrait lifts off the wall in turn while light flows out from it along the lines.
    /// Portraits only ever move as whole layers.
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

        Coroutine intro;
        LayerMotion lifted;
        int nextPortrait;
        bool introDone;

        /// <summary>The gold line glow, for interaction scripts on this prefab.</summary>
        public MaskGlow GoldLines => goldLines;

        /// <summary>Portrait elements on this mural, for interaction scripts on this prefab.</summary>
        public string[] Portraits => portraits;

        /// <summary>How far a portrait lifts off the wall, for interaction scripts on this prefab.</summary>
        public float PeelDistance => peelDistance;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (goldLines != null) goldLines.Build(Data.PhysicalSizeMeters);
        }

        protected override void OnStarted(bool resumed)
        {
            StopIntro();
            if (goldLines != null)
            {
                goldLines.FadeAlpha(1f, FadeDuration);
                goldLines.FadeIntensity(restingGlow, FadeDuration);
            }
            if (!introDone) intro = StartCoroutine(Intro());
        }

        protected override void OnStopped()
        {
            StopIntro();
            if (goldLines != null) goldLines.FadeAlpha(0f, FadeDuration);
        }

        protected override void OnResetState()
        {
            StopIntro();
            nextPortrait = 0;
            introDone = false;
            if (goldLines != null) goldLines.ResetGlow();
        }

        /// <summary>Sends light along the gold lines from a portrait's pivot.</summary>
        public void FlowFrom(MuralLayer layer)
        {
            if (goldLines == null || layer == null || layer.Entry == null) return;
            goldLines.Flow(new Vector2(layer.Entry.pivotX, layer.Entry.pivotY), flowSeconds);
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(FadeDuration);
            for (; nextPortrait < portraits.Length; nextPortrait++)
            {
                if (!MotionSet.TryGetMotion(portraits[nextPortrait], out LayerMotion motion)) continue;
                lifted = motion;
                motion.Peel(peelDistance);
                FlowFrom(motion.Layer);
                yield return new WaitForSeconds(holdSeconds);
                motion.Return();
                lifted = null;
                yield return new WaitForSeconds(gapSeconds);
            }
            introDone = true;
            intro = null;
        }

        void StopIntro()
        {
            if (intro != null) StopCoroutine(intro);
            intro = null;
            if (lifted != null) lifted.Return();
            lifted = null;
        }
    }
}
