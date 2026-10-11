using System;
using UnityEngine;

namespace WakeTheWalls.Rig
{
    [Serializable]
    public class LayerManifest
    {
        public string mural;

        public int canvasWidth;
        public int canvasHeight;

        public string background;

        public LayerEntry[] layers = new LayerEntry[0];

        public static LayerManifest FromJson(TextAsset json)
        {
            return JsonUtility.FromJson<LayerManifest>(json.text);
        }
    }

    [Serializable]
    public class LayerEntry
    {
        public string file;

        public string element;

        public float depthMm = 3f;

        public int order;

        public float pivotX = 0.5f;
        public float pivotY = 0.5f;
    }
}
