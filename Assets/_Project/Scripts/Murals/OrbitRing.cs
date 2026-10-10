using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// One painted ring that becomes a real 3D ring. It starts flat on the wall, tilted so that from the
    /// front it covers its painted ellipse exactly, then gains depth as it lifts off the wall and keeps
    /// turning and gently rocking, so it reads as orbiting the figure's head.
    /// </summary>
    public class OrbitRing : MonoBehaviour
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        Transform tilt, spinner;
        Vector3 wallPoint;
        float tiltDegrees, outDistance, depthScale, spinSpeed, rockDegrees, phase;
        float lift, size, spinBoost = 1f, spread;
        Coroutine liftTween, sizeTween, boostTween;

        /// <summary>True while the ring turns.</summary>
        public bool IsPlaying { get; set; }

        /// <summary>True once the ring has lifted off the wall.</summary>
        public bool IsOut => lift > 0.99f;

        /// <summary>Builds the ring at its painted spot. It starts hidden.</summary>
        /// <param name="centre">Centre of the painted ellipse in mural space, metres.</param>
        /// <param name="radius">Half the painted ellipse's width, metres.</param>
        /// <param name="heightRatio">Painted ellipse height divided by its width.</param>
        public void Build(Material material, Color colour, Vector3 centre, float radius, float thickness,
            float heightRatio, float depthRatio, float spinDegreesPerSecond, float rock, float phaseOffset)
        {
            wallPoint = centre;
            tiltDegrees = Mathf.Asin(Mathf.Clamp(heightRatio, 0.02f, 0.9f)) * Mathf.Rad2Deg;
            depthScale = Mathf.Clamp01(depthRatio);
            outDistance = radius * depthScale + 0.05f;
            spinSpeed = spinDegreesPerSecond;
            rockDegrees = rock;
            phase = phaseOffset;

            tilt = new GameObject("Tilt").transform;
            tilt.SetParent(transform, false);
            spinner = new GameObject("Ring").transform;
            spinner.SetParent(tilt, false);
            spinner.localRotation = Quaternion.Euler(0f, phase * 360f, 0f);
            spinner.gameObject.AddComponent<MeshFilter>().sharedMesh = RingMesh.Build(radius, thickness);
            var ring = spinner.gameObject.AddComponent<MeshRenderer>();
            ring.sharedMaterial = material;
            var block = new MaterialPropertyBlock();
            block.SetColor(BaseColor, colour);
            ring.SetPropertyBlock(block);
            ResetRing();
        }

        /// <summary>Appears on the wall over the painted ring.</summary>
        public Coroutine Show(float seconds) => Restart(ref sizeTween, Tween.Value(size, 1f, SetSize, seconds, Tween.EaseOutCubic));

        /// <summary>Shrinks away, for when the mural is lost.</summary>
        public Coroutine Hide(float seconds) => Restart(ref sizeTween, Tween.Value(size, 0f, SetSize, seconds));

        /// <summary>Lifts off the wall into its orbit.</summary>
        public Coroutine LiftOut(float seconds) => Restart(ref liftTween, Tween.Value(lift, 1f, SetLift, seconds, Tween.EaseInOutCubic));

        /// <summary>Turns faster for a while, then eases back to the normal speed.</summary>
        public void Boost(float factor, float seconds) =>
            Restart(ref boostTween, Tween.Value(factor, 1f, v => spinBoost = v, seconds, Tween.EaseOutCubic));

        /// <summary>Moves the ring up or down from its orbit, in metres, for interactions that spread the rings.</summary>
        public float Spread
        {
            get => spread;
            set { spread = value; SetLift(lift); }
        }

        /// <summary>Back flat on the wall, hidden and still.</summary>
        public void ResetRing()
        {
            StopAllCoroutines();
            liftTween = sizeTween = boostTween = null;
            IsPlaying = false;
            spinBoost = 1f;
            spread = 0f;
            SetLift(0f);
            SetSize(0f);
        }

        void Update()
        {
            if (!IsPlaying) return;
            spinner.Rotate(0f, spinSpeed * spinBoost * Time.deltaTime, 0f, Space.Self);
            float t = Time.time * 0.4f + phase * Mathf.PI * 2f;
            float rock = rockDegrees * lift;
            tilt.localRotation = Quaternion.Euler(tiltDegrees + Mathf.Sin(t) * rock, 0f, Mathf.Cos(t) * rock * 0.5f);
        }

        // 0 is flat on the wall over the painting, 1 is out in its orbit with full depth.
        void SetLift(float value)
        {
            lift = value;
            transform.localPosition = wallPoint + new Vector3(0f, spread * lift, -outDistance * lift);
            transform.localScale = new Vector3(1f, 1f, Mathf.Lerp(0.02f, depthScale, lift));
            if (lift < 0.01f) tilt.localRotation = Quaternion.Euler(tiltDegrees, 0f, 0f);
        }

        void SetSize(float value)
        {
            size = value;
            tilt.localScale = Vector3.one * value;
            spinner.gameObject.SetActive(value > 0.001f);
        }

        Coroutine Restart(ref Coroutine slot, IEnumerator routine)
        {
            if (slot != null) StopCoroutine(slot);
            slot = isActiveAndEnabled ? StartCoroutine(routine) : null;
            return slot;
        }
    }
}
