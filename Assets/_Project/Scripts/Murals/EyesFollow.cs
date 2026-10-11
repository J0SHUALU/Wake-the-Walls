using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Turns a painted face's eyes towards the viewer. The iris layers slide a little inside the eye
    /// towards where the phone is, so the eyes seem to follow people as they walk past.
    /// Which layers are the irises is set on the prefab, so the script holds no mural names.
    /// </summary>
    public class EyesFollow : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("The iris layers, drawn under the lids.")]
        [SerializeField] string[] irises = new string[0];
        [Tooltip("Furthest an iris moves from its painted spot, in metres.")]
        [SerializeField] float maxOffset = 0.035f;
        [Tooltip("Higher is snappier.")]
        [SerializeField] float followSpeed = 4f;

        Transform[] quads;
        Vector3[] rest;
        Vector2 offset;

        void Update()
        {
            if (experience == null || !experience.IsRunning || Camera.main == null) return;
            if (quads == null && !Find()) return;

            // Where the phone is, seen from the mural, turned into a direction across the wall.
            Vector3 viewer = experience.transform.InverseTransformPoint(Camera.main.transform.position);
            float distance = Mathf.Max(0.5f, -viewer.z);
            Vector2 target = Vector2.ClampMagnitude(new Vector2(viewer.x, viewer.y) / distance, 1f) * maxOffset;
            offset = Vector2.Lerp(offset, target, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));

            for (int i = 0; i < quads.Length; i++)
            {
                if (quads[i] != null) quads[i].localPosition = rest[i] + (Vector3)offset;
            }
        }

        // The quad under each iris pivot is only moved by this script, so the sway and pop-out still work.
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
