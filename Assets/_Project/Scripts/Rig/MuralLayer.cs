using UnityEngine;

namespace WakeTheWalls.Rig
{
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

        public LayerEntry Entry { get; private set; }

        public string Element => Entry != null ? Entry.element : string.Empty;

        public Transform Pivot { get; private set; }

        public Vector2 PivotLocal { get; private set; }

        public float RestDepth => Entry != null ? Entry.depthMm / 1000f : 0f;

        public Renderer Renderer => quadRenderer;

        public float Alpha => alpha;

        public float Dissolve => dissolve;

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

        public void SetAlpha(float value)
        {
            alpha = Mathf.Clamp01(value);
            Apply();
        }

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
