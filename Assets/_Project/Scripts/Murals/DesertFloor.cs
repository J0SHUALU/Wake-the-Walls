using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
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

        public bool IsOut { get; private set; }

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

        public void SpreadOut()
        {
            IsOut = true;
            Animate(spreadEnd, spreadSeconds);
        }

        public void PullBack(float seconds = 0.8f)
        {
            IsOut = false;
            Animate(1f, seconds);
        }

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
