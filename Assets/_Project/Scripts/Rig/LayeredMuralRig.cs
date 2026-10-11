using System.Collections.Generic;
using UnityEngine;

namespace WakeTheWalls.Rig
{
    public class LayeredMuralRig : MonoBehaviour
    {
        [Tooltip("The mural's muralN_layers.json file.")]
        [SerializeField] TextAsset manifest;

        [Tooltip("The filled background and every layer texture. Matched by file name.")]
        [SerializeField] List<Texture> textures = new List<Texture>();

        [Tooltip("Shared material that uses the WakeTheWalls/MuralLayer shader.")]
        [SerializeField] Material layerMaterial;

        readonly Dictionary<string, MuralLayer> byElement = new Dictionary<string, MuralLayer>();
        readonly List<MuralLayer> layers = new List<MuralLayer>();

        public MuralLayer Background { get; private set; }

        public IReadOnlyList<MuralLayer> Layers => layers;

        public Vector2 MuralSize { get; private set; }

        public bool IsBuilt => Background != null;

        public void Build(Vector2 muralSize)
        {
            Clear();
            MuralSize = muralSize;

            if (manifest == null || layerMaterial == null)
            {
                Debug.LogWarning($"{name}: the rig needs a manifest and a layer material.", this);
                return;
            }

            LayerManifest data = LayerManifest.FromJson(manifest);

            var backgroundEntry = new LayerEntry { file = data.background, element = "background", depthMm = 0f, order = 0 };
            Background = CreateLayer(backgroundEntry, muralSize);

            foreach (LayerEntry entry in data.layers)
            {
                MuralLayer layer = CreateLayer(entry, muralSize);
                if (layer == null) continue;
                layers.Add(layer);
                byElement[entry.element] = layer;
            }
        }

        public bool TryGetLayer(string element, out MuralLayer layer)
        {
            return byElement.TryGetValue(element, out layer);
        }

        public void SetAlpha(float value)
        {
            if (Background != null) Background.SetAlpha(value);
            foreach (MuralLayer layer in layers) layer.SetAlpha(value);
        }

        MuralLayer CreateLayer(LayerEntry entry, Vector2 muralSize)
        {
            Texture texture = FindTexture(entry.file);
            if (texture == null)
            {
                Debug.LogWarning($"{name}: no texture named {entry.file} in the rig's texture list.", this);
                return null;
            }

            var go = new GameObject(entry.element);
            go.transform.SetParent(transform, false);
            var layer = go.AddComponent<MuralLayer>();
            layer.Build(entry, texture, muralSize, layerMaterial);
            return layer;
        }

        Texture FindTexture(string file)
        {
            foreach (Texture texture in textures)
                if (texture != null && texture.name == file) return texture;
            return null;
        }

        void Clear()
        {
            byElement.Clear();
            layers.Clear();
            Background = null;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }
    }
}
