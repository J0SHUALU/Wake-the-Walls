using System;
using System.Collections;
using UnityEngine;

namespace WakeTheWalls.Core
{
    public static class Tween
    {
        public const float DefaultDuration = 0.8f;

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

        public static IEnumerator Scale(Transform target, Vector3 to, float duration = DefaultDuration, Func<float, float> ease = null)
        {
            Vector3 from = target.localScale;
            return Run(duration, t => target.localScale = Vector3.LerpUnclamped(from, to, t), ease);
        }

        public static IEnumerator Move(Transform target, Vector3 to, float duration = DefaultDuration, Func<float, float> ease = null)
        {
            Vector3 from = target.localPosition;
            return Run(duration, t => target.localPosition = Vector3.LerpUnclamped(from, to, t), ease);
        }

        public static IEnumerator Value(float from, float to, Action<float> apply, float duration = DefaultDuration, Func<float, float> ease = null)
        {
            return Run(duration, t => apply(Mathf.LerpUnclamped(from, to, t)), ease);
        }

        public static float EaseInOutCubic(float t) =>
            t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
