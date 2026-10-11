using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.VFX
{
    /// <summary>
    /// Keeps a <see cref="MaskGlow"/> slowly breathing while its mural is in view, for lit windows,
    /// a glowing mask or a warm sky. Fades the glow in when the mural is found and out when it is lost.
    /// </summary>
    public class GlowPulse : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] MaskGlow glow;
        [SerializeField] float startDelay = 2f;
        [SerializeField, Range(0f, 1f)] float low = 0.15f;
        [SerializeField, Range(0f, 1f)] float high = 0.8f;
        [SerializeField] float breathSeconds = 3.5f;

        void OnEnable()
        {
            GameEvents.MuralFound += HandleFound;
            GameEvents.MuralLost += HandleLost;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleFound;
            GameEvents.MuralLost -= HandleLost;
        }

        void HandleFound(MuralData mural)
        {
            if (experience == null || glow == null || experience.Data != mural) return;
            CancelInvoke();
            Invoke(nameof(Begin), startDelay);
        }

        void HandleLost(MuralData mural)
        {
            if (experience == null || glow == null || experience.Data != mural) return;
            CancelInvoke();
            glow.FadeAlpha(0f, 0.5f);
        }

        void Begin()
        {
            if (!experience.IsRunning) return;
            glow.Build(experience.Rig.MuralSize);
            glow.FadeAlpha(1f, 1f);
            glow.Breathe(low, high, breathSeconds, Random.value);
        }
    }
}
