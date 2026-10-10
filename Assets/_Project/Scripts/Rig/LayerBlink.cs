using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Rig
{
    /// <summary>
    /// Makes closed-eyelid layers blink now and then. At rest each lid is folded up to nothing at its
    /// pivot (the top of the lid), so the open eye shows. A blink unfolds the lids down over the eyes
    /// and folds them back up, all lids together.
    /// </summary>
    /// <remarks>
    /// The lids are hidden by scale, not opacity, so the mural's fade in and out never shows them.
    /// Put it on the experience prefab, list the lid elements from the layers manifest, and call
    /// <see cref="Apply"/> once the rig is built, then <see cref="Play"/> and <see cref="Stop"/>.
    /// </remarks>
    public class LayerBlink : MonoBehaviour
    {
        [Tooltip("Lid element names from the layers manifest, for example lid_left.")]
        [SerializeField] string[] elements = new string[0];
        [Tooltip("Seconds between blinks, picked at random in this range.")]
        [SerializeField] Vector2 interval = new Vector2(3f, 6f);
        [SerializeField, Range(0.05f, 0.6f)] float closeSeconds = 0.12f;
        [SerializeField, Range(0f, 0.3f)] float holdSeconds = 0.06f;
        [SerializeField, Range(0.05f, 0.6f)] float openSeconds = 0.18f;
        [Tooltip("Chance that a blink is followed straight away by a second one.")]
        [SerializeField, Range(0f, 1f)] float doubleBlinkChance = 0.2f;

        readonly List<Transform> lids = new List<Transform>();
        Coroutine loop;

        /// <summary>Finds the lid layers in the rig and folds them open. Call after the rig is built.</summary>
        public void Apply(LayeredMuralRig rig)
        {
            lids.Clear();
            foreach (string element in elements)
            {
                if (!rig.TryGetLayer(element, out MuralLayer layer))
                {
                    Debug.LogWarning($"{name}: no layer called {element} in the rig.", this);
                    continue;
                }
                lids.Add(layer.Pivot);
            }
            SetClosed(0f);
        }

        /// <summary>Starts blinking at random intervals.</summary>
        public void Play()
        {
            Stop();
            if (lids.Count > 0) loop = StartCoroutine(BlinkLoop());
        }

        /// <summary>Stops blinking and leaves the eyes open.</summary>
        public void Stop()
        {
            if (loop != null) StopCoroutine(loop);
            loop = null;
            SetClosed(0f);
        }

        IEnumerator BlinkLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
                yield return Blink();
                if (Random.value < doubleBlinkChance)
                {
                    yield return new WaitForSeconds(0.15f);
                    yield return Blink();
                }
            }
        }

        IEnumerator Blink()
        {
            yield return Tween.Run(closeSeconds, SetClosed, Tween.EaseInOutCubic);
            if (holdSeconds > 0f) yield return new WaitForSeconds(holdSeconds);
            yield return Tween.Run(openSeconds, t => SetClosed(1f - t), Tween.EaseOutCubic);
        }

        // 0 is open (lid folded away), 1 is fully closed.
        void SetClosed(float amount)
        {
            foreach (Transform lid in lids)
                if (lid != null) lid.localScale = new Vector3(1f, amount, 1f);
        }
    }
}
