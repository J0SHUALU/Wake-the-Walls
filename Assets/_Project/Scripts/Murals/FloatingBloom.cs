using System;
using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    public class FloatingBloom : MonoBehaviour
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        const float ClosedDegrees = 72f;
        const float OpenDegrees = 12f;

        Transform[] petals;
        Renderer centre;
        Renderer[] petalRenderers;
        Coroutine moving;
        Vector3 hoverPoint;
        float size, hoverTime, opening = ClosedDegrees;

        public Vector3 Home { get; private set; }

        public Vector3 Away { get; private set; }

        public bool IsOut { get; private set; }

        public event Action Returned;

        public void Build(Material material, Color petalColour, Color centreColour, float diameter, Vector3 home, Vector3 away)
        {
            size = diameter;
            Home = home;
            Away = away;
            petalRenderers = BloomBuilder.Build(transform, 6, material, out petals, out centre);
            var block = new MaterialPropertyBlock();
            block.SetColor(BaseColor, petalColour);
            foreach (Renderer r in petalRenderers) r.SetPropertyBlock(block);
            block.SetColor(BaseColor, centreColour);
            centre.SetPropertyBlock(block);
            Hide();
        }

        public Coroutine FloatOut(float seconds = 1.5f) => Run(FloatOutRoutine(seconds));

        public Coroutine GoHome(float seconds = 1.2f) => Run(GoHomeRoutine(seconds));

        public Coroutine FadeAway(float seconds = 0.6f) => Run(Shrink(seconds));

        public Coroutine ComeBack(float seconds = 0.8f) => IsOut ? Run(Grow(Away, seconds)) : null;

        public void Hide()
        {
            if (moving != null) StopCoroutine(moving);
            moving = null;
            IsOut = false;
            transform.localPosition = Home;
            transform.localScale = Vector3.zero;
            SetOpening(ClosedDegrees);
        }

        void Update()
        {
            if (!IsOut || moving != null) return;
            hoverTime += Time.deltaTime;
            transform.localPosition = hoverPoint + new Vector3(0f, Mathf.Sin(hoverTime * 1.3f) * 0.02f, Mathf.Sin(hoverTime * 0.7f) * 0.015f);
            transform.localRotation = Quaternion.Euler(0f, 0f, hoverTime * 8f);
        }

        IEnumerator FloatOutRoutine(float seconds)
        {
            IsOut = true;
            transform.localPosition = Home;
            yield return Tween.Scale(transform, Vector3.one * size * 0.5f, 0.6f, Tween.EaseOutBack);
            StartCoroutine(Tween.Value(opening, OpenDegrees, SetOpening, seconds));
            transform.localScale = Vector3.one * size * 0.5f;
            yield return Tween.Run(seconds, t =>
            {
                transform.localPosition = Vector3.LerpUnclamped(Home, Away, t);
                transform.localScale = Vector3.one * size * Mathf.Lerp(0.5f, 1f, t);
            });
            hoverPoint = Away;
        }

        IEnumerator GoHomeRoutine(float seconds)
        {
            StartCoroutine(Tween.Value(opening, ClosedDegrees, SetOpening, seconds * 0.6f));
            yield return Tween.Move(transform, Home, seconds);
            yield return Shrink(0.6f);
            IsOut = false;
            Returned?.Invoke();
        }

        IEnumerator Shrink(float seconds) => Tween.Scale(transform, Vector3.zero, seconds, Tween.EaseInOutCubic);

        IEnumerator Grow(Vector3 at, float seconds)
        {
            transform.localPosition = at;
            hoverPoint = at;
            yield return Tween.Scale(transform, Vector3.one * size, seconds, Tween.EaseOutBack);
        }

        Coroutine Run(IEnumerator routine)
        {
            if (moving != null) StopCoroutine(moving);
            moving = StartCoroutine(Wrap(routine));
            return moving;
        }

        IEnumerator Wrap(IEnumerator routine)
        {
            yield return routine;
            moving = null;
        }

        void SetOpening(float degrees)
        {
            opening = degrees;
            if (petals != null) BloomBuilder.SetOpening(petals, degrees);
        }
    }
}
