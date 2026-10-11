using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WakeTheWalls.Rig
{
    public class LayerMotionSet : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Element name from the layers manifest, for example tree_left.")]
            public string element;
            public float swayDegrees = 2f;
            public float swaySpeed = 0.25f;
            public float bobMeters = 0.003f;
            public float bobSpeed = 0.3f;
            [Range(0f, 1f)] public float phase;
        }

        [SerializeField] List<Entry> entries = new List<Entry>();
        [Tooltip("Seconds between each layer starting, so they do not all wake at once.")]
        [SerializeField, Min(0f)] float stagger = 0.35f;

        readonly List<LayerMotion> motions = new List<LayerMotion>();

        public IReadOnlyList<LayerMotion> Motions => motions;

        public void Apply(LayeredMuralRig rig)
        {
            motions.Clear();
            foreach (Entry entry in entries)
            {
                if (!rig.TryGetLayer(entry.element, out MuralLayer layer))
                {
                    Debug.LogWarning($"{name}: no layer called {entry.element} in the rig.", this);
                    continue;
                }
                LayerMotion motion = layer.GetComponent<LayerMotion>();
                if (motion == null) motion = layer.gameObject.AddComponent<LayerMotion>();
                motion.Configure(entry.swayDegrees, entry.swaySpeed, entry.bobMeters, entry.bobSpeed, entry.phase);
                motions.Add(motion);
            }
        }

        public bool TryGetMotion(string element, out LayerMotion motion)
        {
            motion = motions.Find(m => m != null && m.Layer.Element == element);
            return motion != null;
        }

        public Coroutine PlayAll() => StartCoroutine(PlayStaggered());

        public void PauseAll()
        {
            StopAllCoroutines();
            foreach (LayerMotion motion in motions) motion.Pause();
        }

        public void ResetAll()
        {
            StopAllCoroutines();
            foreach (LayerMotion motion in motions) motion.ResetMotion();
        }

        IEnumerator PlayStaggered()
        {
            foreach (LayerMotion motion in motions)
            {
                motion.Play();
                if (stagger > 0f) yield return new WaitForSeconds(stagger);
            }
        }
    }
}
