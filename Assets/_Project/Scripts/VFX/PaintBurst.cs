using UnityEngine;

namespace WakeTheWalls.VFX
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PaintBurst : MonoBehaviour
    {
        [Tooltip("How many palette colours to use, starting from the first. 0 uses them all.")]
        [SerializeField, Min(0)] int colourCount;

        ParticleSystem particles;

        ParticleSystem Particles => particles != null ? particles : particles = GetComponent<ParticleSystem>();

        public static PaintBurst Spawn(PaintBurst prefab, Transform parent, Vector3 localPosition, Color[] palette)
        {
            PaintBurst burst = Instantiate(prefab, parent);
            burst.transform.localPosition = localPosition;
            var main = burst.Particles.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            burst.Play(palette);
            return burst;
        }

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

        Gradient BuildGradient(Color[] palette)
        {
            int count = colourCount > 0 ? Mathf.Min(colourCount, palette.Length) : palette.Length;
            count = Mathf.Min(count, 4);
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
