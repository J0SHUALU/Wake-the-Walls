using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Rig
{
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

        public const float MaxPeelMeters = 0.3f;

        const float MinTransition = 0.6f;
        const float MaxTransition = 1.5f;

        MuralLayer layer;
        Coroutine weightTween;
        Coroutine peelTween;
        float time;
        bool playing;
        Vector2 extraOffset;

        public float Weight { get; private set; }

        public float ExtraDegrees { get; set; }

        public Vector2 ExtraOffset
        {
            get => extraOffset;
            set { extraOffset = value; ApplyPose(); }
        }

        public float PeelDistance => Layer != null ? -Layer.transform.localPosition.z - Layer.RestDepth : 0f;

        public MuralLayer Layer => layer != null ? layer : layer = GetComponent<MuralLayer>();

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

        public void Play(float duration = Tween.DefaultDuration)
        {
            playing = true;
            FadeWeight(1f, duration);
        }

        public void Pause(float duration = Tween.DefaultDuration)
        {
            FadeWeight(0f, duration, () => playing = false);
        }

        public void Peel(float distance, float duration = Tween.DefaultDuration)
        {
            MoveToDepth(Layer.RestDepth + Mathf.Clamp(distance, 0f, MaxPeelMeters), duration, Tween.EaseOutCubic);
        }

        public void Return(float duration = Tween.DefaultDuration)
        {
            MoveToDepth(Layer.RestDepth, duration, Tween.EaseInOutCubic);
        }

        public void ResetMotion()
        {
            if (weightTween != null) StopCoroutine(weightTween);
            if (peelTween != null) StopCoroutine(peelTween);
            playing = false;
            Weight = 0f;
            time = 0f;
            Vector3 p = Layer.transform.localPosition;
            Layer.transform.localPosition = new Vector3(p.x, p.y, -Layer.RestDepth);
            ApplyPose();
        }

        void MoveToDepth(float depth, float duration, System.Func<float, float> ease)
        {
            if (peelTween != null) StopCoroutine(peelTween);
            duration = Mathf.Clamp(duration, MinTransition, MaxTransition);
            Transform t = Layer.transform;
            peelTween = StartCoroutine(Tween.Value(t.localPosition.z, -depth,
                z => t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z), duration, ease));
        }

        void Update()
        {
            if (!playing) return;
            time += Time.deltaTime;
            ApplyPose();
        }

        void ApplyPose()
        {
            if (Layer == null || Layer.Pivot == null) return;
            const float TwoPi = Mathf.PI * 2f;
            float angle = swayDegrees * Mathf.Sin(TwoPi * (swaySpeed * time + phase)) * Weight + ExtraDegrees;
            float lift = bobMeters * Mathf.Sin(TwoPi * (bobSpeed * time + phase + 0.25f)) * Weight;
            Layer.Pivot.localRotation = Quaternion.Euler(0f, 0f, angle);
            Layer.Pivot.localPosition = new Vector3(Layer.PivotLocal.x + extraOffset.x, Layer.PivotLocal.y + lift + extraOffset.y, Layer.Pivot.localPosition.z);
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
