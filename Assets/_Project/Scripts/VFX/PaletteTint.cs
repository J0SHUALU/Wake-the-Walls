using UnityEngine;

namespace WakeTheWalls.VFX
{
    /// <summary>
    /// Tints a renderer with one colour from a mural's palette, so 3D content and effects
    /// use the painting's own colours. Works with any material that has a _BaseColor property.
    /// </summary>
    public class PaletteTint : MonoBehaviour
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [Tooltip("Renderers to tint. Uses the renderers on this GameObject and its children when empty.")]
        [SerializeField] Renderer[] targets = new Renderer[0];

        [Tooltip("Which palette colour to use. Wraps round if the palette is shorter.")]
        [SerializeField, Min(0)] int paletteIndex;

        MaterialPropertyBlock block;

        /// <summary>Colour currently applied.</summary>
        public Color Current { get; private set; } = Color.white;

        /// <summary>Applies the colour at <see cref="paletteIndex"/> from the given palette.</summary>
        /// <param name="palette">Usually MuralData.Palette.</param>
        public void Apply(Color[] palette)
        {
            if (palette == null || palette.Length == 0) return;
            Apply(palette[paletteIndex % palette.Length]);
        }

        /// <summary>Applies a specific colour.</summary>
        public void Apply(Color color)
        {
            Current = color;
            block ??= new MaterialPropertyBlock();
            Renderer[] list = targets.Length > 0 ? targets : GetComponentsInChildren<Renderer>();
            foreach (Renderer target in list)
            {
                if (target == null) continue;
                target.GetPropertyBlock(block);
                block.SetColor(BaseColor, color);
                target.SetPropertyBlock(block);
            }
        }

        /// <summary>Sets which palette colour <see cref="Apply(Color[])"/> picks.</summary>
        public void SetIndex(int index) => paletteIndex = Mathf.Max(0, index);
    }
}
