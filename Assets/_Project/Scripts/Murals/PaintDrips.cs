using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Lets wet paint drip off chosen elements of a mural and splash on the ground in front of the
    /// wall, in the mural's own colours, while it is in view.
    /// Which elements drip is set on the prefab, so the script holds no mural names.
    /// </summary>
    public class PaintDrips : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] string[] elements = new string[0];
        [SerializeField] PaintBurst dripPrefab;
        [SerializeField] PaintBurst splashPrefab;
        [Tooltip("Colours for drips and splashes. Uses the mural palette when empty.")]
        [SerializeField] Color[] colours = new Color[0];
        [SerializeField] float startDelay = 3f;
        [SerializeField] Vector2 interval = new Vector2(1.5f, 3f);
        [Tooltip("Seconds from a drip starting to its splash on the ground.")]
        [SerializeField] float fallSeconds = 1.1f;
        [Tooltip("How far in front of the wall the splash lands, in metres.")]
        [SerializeField] float splashOut = 0.15f;

        Coroutine loop;

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
            if (experience == null || experience.Data != mural || elements.Length == 0) return;
            if (loop != null) StopCoroutine(loop);
            loop = StartCoroutine(Drip());
        }

        void HandleLost(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            StopAllCoroutines();
            loop = null;
        }

        IEnumerator Drip()
        {
            yield return new WaitForSeconds(startDelay);
            while (experience.IsRunning)
            {
                string element = elements[Random.Range(0, elements.Length)];
                if (experience.Rig.TryGetLayer(element, out var layer))
                {
                    Color[] palette = colours.Length > 0 ? colours : experience.Data.Palette;
                    Vector2 at = layer.PivotLocal;
                    if (dripPrefab != null) PaintBurst.Spawn(dripPrefab, experience.transform, new Vector3(at.x, at.y, -0.04f), palette);
                    if (splashPrefab != null) StartCoroutine(Splash(at.x, palette));
                }
                yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
            }
        }

        IEnumerator Splash(float x, Color[] palette)
        {
            yield return new WaitForSeconds(fallSeconds);
            float floor = -experience.Rig.MuralSize.y * 0.5f + 0.03f;
            PaintBurst.Spawn(splashPrefab, experience.transform, new Vector3(x, floor, -splashOut), palette);
        }
    }
}
