using System;
using UnityEngine;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// The contents of a mural[N]_layers.json file exported with the art.
    /// See docs/layer-export-guide.md for the format.
    /// </summary>
    [Serializable]
    public class LayerManifest
    {
        /// <summary>Mural folder name, for example "Mural1".</summary>
        public string mural;

        /// <summary>Pixel size of the reference image. Every layer uses the same canvas.</summary>
        public int canvasWidth;
        public int canvasHeight;

        /// <summary>File name of the filled background, without extension.</summary>
        public string background;

        /// <summary>Cut-out layers, back to front.</summary>
        public LayerEntry[] layers = new LayerEntry[0];

        /// <summary>Reads a manifest from a JSON text asset.</summary>
        public static LayerManifest FromJson(TextAsset json)
        {
            return JsonUtility.FromJson<LayerManifest>(json.text);
        }
    }

    /// <summary>One cut-out element of a mural.</summary>
    [Serializable]
    public class LayerEntry
    {
        /// <summary>Texture file name without extension, for example "mural1_L01_bird".</summary>
        public string file;

        /// <summary>Short name of the element, for example "bird".</summary>
        public string element;

        /// <summary>Distance in front of the wall in millimetres.</summary>
        public float depthMm = 3f;

        /// <summary>Draw order. Higher numbers draw on top.</summary>
        public int order;

        /// <summary>Pivot for sway and rotation, 0 to 1 across the canvas (0,0 is bottom left).</summary>
        public float pivotX = 0.5f;
        public float pivotY = 0.5f;
    }
}
