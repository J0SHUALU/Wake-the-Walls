using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.VFX
{
    /// <summary>
    /// Makes part of a mural glow using one of its greyscale light masks (for example the gold lines).
    /// It lays a quad over the whole mural just off the wall and draws light only where the mask is white.
    /// The light can glow steadily, or flow outwards from a point like water running along channels.
    /// </summary>
    public class MaskGlow : MonoBehaviour
    {
        static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int OriginId = Shader.PropertyToID("_Origin");
        static readonly int BandId = Shader.PropertyToID("_BandCenter");
        static readonly int AspectId = Shader.PropertyToID("_Aspect");
        static readonly int MaskId = Shader.PropertyToID("_MaskMap");
        static readonly int ColourId = Shader.PropertyToID("_GlowColor");

        [Tooltip("Greyscale light mask on the full mural canvas, from Art/Murals/MuralN.")]
        [SerializeField] Texture mask;
        [Tooltip("Material using the MaskGlow shader. A copy is made at runtime.")]
        [SerializeField] Material glowMaterial;
        [SerializeField] Color colour = new Color(1f, 0.82f, 0.4f, 1f);
        [Tooltip("Height above the wall in millimetres. Keep it under the lowest moving layer.")]
        [SerializeField] float depthMm = 1.5f;
        [Tooltip("Draw order among the rig's layers. 0 sits just above the background.")]
        [SerializeField] int sortingOrder;

        Material material;
        Coroutine alphaTween, intensityTween, flowTween;
        float alpha, intensity;

        /// <summary>True while a flow is travelling across the mask.</summary>
        public bool IsFlowing => flowTween != null;

        /// <summary>Builds the glow quad at the mural's real size. Call once, before using it.</summary>
        public void Build(Vector2 muralSize)
        {
            if (material != null) return;
            material = new Material(glowMaterial);
            material.SetTexture(MaskId, mask);
            material.SetColor(ColourId, colour);
            material.SetFloat(AspectId, muralSize.x / Mathf.Max(0.01f, muralSize.y));

            var quad = new GameObject("GlowQuad");
            quad.transform.SetParent(transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, -depthMm / 1000f);
            quad.transform.localScale = new Vector3(muralSize.x, muralSize.y, 1f);
            quad.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            var quadRenderer = quad.AddComponent<MeshRenderer>();
            quadRenderer.sharedMaterial = material;
            quadRenderer.sortingOrder = sortingOrder;
            quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            quadRenderer.receiveShadows = false;
            ResetGlow();
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
        }

        /// <summary>Fades the whole glow in or out, for the mural's own fade.</summary>
        public void FadeAlpha(float target, float seconds) =>
            Restart(ref alphaTween, Tween.Value(alpha, target, v => Set(AlphaId, alpha = v), seconds));

        /// <summary>Fades the steady glow across the whole mask to a level from 0 to 1.</summary>
        public void FadeIntensity(float target, float seconds) =>
            Restart(ref intensityTween, Tween.Value(intensity, target, v => Set(IntensityId, intensity = v), seconds));

        /// <summary>Keeps the steady glow breathing between two levels until another fade or a reset.</summary>
        /// <param name="phase">0 to 1, so several glows can breathe out of step.</param>
        public void Breathe(float low, float high, float seconds, float phase = 0f) =>
            Restart(ref intensityTween, BreatheRoutine(low, high, Mathf.Max(0.1f, seconds), phase));

        /// <summary>Sends light outwards from a point (0 to 1 across the mural) until it has crossed the mural.</summary>
        public void Flow(Vector2 originOnMural, float seconds)
        {
            if (material == null) return;
            material.SetVector(OriginId, originOnMural);
            Restart(ref flowTween, FlowRoutine(seconds));
        }

        /// <summary>Stops at once and turns the light off. Use when the mural is hidden.</summary>
        public void ResetGlow()
        {
            Stop(ref alphaTween);
            Stop(ref intensityTween);
            Stop(ref flowTween);
            Set(AlphaId, alpha = 0f);
            Set(IntensityId, intensity = 0f);
            Set(BandId, -1f);
        }

        IEnumerator FlowRoutine(float seconds)
        {
            // 1.3 canvas widths is enough to reach every corner from any starting point.
            yield return Tween.Value(0f, 1.3f, v => Set(BandId, v), seconds, Tween.EaseInOutCubic);
            Set(BandId, -1f);
            flowTween = null;
        }

        IEnumerator BreatheRoutine(float low, float high, float seconds, float phase)
        {
            for (float t = phase * seconds; ; t += Time.deltaTime)
            {
                intensity = Mathf.Lerp(low, high, 0.5f - 0.5f * Mathf.Cos(t / seconds * Mathf.PI * 2f));
                Set(IntensityId, intensity);
                yield return null;
            }
        }

        void Set(int id, float value)
        {
            if (material != null) material.SetFloat(id, value);
        }

        void Restart(ref Coroutine slot, IEnumerator routine)
        {
            Stop(ref slot);
            if (material != null && isActiveAndEnabled) slot = StartCoroutine(routine);
        }

        void Stop(ref Coroutine slot)
        {
            if (slot != null) StopCoroutine(slot);
            slot = null;
        }
    }
}
