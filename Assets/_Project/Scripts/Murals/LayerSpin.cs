using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    // Turns layers around their pivot while the mural is awake, for round things like rosettes and pinwheel flowers.
    public class LayerSpin : MonoBehaviour
    {
        [System.Serializable]
        public class Item
        {
            [Tooltip("Element name from the layers manifest.")]
            public string element;
            [Tooltip("Degrees per second. Negative turns clockwise.")]
            public float speed = 20f;
        }

        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] Item[] items = new Item[0];
        [Tooltip("Seconds to reach full speed after the mural wakes.")]
        [SerializeField, Range(0.1f, 3f)] float easeIn = 1.2f;

        Transform[] pivots;
        LayerMotion[] motions;
        float[] angles;
        float weight;

        void Update()
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            if (pivots == null) Find();

            weight = Mathf.MoveTowards(weight, experience.IsRunning ? 1f : 0f, Time.deltaTime / easeIn);
            if (weight <= 0f) return;

            for (int i = 0; i < pivots.Length; i++)
            {
                if (pivots[i] == null) continue;
                angles[i] = Mathf.Repeat(angles[i] + items[i].speed * weight * Time.deltaTime, 360f);
                // A layer that also sways gets the turn through its motion, so the two don't fight.
                if (motions[i] != null) motions[i].ExtraDegrees = angles[i];
                else pivots[i].localRotation = Quaternion.Euler(0f, 0f, angles[i]);
            }
        }

        void Find()
        {
            pivots = new Transform[items.Length];
            angles = new float[items.Length];
            motions = new LayerMotion[items.Length];
            for (int i = 0; i < items.Length; i++)
                if (experience.Rig.TryGetLayer(items[i].element, out MuralLayer layer))
                {
                    pivots[i] = layer.Pivot;
                    motions[i] = layer.GetComponent<LayerMotion>();
                }
        }
    }
}
