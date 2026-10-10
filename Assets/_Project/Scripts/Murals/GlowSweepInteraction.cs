using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// A tap area that sends a band of light across one layer, like beads lighting up one after another.
    /// It lays a copy of the layer on top with the brush dissolve, and the glowing edge of the
    /// dissolve is the band of light. When the sweep has passed, the copy is gone and the paint is untouched.
    /// </summary>
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class GlowSweepInteraction : MonoBehaviour
    {
        static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int Dissolve = Shader.PropertyToID("_Dissolve");

        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
        [Tooltip("The layer the light travels across.")]
        [SerializeField] string element;
        [Tooltip("BrushDissolve material. A copy is made with the glow settings below.")]
        [SerializeField] Material brushMaterial;
        [SerializeField] Color glowColour = new Color(1f, 0.86f, 0.45f, 1f);
        [SerializeField, Range(0.02f, 0.3f)] float glowWidth = 0.08f;
        [Tooltip("Direction the light travels, in degrees (0 is left to right).")]
        [SerializeField] float direction;
        [SerializeField, Range(0.6f, 3f)] float sweepSeconds = 1.6f;
        [SerializeField] AudioClip sound;

        [SerializeField] string interactionId = "glow";
        [SerializeField] UnityEvent<string> used = new UnityEvent<string>();

        MeshRenderer glow;
        Material glowMaterial;
        Coroutine sweep;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnDestroy()
        {
            if (glowMaterial != null) Destroy(glowMaterial);
        }

        void OnTapped()
        {
            if (!experience.IsRunning || !experience.Rig.TryGetLayer(element, out MuralLayer layer)) return;
            if (glow == null) CreateGlow(layer);
            if (sweep != null) StopCoroutine(sweep);
            sweep = StartCoroutine(Sweep());
            if (sound != null) AudioSource.PlayClipAtPoint(sound, transform.position);
            used.Invoke(interactionId);
        }

        IEnumerator Sweep()
        {
            // The brush mask runs across the whole canvas, so only sweep the part that covers the tap area.
            Vector2 range = SweepRange();
            glowMaterial.SetFloat(Dissolve, range.x);
            glow.enabled = true;
            yield return Tween.Value(range.x, range.y, v => glowMaterial.SetFloat(Dissolve, v), sweepSeconds, Tween.EaseInOutCubic);
            glow.enabled = false;
            sweep = null;
        }

        Vector2 SweepRange()
        {
            float angle = direction * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float min = float.MaxValue, max = float.MinValue;
            foreach (Vector2 corner in new[] { area.min, area.max, new Vector2(area.xMin, area.yMax), new Vector2(area.xMax, area.yMin) })
            {
                float along = Vector2.Dot(corner - new Vector2(0.5f, 0.5f), dir) + 0.5f;
                min = Mathf.Min(min, along);
                max = Mathf.Max(max, along);
            }
            return new Vector2(min - glowWidth * 2f, max + glowWidth * 2f);
        }

        // A copy of the layer's quad, a hair in front, drawn with a glowing brush edge.
        void CreateGlow(MuralLayer layer)
        {
            var block = new MaterialPropertyBlock();
            layer.Renderer.GetPropertyBlock(block);

            glowMaterial = new Material(brushMaterial) { name = "GlowSweep" };
            glowMaterial.SetTexture(BaseMap, block.GetTexture(BaseMap));
            glowMaterial.SetColor("_EdgeColor", glowColour);
            glowMaterial.SetFloat("_EdgeSoftness", glowWidth);
            glowMaterial.SetFloat("_Sweep", 1f);
            glowMaterial.SetFloat("_StrokeAngle", direction);

            var go = new GameObject("Glow");
            go.transform.SetParent(layer.Renderer.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, -0.0005f);
            go.AddComponent<MeshFilter>().sharedMesh = layer.Renderer.GetComponent<MeshFilter>().sharedMesh;
            glow = go.AddComponent<MeshRenderer>();
            glow.sharedMaterial = glowMaterial;
            glow.sortingOrder = layer.Renderer.sortingOrder + 1;
            glow.enabled = false;
        }
    }
}
