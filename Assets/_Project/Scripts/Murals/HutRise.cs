using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    // The huts grow up out of the enclosure floor one by one, in place on the wall, once the wall has rebuilt.
    public class HutRise : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] string[] huts = new string[0];
        [Tooltip("Plain floor painted over the huts until they rise.")]
        [SerializeField] string floorElement = "floor_patch";
        [SerializeField, Min(0f)] float startDelay = 3.6f;
        [SerializeField, Min(0f)] float stagger = 0.7f;
        [SerializeField, Range(0.3f, 1.5f)] float riseSeconds = 0.7f;

        bool played;
        Coroutine rise;

        void OnEnable()
        {
            GameEvents.MuralFound += HandleFound;
            GameEvents.ResetRequested += HandleReset;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleFound;
            GameEvents.ResetRequested -= HandleReset;
        }

        void HandleFound(MuralData mural)
        {
            if (experience != null && experience.Data == mural) TryStart();
        }

        // Also starts from the experience itself, so it plays however the mural was woken.
        void Update()
        {
            if (!played && experience != null && experience.Rig.IsBuilt) SetHuts(0f);
            TryStart();
        }

        void TryStart()
        {
            if (played || experience == null || !experience.IsRunning || !experience.Rig.IsBuilt) return;
            played = true;
            if (rise != null) StopCoroutine(rise);
            rise = StartCoroutine(Rise());
        }

        void HandleReset()
        {
            played = false;
            if (rise != null) StopCoroutine(rise);
            rise = null;
            SetHuts(1f);
            if (experience != null && experience.Rig.TryGetLayer(floorElement, out MuralLayer floor)) floor.SetAlpha(0f);
        }

        IEnumerator Rise()
        {
            SetHuts(0f);
            bool hasFloor = experience.Rig.TryGetLayer(floorElement, out MuralLayer floor);
            if (hasFloor) floor.SetAlpha(1f);
            yield return new WaitForSeconds(startDelay);
            foreach (string hut in huts)
            {
                if (experience.Rig.TryGetLayer(hut, out MuralLayer layer))
                    StartCoroutine(Tween.Scale(layer.Pivot, Vector3.one, riseSeconds, Tween.EaseOutBack));
                yield return new WaitForSeconds(stagger);
            }
            yield return new WaitForSeconds(riseSeconds);
            if (hasFloor) yield return Tween.Value(1f, 0f, floor.SetAlpha, 0.6f);
            rise = null;
        }

        void SetHuts(float scale)
        {
            if (experience == null) return;
            foreach (string hut in huts)
                if (experience.Rig.TryGetLayer(hut, out MuralLayer layer)) { float s = Mathf.Max(scale, 0.001f); layer.Pivot.localScale = new Vector3(s, s, 1f); }
        }
    }
}
