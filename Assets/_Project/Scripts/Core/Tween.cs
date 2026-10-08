using System;
using System.Collections;
using UnityEngine;

namespace WakeTheWalls.Core
{
    /// <summary>
    /// Small eased tweens run as coroutines. Start them from a MonoBehaviour with StartCoroutine.
    /// Transitions in this project last 0.6 to 1.5 seconds and are never instant.
    /// </summary>
    public static class Tween
    {
        /// <summary>Default length of a transition in seconds.</summary>
        public const float DefaultDuration = 0.8f;

        /// <summary>Runs <paramref name="step"/> from 0 to 1 over <paramref name="duration"/> seconds, eased.</summary>
        public static IEnumerator Run(float duration, Action<float> step, Func<float, float> ease = null)
        {
            ease ??= EaseInOutCubic;
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                step(ease(Mathf.Clamp01(time / duration)));
                yield return null;
            }
            step(1f);
        }

        /// <summary>Scales <paramref name="target"/> from its current scale to <paramref name="to"/>.</summary>
        public static IEnumerator Scale(Transform target, Vector3 to, float duration = DefaultDuration, Func<float, float> ease = null)
        {
            Vector3 from = target.localScale;
            return Run(duration, t => target.localScale = Vector3.LerpUnclamped(from, to, t), ease);
        }

        /// <summary>Moves <paramref name="target"/> in local space from its current position to <paramref name="to"/>.</summary>
        public static IEnumerator Move(Transform target, Vector3 to, float duration = DefaultDuration, Func<float, float> ease = null)
        {
            Vector3 from = target.localPosition;
            return Run(duration, t => target.localPosition = Vector3.LerpUnclamped(from, to, t), ease);
        }

        /// <summary>Changes a value from <paramref name="from"/> to <paramref name="to"/>, for fades and similar.</summary>
        public static IEnumerator Value(float from, float to, Action<float> apply, float duration = DefaultDuration, Func<float, float> ease = null)
        {
            return Run(duration, t => apply(Mathf.LerpUnclamped(from, to, t)), ease);
        }

        /// <summary>Slow start, slow end.</summary>
        public static float EaseInOutCubic(float t) =>
            t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        /// <summary>Fast start, gentle stop.</summary>
        public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        /// <summary>Overshoots slightly before settling. Good for things that grow into place.</summary>
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
