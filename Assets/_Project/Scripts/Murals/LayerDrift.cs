using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    // Slides a layer slowly out and back from where it is painted, for clouds and other things carried by the wind.
    // It only travels one way so a layer near the edge never slides off the mural.
    public class LayerDrift : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Element name from the layers manifest.")]
        [SerializeField] string element;
        [Tooltip("How far it travels from where it is painted, as a share of the mural width. Negative travels left.")]
        [SerializeField, Range(-0.3f, 0.3f)] float distance = 0.06f;
        [Tooltip("Seconds for one trip there and back.")]
        [SerializeField, Min(2f)] float seconds = 24f;
        [Tooltip("Seconds to ease in after the mural wakes.")]
        [SerializeField, Range(0.1f, 3f)] float easeIn = 1.5f;

        LayerMotion motion;
        Transform pivot;
        Vector3 rest;
        float time, weight;
        bool found;

        void Update()
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            if (!found) Find();
            if (motion == null && pivot == null) return;

            weight = Mathf.MoveTowards(weight, experience.IsRunning ? 1f : 0f, Time.deltaTime / easeIn);
            if (weight > 0f) time += Time.deltaTime;

            float trip = 0.5f - 0.5f * Mathf.Cos(time / seconds * Mathf.PI * 2f);
            float x = trip * distance * experience.Rig.MuralSize.x * weight;
            if (motion != null) motion.ExtraOffset = new Vector2(x, 0f);
            else pivot.localPosition = rest + new Vector3(x, 0f, 0f);
        }

        void Find()
        {
            found = true;
            if (!experience.Rig.TryGetLayer(element, out MuralLayer layer)) return;
            motion = layer.GetComponent<LayerMotion>();
            pivot = layer.Pivot;
            rest = pivot.localPosition;
        }
    }
}
