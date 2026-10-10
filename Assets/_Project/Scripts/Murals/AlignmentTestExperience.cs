using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Test experience that lays a see-through copy of the mural photo over the painting at real size,
    /// with an outline and a centre cross. Use it to check that position and scale line up before
    /// building the real experience. Tapping the overlay logs the tap and gives a small pulse.
    /// </summary>
    public class AlignmentTestExperience : MuralExperience
    {
        static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] Texture overlayTexture;
        [Tooltip("Transparent unlit material used for the photo overlay.")]
        [SerializeField] Material overlayMaterial;
        [Tooltip("Unlit material used for the outline and centre cross.")]
        [SerializeField] Material lineMaterial;
        [SerializeField, Range(0f, 1f)] float overlayAlpha = 0.6f;
        [SerializeField] Color lineColor = Color.yellow;

        Renderer overlay;
        MaterialPropertyBlock block;
        Coroutine fade;
        float alpha;

        protected override void OnInitialized()
        {
            Vector2 size = Data.PhysicalSizeMeters;

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Overlay";
            quad.transform.SetParent(transform, false);
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            overlay = quad.GetComponent<Renderer>();
            overlay.sharedMaterial = overlayMaterial;
            block = new MaterialPropertyBlock();
            SetAlpha(0f);

            quad.AddComponent<Interactable>().Tapped.AddListener(OnOverlayTapped);

            float half = 0.5f;
            float width = Mathf.Max(size.x, size.y) * 0.006f;
            CreateLine("Outline", true, width, new[]
            {
                new Vector3(-size.x * half, -size.y * half), new Vector3(size.x * half, -size.y * half),
                new Vector3(size.x * half, size.y * half), new Vector3(-size.x * half, size.y * half)
            });
            float arm = Mathf.Min(size.x, size.y) * 0.05f;
            CreateLine("CentreH", false, width, new[] { new Vector3(-arm, 0f), new Vector3(arm, 0f) });
            CreateLine("CentreV", false, width, new[] { new Vector3(0f, -arm), new Vector3(0f, arm) });
        }

        /// <summary>Fades the overlay in.</summary>
        public override void OnFound() => FadeTo(overlayAlpha);

        /// <summary>Fades the overlay out while the mural is out of view.</summary>
        public override void OnLost() => FadeTo(0f);

        /// <summary>Fades the overlay back in.</summary>
        public override void OnResume() => FadeTo(overlayAlpha);

        /// <summary>Hides the overlay so OnFound can play again.</summary>
        public override void ResetExperience()
        {
            base.ResetExperience();
            if (fade != null) StopCoroutine(fade);
            SetAlpha(0f);
        }

        void OnOverlayTapped()
        {
            Debug.Log($"Tapped alignment overlay on {Data.ReferenceImageName}.");
            StartCoroutine(Pulse(overlay.transform));
        }

        IEnumerator Pulse(Transform target)
        {
            Vector3 start = target.localScale;
            yield return Tween.Scale(target, start * 1.03f, 0.15f, Tween.EaseOutCubic);
            yield return Tween.Scale(target, start, 0.3f, Tween.EaseInOutCubic);
        }

        void FadeTo(float target)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Tween.Value(alpha, target, SetAlpha));
        }

        void SetAlpha(float value)
        {
            alpha = value;
            overlay.GetPropertyBlock(block);
            if (overlayTexture != null) block.SetTexture(BaseMap, overlayTexture);
            block.SetColor(BaseColor, new Color(1f, 1f, 1f, value));
            overlay.SetPropertyBlock(block);
        }

        void CreateLine(string lineName, bool loop, float width, Vector3[] points)
        {
            var line = new GameObject(lineName).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.useWorldSpace = false;
            line.loop = loop;
            line.widthMultiplier = width;
            line.sharedMaterial = lineMaterial;
            line.startColor = line.endColor = lineColor;
            // Sit just in front of the overlay so the lines are not hidden by it.
            for (int i = 0; i < points.Length; i++) points[i].z = -0.002f;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
    }
}
