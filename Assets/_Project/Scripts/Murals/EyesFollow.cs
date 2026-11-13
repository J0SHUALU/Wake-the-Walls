using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    // Moves the iris layers like eyeballs: they follow the viewer and glance around now and then.
    // A socket layer drawn above the irises hides them outside the eye opening.
    public class EyesFollow : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("The iris layers, drawn under the lids.")]
        [SerializeField] string[] irises = new string[0];
        [Tooltip("Furthest an iris moves from its painted spot, in metres.")]
        [SerializeField] float maxOffset = 0.035f;
        [Tooltip("Higher is snappier.")]
        [SerializeField] float followSpeed = 4f;

        [Header("Glances")]
        [Tooltip("Seconds between glances away from the viewer, picked at random in this range. 0,0 turns glances off.")]
        [SerializeField] Vector2 glanceEvery = new Vector2(2.5f, 5f);
        [Tooltip("How long a glance is held, in seconds.")]
        [SerializeField] Vector2 glanceHold = new Vector2(0.5f, 1.2f);
        [Tooltip("How far up and down a glance goes compared with side to side.")]
        [SerializeField, Range(0f, 1f)] float glanceVertical = 0.35f;
        [SerializeField] float glanceSpeed = 14f;

        Transform[] quads;
        Vector3[] rest;
        Vector2 offset, glance;
        float nextGlance, glanceEnds;
        bool glancing;

        void Update()
        {
            if (experience == null || !experience.IsRunning || Camera.main == null) return;
            if (quads == null && !Find()) return;

            Vector3 viewer = experience.transform.InverseTransformPoint(Camera.main.transform.position);
            float distance = Mathf.Max(0.5f, -viewer.z);
            Vector2 target = Vector2.ClampMagnitude(new Vector2(viewer.x, viewer.y) / distance, 1f) * maxOffset;

            UpdateGlance();
            if (glancing) target = glance;

            float speed = glancing ? glanceSpeed : followSpeed;
            offset = Vector2.Lerp(offset, target, 1f - Mathf.Exp(-speed * Time.deltaTime));

            for (int i = 0; i < quads.Length; i++)
            {
                if (quads[i] != null) quads[i].localPosition = rest[i] + (Vector3)offset;
            }
        }

        void UpdateGlance()
        {
            if (glanceEvery.y <= 0f) return;
            if (nextGlance <= 0f) nextGlance = Time.time + Random.Range(glanceEvery.x, glanceEvery.y);

            if (!glancing && Time.time >= nextGlance)
            {
                glancing = true;
                glanceEnds = Time.time + Random.Range(glanceHold.x, glanceHold.y);
                float side = Random.value < 0.5f ? -1f : 1f;
                glance = new Vector2(side * Random.Range(0.6f, 1f), Random.Range(-1f, 1f) * glanceVertical) * maxOffset;
            }
            else if (glancing && Time.time >= glanceEnds)
            {
                glancing = false;
                nextGlance = Time.time + Random.Range(glanceEvery.x, glanceEvery.y);
            }
        }

        bool Find()
        {
            if (experience.Rig == null || !experience.Rig.IsBuilt) return false;
            quads = new Transform[irises.Length];
            rest = new Vector3[irises.Length];
            for (int i = 0; i < irises.Length; i++)
            {
                if (!experience.Rig.TryGetLayer(irises[i], out MuralLayer layer) || layer.Pivot.childCount == 0) continue;
                quads[i] = layer.Pivot.GetChild(0);
                rest[i] = quads[i].localPosition;
            }
            return true;
        }
    }
}
