using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// Gentle idle motion for one mural layer: a sway that rotates the element around its pivot,
    /// plus a small vertical bob. Add it to the same GameObject as the <see cref="MuralLayer"/>.
    /// </summary>
    /// <remarks>
    /// Motion fades in and out through <see cref="Weight"/>, so starting or stopping never snaps.
    /// Use a different phase on each layer so they do not move in step.
    /// </remarks>
    [RequireComponent(typeof(MuralLayer))]
    public class LayerMotion : MonoBehaviour
    {
        [Header("Sway")]
        [Tooltip("Largest rotation either side of rest, in degrees.")]
        [SerializeField] float swayDegrees = 2f;
        [Tooltip("Full sway cycles per second.")]
        [SerializeField] float swaySpeed = 0.25f;

        [Header("Bob")]
        [Tooltip("Largest vertical movement either side of rest, in metres.")]
        [SerializeField] float bobMeters = 0.004f;
        [Tooltip("Full bob cycles per second.")]
        [SerializeField] float bobSpeed = 0.35f;

        [Header("Timing")]
        [Tooltip("Offset in cycles (0 to 1) so neighbouring layers move out of step.")]
        [SerializeField, Range(0f, 1f)] float phase;
        [SerializeField] bool playOnStart;

        MuralLayer layer;
        Coroutine weightTween;
        float time;
        bool playing;

        /// <summary>How much of the motion is applied, 0 at rest to 1 full.</summary>
        public float Weight { get; private set; }

        /// <summary>The layer this component moves.</summary>
        public MuralLayer Layer => layer != null ? layer : layer = GetComponent<MuralLayer>();

        /// <summary>Sets the motion values from code, for layers the rig creates at runtime.</summary>
        public void Configure(float swayDegrees, float swaySpeed, float bobMeters, float bobSpeed, float phase)
        {
            this.swayDegrees = swayDegrees;
            this.swaySpeed = swaySpeed;
            this.bobMeters = bobMeters;
            this.bobSpeed = bobSpeed;
            this.phase = phase;
        }

        void Start()
        {
            if (playOnStart) Play();
        }

        /// <summary>Starts or continues the motion, easing it in.</summary>
        public void Play(float duration = Tween.DefaultDuration)
        {
            playing = true;
            FadeWeight(1f, duration);
        }

        /// <summary>Eases the motion out and holds the layer at rest. <see cref="Play"/> continues from here.</summary>
        public void Pause(float duration = Tween.DefaultDuration)
        {
            FadeWeight(0f, duration, () => playing = false);
        }

        /// <summary>Stops at once and puts the layer back at rest. Use only when it is hidden.</summary>
        public void ResetMotion()
        {
            if (weightTween != null) StopCoroutine(weightTween);
            playing = false;
            Weight = 0f;
            time = 0f;
            ApplyPose();
        }

        void Update()
        {
            if (!playing) return;
            time += Time.deltaTime;
            ApplyPose();
        }

        void ApplyPose()
        {
            if (Layer.Pivot == null) return;
            const float TwoPi = Mathf.PI * 2f;
            float angle = swayDegrees * Mathf.Sin(TwoPi * (swaySpeed * time + phase)) * Weight;
            float lift = bobMeters * Mathf.Sin(TwoPi * (bobSpeed * time + phase + 0.25f)) * Weight;
            Layer.Pivot.localRotation = Quaternion.Euler(0f, 0f, angle);
            Layer.Pivot.localPosition = new Vector3(Layer.PivotLocal.x, Layer.PivotLocal.y + lift, Layer.Pivot.localPosition.z);
        }

        void FadeWeight(float target, float duration, System.Action done = null)
        {
            if (weightTween != null) StopCoroutine(weightTween);
            weightTween = StartCoroutine(FadeRoutine(target, duration, done));
        }

        IEnumerator FadeRoutine(float target, float duration, System.Action done)
        {
            yield return Tween.Value(Weight, target, w => { Weight = w; ApplyPose(); }, duration);
            done?.Invoke();
        }
    }
}
