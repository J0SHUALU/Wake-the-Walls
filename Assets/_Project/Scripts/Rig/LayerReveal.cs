using System;
using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Rig
{
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

        public event Action Revealed;

        public event Action Hidden;

        public bool IsShown { get; private set; }

        public bool IsAnimating => running != null;

        MuralLayer Layer => layer != null ? layer : layer = GetComponent<MuralLayer>();

        void Start()
        {
            if (Layer.Renderer != null && Layer.Renderer.sharedMaterial != null &&
                Layer.Renderer.sharedMaterial.shader.name != "WakeTheWalls/BrushDissolve")
                Debug.LogWarning($"{name}: LayerReveal needs a BrushDissolve material to be visible.", this);

            if (startHidden && !IsShown && !IsAnimating) SetShown(false);
        }

        public Coroutine Reveal(float seconds = -1f) => Play(true, seconds);

        public Coroutine Hide(float seconds = -1f) => Play(false, seconds);

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
