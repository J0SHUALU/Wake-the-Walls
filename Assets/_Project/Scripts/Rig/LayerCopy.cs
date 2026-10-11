using UnityEngine;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// Makes a loose copy of a mural layer that animations can move freely without touching the
    /// painted original. The copy starts hidden and draws in front of the mural.
    /// </summary>
    public static class LayerCopy
    {
        static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        /// <summary>Builds a hidden copy of <paramref name="source"/> next to it.</summary>
        /// <param name="source">The layer to copy.</param>
        /// <param name="muralSize">Real size of the mural in metres.</param>
        /// <param name="orderBoost">How far in front of the source the copy is drawn.</param>
        public static MuralLayer Make(MuralLayer source, Vector2 muralSize, int orderBoost = 50)
        {
            var block = new MaterialPropertyBlock();
            source.Renderer.GetPropertyBlock(block);
            var copy = new GameObject(source.Element + "_copy").AddComponent<MuralLayer>();
            copy.transform.SetParent(source.transform.parent, false);
            copy.Build(source.Entry, block.GetTexture(BaseMap), muralSize, source.Renderer.sharedMaterial);
            copy.Renderer.sortingOrder = source.Entry.order + orderBoost;
            copy.SetAlpha(0f);
            return copy;
        }
    }
}
