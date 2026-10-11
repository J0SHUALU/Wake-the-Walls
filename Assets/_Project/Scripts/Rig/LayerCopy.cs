using UnityEngine;

namespace WakeTheWalls.Rig
{
    public static class LayerCopy
    {
        static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

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
