using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Lifts chosen elements of a layered mural off the wall towards the viewer, one after another,
    /// grows them slightly so they read as coming out, holds them in the air and lays them back down.
    /// Plays a little after the mural is found and repeats while it stays in view.
    /// Which elements move is set on the prefab, so the script holds no mural names.
    /// </summary>
    public class PopOutIntro : MonoBehaviour
    {
        [Serializable]
        public class Item
        {
            public string element;
            [Range(0f, LayerMotion.MaxPeelMeters)] public float distance = 0.2f;
            [Tooltip("Seconds after the start of each cycle before this element lifts.")]
            public float delay;
            [Tooltip("How much bigger the element gets while out, 0.1 is 10 percent.")]
            [Range(0f, 0.3f)] public float grow = 0.08f;
        }

        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] List<Item> items = new List<Item>();
        [SerializeField] float startDelay = 1.6f;
        [SerializeField, Range(0.6f, 1.5f)] float liftDuration = 1.2f;
        [SerializeField] float holdSeconds = 4f;
        [SerializeField] float restSeconds = 5f;
        [SerializeField] bool loop = true;

        Coroutine cycle;

        void OnEnable()
        {
            GameEvents.MuralFound += HandleFound;
            GameEvents.MuralLost += HandleLost;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleFound;
            GameEvents.MuralLost -= HandleLost;
        }

        void HandleFound(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            if (cycle != null) StopCoroutine(cycle);
            cycle = StartCoroutine(Cycle());
        }

        void HandleLost(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            if (cycle != null) StopCoroutine(cycle);
            cycle = null;
            LayAllDown();
        }

        IEnumerator Cycle()
        {
            yield return new WaitForSeconds(startDelay);
            do
            {
                float longestDelay = 0f;
                foreach (var item in items)
                {
                    StartCoroutine(Lift(item));
                    longestDelay = Mathf.Max(longestDelay, item.delay);
                }
                yield return new WaitForSeconds(longestDelay + liftDuration + holdSeconds);
                LayAllDown();
                yield return new WaitForSeconds(liftDuration + restSeconds);
            }
            while (loop && experience.IsRunning);
            cycle = null;
        }

        IEnumerator Lift(Item item)
        {
            if (item.delay > 0f) yield return new WaitForSeconds(item.delay);
            if (!experience.IsRunning || !TryGet(item.element, out var layer, out var motion)) yield break;
            motion.Peel(item.distance, liftDuration);
            yield return Tween.Scale(layer.Pivot, Vector3.one * (1f + item.grow), liftDuration, Tween.EaseOutBack);
        }

        void LayAllDown()
        {
            foreach (var item in items)
            {
                if (!TryGet(item.element, out var layer, out var motion)) continue;
                motion.Return(liftDuration);
                StartCoroutine(Tween.Scale(layer.Pivot, Vector3.one, liftDuration));
            }
        }

        bool TryGet(string element, out MuralLayer layer, out LayerMotion motion)
        {
            motion = null;
            if (!experience.Rig.TryGetLayer(element, out layer)) return false;
            motion = layer.GetComponent<LayerMotion>();
            if (motion == null) motion = layer.gameObject.AddComponent<LayerMotion>();
            return true;
        }
    }
}
