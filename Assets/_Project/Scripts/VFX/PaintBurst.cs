using UnityEngine;

namespace WakeTheWalls.VFX
{
    /// <summary>
    /// Plays a one-shot paint particle effect (paint dust, paint drips) in a mural's own colours.
    /// Put it on the root of an effect prefab next to its ParticleSystem.
    /// </summary>
    /// <remarks>
    /// Each particle picks a random colour from the palette passed to <see cref="Play"/>.
    /// Use <see cref="Spawn"/> to create a copy that removes itself when it has finished.
    /// </remarks>
    [RequireComponent(typeof(ParticleSystem))]
    public class PaintBurst : MonoBehaviour
    {
        [Tooltip("How many palette colours to use, starting from the first. 0 uses them all.")]
        [SerializeField, Min(0)] int colourCount;

        ParticleSystem particles;

        ParticleSystem Particles => particles != null ? particles : particles = GetComponent<ParticleSystem>();

        /// <summary>
        /// Creates a copy of an effect prefab at a position, tints it and plays it.
        /// The copy destroys itself once every particle has died.
        /// </summary>
        /// <param name="prefab">PaintDust, PaintDrip or another prefab with a PaintBurst.</param>
        /// <param name="parent">Usually the experience root, so the effect moves with the mural.</param>
        /// <param name="localPosition">Where to play it, in the parent's space (metres).</param>
        /// <param name="palette">Usually MuralData.Palette. Leave null to keep the prefab's colours.</param>
        public static PaintBurst Spawn(PaintBurst prefab, Transform parent, Vector3 localPosition, Color[] palette)
        {
            PaintBurst burst = Instantiate(prefab, parent);
            burst.transform.localPosition = localPosition;
            var main = burst.Particles.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            burst.Play(palette);
            return burst;
        }

        /// <summary>Tints the particles from the palette and plays the effect once.</summary>
        /// <param name="palette">Colours to pick from. Null or empty keeps the prefab's own colours.</param>
        public void Play(Color[] palette)
        {
            if (palette != null && palette.Length > 0)
            {
                var main = Particles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(BuildGradient(palette))
                {
                    mode = ParticleSystemGradientMode.RandomColor
                };
            }
            Particles.Clear(true);
            Particles.Play(true);
        }

        // RandomColor picks a point along the gradient, so hard steps give one palette colour per particle.
        Gradient BuildGradient(Color[] palette)
        {
            int count = colourCount > 0 ? Mathf.Min(colourCount, palette.Length) : palette.Length;
            count = Mathf.Min(count, 4); // Unity gradients hold 8 keys; two per colour makes hard steps
            var colours = new GradientColorKey[count * 2];
            for (int i = 0; i < count; i++)
            {
                Color c = palette[i];
                colours[i * 2] = new GradientColorKey(c, i / (float)count);
                colours[i * 2 + 1] = new GradientColorKey(c, (i + 1) / (float)count - 0.001f);
            }
            var gradient = new Gradient { mode = GradientMode.Fixed };
            gradient.SetKeys(colours, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }
    }
}
