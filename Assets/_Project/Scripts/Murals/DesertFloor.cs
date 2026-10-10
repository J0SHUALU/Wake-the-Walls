using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// A painted floor that spreads out from the bottom edge of a mural onto the ground in front of
    /// the viewer, brushed on with the brush dissolve so it looks like the painting is spilling out.
    /// </summary>
    /// <remarks>
    /// Uses the BrushDissolve material in a flat colour. The floor lies at the mural's bottom edge,
    /// a little wider than the mural, and reaches <see cref="depth"/> metres towards the viewer
    /// (the shared limit is 1.5 m).
    /// </remarks>
    public class DesertFloor : MonoBehaviour
    {
        static readonly int Dissolve = Shader.PropertyToID("_Dissolve");

        [SerializeField] Material brushMaterial;
        [SerializeField] Color colour = new Color(0.66f, 0.56f, 0.41f, 1f);
        [SerializeField, Range(0.2f, 1.5f)] float depth = 1.2f;
        [Tooltip("How much wider than the mural the floor is, as a fraction of its width.")]
        [SerializeField, Range(0f, 0.5f)] float extraWidth = 0.2f;
        [Tooltip("How far the brush gets before stopping. Lower leaves a ragged far edge.")]
        [SerializeField, Range(0f, 1f)] float spreadEnd = 0.2f;
        [SerializeField, Range(0.6f, 3f)] float spreadSeconds = 2.2f;

        Material material;
        Coroutine spread;
        float dissolve = 1f;

        /// <summary>True once the floor has started spreading out.</summary>
        public bool IsOut { get; private set; }

        /// <summary>Builds the floor along the bottom of a mural of this size. Call once.</summary>
        public void Build(Vector2 muralSize)
        {
            material = new Material(brushMaterial) { name = "DesertFloor" };
            material.SetColor("_BaseColor", colour);
            material.SetFloat("_Sweep", 0.7f);
            material.SetFloat("_StrokeAngle", -90f);
            material.SetVector("_StrokeScale", new Vector4(6f, 18f, 0f, 0f));

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<Collider>());
            quad.name = "Floor";
            quad.transform.SetParent(transform, false);
            // Lie flat, starting at the wall's bottom edge and reaching out towards the viewer (-Z).
            quad.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            quad.transform.localPosition = new Vector3(0f, -muralSize.y * 0.5f, -depth * 0.5f);
            quad.transform.localScale = new Vector3(muralSize.x * (1f + extraWidth), depth, 1f);
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            SetDissolve(1f);
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
        }

        /// <summary>Brushes the floor out from the wall.</summary>
        public void SpreadOut()
        {
            IsOut = true;
            Animate(spreadEnd, spreadSeconds);
        }

        /// <summary>Wipes the floor back into the wall.</summary>
        public void PullBack(float seconds = 0.8f)
        {
            IsOut = false;
            Animate(1f, seconds);
        }

        /// <summary>Hides the floor at once. Use only while the mural is out of view.</summary>
        public void Hide()
        {
            if (spread != null) StopCoroutine(spread);
            IsOut = false;
            SetDissolve(1f);
        }

        void Animate(float target, float seconds)
        {
            if (spread != null) StopCoroutine(spread);
            spread = StartCoroutine(Tween.Value(dissolve, target, SetDissolve, seconds, Tween.EaseInOutCubic));
        }

        void SetDissolve(float value)
        {
            dissolve = value;
            if (material != null) material.SetFloat(Dissolve, value);
        }
    }
}
