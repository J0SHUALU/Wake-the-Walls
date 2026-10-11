using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    public class PaintInIntro : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] float backgroundSeconds = 1.6f;
        [SerializeField] float layerSeconds = 0.8f;
        [Tooltip("Seconds between one element starting to paint in and the next.")]
        [SerializeField] float stagger = 0.08f;

        bool played;

        void OnEnable() => GameEvents.MuralFound += HandleFound;
        void OnDisable() => GameEvents.MuralFound -= HandleFound;

        void HandleFound(MuralData mural)
        {
            if (played || experience == null || experience.Data != mural || !experience.Rig.IsBuilt) return;
            played = true;
            StartCoroutine(Paint());
        }

        IEnumerator Paint()
        {
            var rig = experience.Rig;
            foreach (MuralLayer layer in rig.Layers) layer.SetDissolve(1f);
            rig.Background.SetDissolve(1f);

            StartCoroutine(Reveal(rig.Background, backgroundSeconds));
            yield return new WaitForSeconds(backgroundSeconds * 0.6f);
            foreach (MuralLayer layer in rig.Layers)
            {
                StartCoroutine(Reveal(layer, layerSeconds));
                yield return new WaitForSeconds(stagger);
            }
        }

        static IEnumerator Reveal(MuralLayer layer, float seconds) =>
            Tween.Value(1f, 0f, v => { if (layer != null) layer.SetDissolve(v); }, seconds, Tween.EaseInOutCubic);
    }
}
