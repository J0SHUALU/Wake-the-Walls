using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Test experience that builds a mural's layered rig at real size and fades it in, with no motion.
    /// Use it on a phone at the real wall to check that every layer sits exactly on the paint
    /// before building the full experience on top of the rig.
    /// </summary>
    public class RigTestExperience : MuralExperience
    {
        [SerializeField] LayeredMuralRig rig;
        [SerializeField, Range(0.6f, 1.5f)] float fadeDuration = 1f;

        Coroutine fade;
        float alpha;

        protected override void OnInitialized()
        {
            rig.Build(Data.PhysicalSizeMeters);
            SetAlpha(0f);
        }

        /// <summary>Fades the layers in over the wall.</summary>
        public override void OnFound() => FadeTo(1f);

        /// <summary>Fades the layers out while the mural is out of view.</summary>
        public override void OnLost() => FadeTo(0f);

        /// <summary>Fades the layers back in.</summary>
        public override void OnResume() => FadeTo(1f);

        /// <summary>Hides the layers so OnFound can play again.</summary>
        public override void ResetExperience()
        {
            base.ResetExperience();
            if (fade != null) StopCoroutine(fade);
            SetAlpha(0f);
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
