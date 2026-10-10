using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// Gives a set of rig layers their idle sway and bob, set up per element in the Inspector,
    /// and starts, pauses or resets them together. Element names come from the layers manifest.
    /// </summary>
    public class LayerMotionSet : MonoBehaviour
    {
        /// <summary>Motion settings for one element.</summary>
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

        /// <summary>The motion components this set controls, in Inspector order.</summary>
        public IReadOnlyList<LayerMotion> Motions => motions;

        /// <summary>Adds and configures a <see cref="LayerMotion"/> on every listed layer. Call after the rig is built.</summary>
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

        /// <summary>Finds the motion for an element, for interactions that push one layer harder.</summary>
        public bool TryGetMotion(string element, out LayerMotion motion)
        {
            motion = motions.Find(m => m != null && m.Layer.Element == element);
            return motion != null;
        }

        /// <summary>Starts every layer moving, one after another.</summary>
        public Coroutine PlayAll() => StartCoroutine(PlayStaggered());

        /// <summary>Eases every layer back to rest. Progress is kept for <see cref="PlayAll"/>.</summary>
        public void PauseAll()
        {
            StopAllCoroutines();
            foreach (LayerMotion motion in motions) motion.Pause();
        }

        /// <summary>Snaps every layer back to rest on the wall. Use only while the mural is hidden.</summary>
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
