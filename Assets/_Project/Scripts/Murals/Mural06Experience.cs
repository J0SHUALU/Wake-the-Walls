using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    public class Mural06Experience : LayeredMuralExperience
    {
        [Header("Caravan")]
        [Tooltip("Elements that walk slowly along the dunes and back, for example the caravan.")]
        [SerializeField] string[] walkElements = new string[0];
        [Tooltip("How far they walk from their painted spot, in metres. Negative walks left.")]
        [SerializeField, Range(-0.3f, 0.3f)] float walkMeters = 0.08f;
        [Tooltip("Full walks out and back per second. Keep it very slow.")]
        [SerializeField, Range(0.005f, 0.2f)] float walkSpeed = 0.04f;
        [Tooltip("Height of each step while walking, in metres.")]
        [SerializeField, Range(0f, 0.02f)] float stepMeters = 0.004f;
        [SerializeField, Range(0.5f, 4f)] float stepsPerSecond = 1.6f;

        float walkTime;

        protected override void OnResetState()
        {
            walkTime = 0f;
            ForEachWalker(m => m.ExtraOffset = Vector2.zero);
        }

        void Update()
        {
            if (!IsRunning || walkElements.Length == 0) return;
            walkTime += Time.deltaTime;
            float x = walkMeters * (0.5f - 0.5f * Mathf.Cos(Mathf.PI * 2f * walkSpeed * walkTime));
            float y = stepMeters * Mathf.Abs(Mathf.Sin(Mathf.PI * stepsPerSecond * walkTime));
            ForEachWalker(m => m.ExtraOffset = new Vector2(x, y));
        }

        void ForEachWalker(System.Action<LayerMotion> action)
        {
            foreach (string element in walkElements)
                if (MotionSet.TryGetMotion(element, out LayerMotion motion)) action(motion);
        }
    }
}
