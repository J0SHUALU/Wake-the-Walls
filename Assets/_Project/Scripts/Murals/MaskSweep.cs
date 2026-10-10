using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Sends a band of light across a painted detail of the mural, such as gold rings or beads.
    /// The detail is picked out by a greyscale mask on the full mural canvas, so the light only
    /// shows on that paint and the paint itself is never covered.
    /// </summary>
    /// <remarks>
    /// Put it on the experience prefab's root, which sits at the centre of the mural. Call
    /// <see cref="Play"/> from an interaction's Used event. Set <see cref="next"/> to chain a
    /// second glow that starts when this one has passed, for light that travels on into another detail.
    /// </remarks>
    public class MaskSweep : MonoBehaviour
    {
        static readonly int MaskMap = Shader.PropertyToID("_MaskMap");
        static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
        static readonly int Position = Shader.PropertyToID("_Position");
        static readonly int Width = Shader.PropertyToID("_Width");
        static readonly int Angle = Shader.PropertyToID("_Angle");

        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Greyscale mask on the full mural canvas, white where the light shows (an M file from the art folder).")]
        [SerializeField] Texture mask;
        [Tooltip("Material using the WakeTheWalls/MaskSweep shader.")]
        [SerializeField] Material glowMaterial;
        [SerializeField, ColorUsage(true, true)] Color colour = new Color(1f, 0.86f, 0.45f, 1f);
        [SerializeField, Range(0.01f, 0.3f)] float width = 0.06f;
        [Tooltip("Direction the light travels, in degrees (0 left to right, -90 top to bottom).")]
        [SerializeField] float direction = -90f;
        [Tooltip("Part of the mural the masked detail covers, 0 to 1 (0,0 bottom left). The light only travels across this part.")]
        [SerializeField] Rect region = new Rect(0f, 0f, 1f, 1f);
        [SerializeField, Range(0.6f, 3f)] float seconds = 1.2f;
        [Tooltip("Distance in front of the wall, in millimetres.")]
        [SerializeField, Range(0f, 10f)] float depthMm = 0.5f;

        [Header("Chain")]
        [Tooltip("Optional glow that starts once this one has passed.")]
        [SerializeField] MaskSweep next;
        [SerializeField, Min(0f)] float nextDelay;

        MeshRenderer glow;
        Material material;
        Coroutine sweep;

        /// <summary>Sends the light across the detail, then on to <see cref="next"/> if set.</summary>
        public void Play()
        {
            if (experience == null || !experience.IsRunning || mask == null || glowMaterial == null) return;
            if (glow == null) CreateGlow();
            if (sweep != null) StopCoroutine(sweep);
            sweep = StartCoroutine(Sweep());
        }

        void OnDisable()
        {
            if (sweep != null) StopCoroutine(sweep);
            sweep = null;
            if (glow != null) glow.enabled = false;
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
        }

        IEnumerator Sweep()
        {
            Vector2 range = SweepRange();
            glow.enabled = true;
            yield return Tween.Value(range.x, range.y, v =>
            {
                // Stop early if the mural is lost, so no light floats over an empty wall.
                material.SetFloat(Position, experience.IsRunning ? v : range.y);
            }, seconds, Tween.EaseInOutCubic);
            glow.enabled = false;
            sweep = null;

            if (next == null || !experience.IsRunning) yield break;
            if (nextDelay > 0f) yield return new WaitForSeconds(nextDelay);
            next.Play();
        }

        // Start and end of the band along the sweep direction, just outside the region so it enters and leaves cleanly.
        Vector2 SweepRange()
        {
            float angle = direction * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float min = float.MaxValue, max = float.MinValue;
            foreach (Vector2 corner in new[] { region.min, region.max, new Vector2(region.xMin, region.yMax), new Vector2(region.xMax, region.yMin) })
            {
                float along = Vector2.Dot(corner - new Vector2(0.5f, 0.5f), dir) + 0.5f;
                min = Mathf.Min(min, along);
                max = Mathf.Max(max, along);
            }
            return new Vector2(min - width * 1.5f, max + width * 1.5f);
        }

        // A quad the size of the whole mural, a hair in front of the wall, drawn on top of every layer.
        void CreateGlow()
        {
            material = new Material(glowMaterial) { name = "MaskSweep" };
            material.SetTexture(MaskMap, mask);
            material.SetColor(GlowColor, colour);
            material.SetFloat(Width, width);
            material.SetFloat(Angle, direction);

            Vector2 size = experience.Rig.MuralSize;
            var go = new GameObject("MaskSweep " + mask.name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, -depthMm / 1000f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.AddComponent<MeshFilter>().sharedMesh = experience.Rig.Background.Renderer.GetComponent<MeshFilter>().sharedMesh;
            glow = go.AddComponent<MeshRenderer>();
            glow.sharedMaterial = material;
            glow.sortingOrder = 1000;
            glow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            glow.receiveShadows = false;
            glow.enabled = false;
        }
    }
}
