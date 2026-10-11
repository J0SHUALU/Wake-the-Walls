using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
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

        public override void OnFound() => FadeTo(1f);

        public override void OnLost() => FadeTo(0f);

        public override void OnResume() => FadeTo(1f);

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
