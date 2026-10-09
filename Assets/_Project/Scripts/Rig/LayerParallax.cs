using UnityEngine;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// Makes the layers of a <see cref="LayeredMuralRig"/> separate when the viewer walks sideways,
    /// by offsetting each layer as if it sat further from the wall than it really does.
    /// </summary>
    /// <remarks>
    /// Layers are only 2 to 5 mm off the wall, so their real parallax is too small to see.
    /// Each frame this uses the camera's angle to the mural centre to work out how far a layer
    /// would slide if its depth were multiplied by <c>depthScale</c>, and shifts it in X and Y.
    /// Seen straight on from the centre the shift is zero, so the layers sit exactly on the paint.
    /// It only touches X and Y, so peeling along Z keeps working at the same time.
    /// </remarks>
    [RequireComponent(typeof(LayeredMuralRig))]
    public class LayerParallax : MonoBehaviour
    {
        [Tooltip("How many times deeper each layer should look. 1 means no extra parallax.")]
        [SerializeField, Min(1f)] float depthScale = 12f;

        [Tooltip("Largest sideways shift for any layer, in metres.")]
        [SerializeField, Min(0f)] float maxOffset = 0.04f;

        [Tooltip("How quickly layers follow the camera. Higher is snappier.")]
        [SerializeField, Min(0.1f)] float followSpeed = 8f;

        [Tooltip("Camera to follow. Uses the main camera when empty.")]
        [SerializeField] Camera viewCamera;

        LayeredMuralRig rig;

        /// <summary>Turns the effect on or off. Layers ease back to centre when it is off.</summary>
        public bool Active { get; set; } = true;

        void Awake()
        {
            rig = GetComponent<LayeredMuralRig>();
        }

        void LateUpdate()
        {
            if (!rig.IsBuilt) return;
            Camera cam = viewCamera != null ? viewCamera : Camera.main;
            if (cam == null) return;

            // Camera position in the rig's space: the wall is the XY plane, the viewer is at -Z.
            Vector3 eye = transform.InverseTransformPoint(cam.transform.position);
            float blend = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            foreach (MuralLayer layer in rig.Layers)
            {
                Vector2 target = Active ? OffsetFor(layer, eye) : Vector2.zero;
                Vector3 p = layer.transform.localPosition;
                Vector2 current = new Vector2(p.x, p.y);
                Vector2 next = Vector2.Lerp(current, target, blend);
                layer.transform.localPosition = new Vector3(next.x, next.y, p.z);
            }
        }

        // Shift that makes the layer look depthScale times deeper, from the camera's angle to the mural centre.
        Vector2 OffsetFor(MuralLayer layer, Vector3 eye)
        {
            float viewerDistance = -eye.z;
            float realDepth = layer.RestDepth;
            float fakeDepth = realDepth * depthScale;
            if (viewerDistance <= fakeDepth + 0.05f) return Vector2.zero;

            float stretch = (fakeDepth - realDepth) / (viewerDistance - fakeDepth);
            Vector2 fromEye = -new Vector2(eye.x, eye.y);
            return Vector2.ClampMagnitude(fromEye * stretch, maxOffset);
        }
    }
}
