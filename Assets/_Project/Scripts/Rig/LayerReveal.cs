using System;
using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// Paints a mural layer on or wipes it off with the brush dissolve, so nothing ever pops in or out.
    /// Add it to the same GameObject as the <see cref="MuralLayer"/>.
    /// </summary>
    /// <remarks>
    /// The layer's material must use the WakeTheWalls/BrushDissolve shader (BrushDissolve.mat),
    /// otherwise the dissolve value has no visible effect. Transitions last 0.6 to 1.5 seconds.
    /// </remarks>
    [RequireComponent(typeof(MuralLayer))]
    public class LayerReveal : MonoBehaviour
    {
        const float MinDuration = 0.6f;
        const float MaxDuration = 1.5f;

        [SerializeField, Range(MinDuration, MaxDuration)] float duration = 1f;
        [Tooltip("Start fully wiped off, waiting for Reveal.")]
        [SerializeField] bool startHidden = true;

        MuralLayer layer;
        Coroutine running;

        /// <summary>Raised when a reveal finishes.</summary>
        public event Action Revealed;

        /// <summary>Raised when a hide finishes.</summary>
        public event Action Hidden;

        /// <summary>True when the layer is fully painted on, or on its way there.</summary>
        public bool IsShown { get; private set; }

        /// <summary>True while a reveal or hide is playing.</summary>
        public bool IsAnimating => running != null;

        MuralLayer Layer => layer != null ? layer : layer = GetComponent<MuralLayer>();

        void Start()
        {
            if (Layer.Renderer != null && Layer.Renderer.sharedMaterial != null &&
                Layer.Renderer.sharedMaterial.shader.name != "WakeTheWalls/BrushDissolve")
                Debug.LogWarning($"{name}: LayerReveal needs a BrushDissolve material to be visible.", this);

            if (startHidden && !IsShown && !IsAnimating) SetShown(false);
        }

        /// <summary>Paints the layer on with a brush sweep.</summary>
        /// <param name="seconds">Optional length, clamped to 0.6 to 1.5 s. Uses the Inspector value when left out.</param>
        public Coroutine Reveal(float seconds = -1f) => Play(true, seconds);

        /// <summary>Wipes the layer off with a brush sweep.</summary>
        /// <param name="seconds">Optional length, clamped to 0.6 to 1.5 s. Uses the Inspector value when left out.</param>
        public Coroutine Hide(float seconds = -1f) => Play(false, seconds);

        /// <summary>Jumps straight to shown or hidden with no animation. Only use it while the mural is out of view.</summary>
        public void SetShown(bool shown)
        {
            Stop();
            IsShown = shown;
            Layer.SetDissolve(shown ? 0f : 1f);
        }

        Coroutine Play(bool show, float seconds)
        {
            Stop();
            IsShown = show;
            float length = Mathf.Clamp(seconds > 0f ? seconds : duration, MinDuration, MaxDuration);
            running = StartCoroutine(Run(show, length));
            return running;
        }

        IEnumerator Run(bool show, float length)
        {
            float target = show ? 0f : 1f;
            yield return Tween.Value(Layer.Dissolve, target, Layer.SetDissolve, length, Tween.EaseInOutCubic);
            running = null;
            if (show) Revealed?.Invoke();
            else Hidden?.Invoke();
        }

        void Stop()
        {
            if (running == null) return;
            StopCoroutine(running);
            running = null;
        }
    }
}
