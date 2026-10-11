using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Makes chosen elements slowly swell and settle, with a slight turn, as if they were breathing,
    /// for example a flower crown opening and closing. Works on top of any other motion on the layer.
    /// Which elements breathe is set on the prefab, so the script holds no mural names.
    /// </summary>
    public class BreatheLayers : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] string[] elements = new string[0];
        [Tooltip("How much bigger at the top of a breath, 0.06 is 6 percent.")]
        [SerializeField, Range(0f, 0.3f)] float swell = 0.06f;
        [SerializeField, Range(0f, 10f)] float turnDegrees = 2f;
        [SerializeField] float breathSeconds = 4f;

        MuralLayer[] layers;
        float[] applied;
        float[] turned;

        void Update()
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            if (layers == null) Find();

            float wave = 0.5f - 0.5f * Mathf.Cos(Time.time / breathSeconds * Mathf.PI * 2f);
            if (!experience.IsRunning) wave = 0f;
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (layer == null) continue;
                float factor = 1f + swell * wave;
                float angle = turnDegrees * Mathf.Sin(Time.time / breathSeconds * Mathf.PI * 2f + i) * wave;

                // Undo last frame's breath before adding this one, so other scripts' scale and turn are kept.
                layer.Pivot.localScale = layer.Pivot.localScale / applied[i] * factor;
                layer.Pivot.localRotation = layer.Pivot.localRotation * Quaternion.Euler(0f, 0f, angle - turned[i]);
                applied[i] = factor;
                turned[i] = angle;
            }
        }

        void Find()
        {
            layers = new MuralLayer[elements.Length];
            applied = new float[elements.Length];
            turned = new float[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                experience.Rig.TryGetLayer(elements[i], out layers[i]);
                applied[i] = 1f;
            }
        }
    }
}
