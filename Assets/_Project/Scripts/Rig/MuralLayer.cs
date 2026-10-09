using UnityEngine;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// One flat layer of a mural: a quad the size of the whole mural showing one cut-out element,
    /// sitting a few millimetres in front of the wall.
    /// </summary>
    /// <remarks>
    /// Hierarchy built by <see cref="Build"/>:
    /// <code>
    /// MuralLayer (this)  at Z = -depthMm / 1000
    ///   Pivot            at the element's pivot point; rotate this to sway around the pivot
    ///     Quad           offset back by the pivot so the art stays exactly on the wall
    /// </code>
    /// Every layer uses the full mural canvas, so all quads share the same size and centre.
    /// </remarks>
    public class MuralLayer : MonoBehaviour
    {
        static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        static Mesh quadMesh;

        MeshRenderer quadRenderer;
        MaterialPropertyBlock block;
        Texture texture;
        float alpha = 1f;
        float dissolve;

        /// <summary>The manifest entry this layer was built from.</summary>
        public LayerEntry Entry { get; private set; }

        /// <summary>Short name of the element, for example "tree_left".</summary>
        public string Element => Entry != null ? Entry.element : string.Empty;

        /// <summary>Transform to rotate or scale so the element moves around its pivot.</summary>
        public Transform Pivot { get; private set; }

        /// <summary>Pivot position in this layer's local space, in metres from the mural centre.</summary>
        public Vector2 PivotLocal { get; private set; }

        /// <summary>Resting distance in front of the wall in metres (positive is towards the viewer).</summary>
        public float RestDepth => Entry != null ? Entry.depthMm / 1000f : 0f;

        /// <summary>Renderer of the layer quad, for colliders, sorting or effects.</summary>
        public Renderer Renderer => quadRenderer;

        /// <summary>Current opacity, 0 to 1.</summary>
        public float Alpha => alpha;

        /// <summary>Current brush dissolve amount, 0 to 1.</summary>
        public float Dissolve => dissolve;

        /// <summary>
        /// Builds the layer quad. Call once, right after adding the component.
        /// </summary>
        /// <param name="entry">The layer's line from the manifest.</param>
        /// <param name="layerTexture">The cut-out texture, drawn on the full mural canvas.</param>
        /// <param name="muralSize">Real size of the mural in metres.</param>
        /// <param name="material">Shared material using the MuralLayer shader.</param>
        public void Build(LayerEntry entry, Texture layerTexture, Vector2 muralSize, Material material)
        {
            Entry = entry;
            texture = layerTexture;
            block = new MaterialPropertyBlock();

            transform.localPosition = new Vector3(0f, 0f, -RestDepth);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            PivotLocal = new Vector2((entry.pivotX - 0.5f) * muralSize.x, (entry.pivotY - 0.5f) * muralSize.y);

            Pivot = new GameObject("Pivot").transform;
            Pivot.SetParent(transform, false);
            Pivot.localPosition = PivotLocal;

            var quad = new GameObject("Quad");
            quad.transform.SetParent(Pivot, false);
            quad.transform.localPosition = -PivotLocal;
            quad.transform.localScale = new Vector3(muralSize.x, muralSize.y, 1f);
            quad.AddComponent<MeshFilter>().sharedMesh = GetQuadMesh();

            quadRenderer = quad.AddComponent<MeshRenderer>();
            quadRenderer.sharedMaterial = material;
            quadRenderer.sortingOrder = entry.order;
            quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            quadRenderer.receiveShadows = false;

            Apply();
        }

        /// <summary>Sets the layer's opacity, 0 hidden to 1 fully visible.</summary>
        public void SetAlpha(float value)
        {
            alpha = Mathf.Clamp01(value);
            Apply();
        }

        /// <summary>Sets the brush dissolve amount used by the reveal effect.</summary>
        public void SetDissolve(float value)
        {
            dissolve = Mathf.Clamp01(value);
            Apply();
        }

        void Apply()
        {
            if (quadRenderer == null) return;
            quadRenderer.GetPropertyBlock(block);
            if (texture != null) block.SetTexture(BaseMap, texture);
            block.SetFloat(AlphaId, alpha);
            block.SetFloat(DissolveId, dissolve);
            quadRenderer.SetPropertyBlock(block);
        }

        // A 1 by 1 quad centred on the origin, facing -Z (towards the viewer), UV 0,0 at bottom left.
        static Mesh GetQuadMesh()
        {
            if (quadMesh != null) return quadMesh;
            quadMesh = new Mesh { name = "MuralLayerQuad" };
            quadMesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f)
            };
            quadMesh.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            quadMesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            quadMesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            quadMesh.RecalculateBounds();
            return quadMesh;
        }
    }
}
