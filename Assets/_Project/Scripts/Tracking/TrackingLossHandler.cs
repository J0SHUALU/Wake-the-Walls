using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    public class TrackingLossHandler : MonoBehaviour
    {
        [Tooltip("Seconds the mural must be out of sight before it pauses. Covers people walking past and quick glances away.")]
        [SerializeField, Min(0f)] float gracePeriod = 2f;
        [Tooltip("Keeps an anchored mural playing while the camera still faces it, even when the painting is blocked.")]
        [SerializeField] MuralAnchors anchors;

        readonly Dictionary<MuralExperience, Coroutine> pending = new Dictionary<MuralExperience, Coroutine>();
        readonly HashSet<MuralExperience> lost = new HashSet<MuralExperience>();

        public void Report(MuralExperience experience, bool isTracking)
        {
            if (experience == null) return;

            if (isTracking || FacesAnchoredMural(experience))
            {
                CancelPending(experience);
                if (lost.Remove(experience))
                {
                    experience.OnResume();
                    GameEvents.RaiseMuralFound(experience.Data);
                }
            }
            else if (!lost.Contains(experience) && !pending.ContainsKey(experience))
            {
                pending[experience] = StartCoroutine(LoseAfterGrace(experience));
            }
        }

        bool FacesAnchoredMural(MuralExperience experience) =>
            anchors != null && anchors.IsTracking(experience) && MuralView.IsInView(experience);

        public void Forget(MuralExperience experience)
        {
            CancelPending(experience);
            lost.Remove(experience);
        }

        IEnumerator LoseAfterGrace(MuralExperience experience)
        {
            yield return new WaitForSeconds(gracePeriod);
            pending.Remove(experience);
            if (experience == null) yield break;

            lost.Add(experience);
            experience.OnLost();
            GameEvents.RaiseMuralLost(experience.Data);
        }

        void CancelPending(MuralExperience experience)
        {
            if (pending.TryGetValue(experience, out var routine))
            {
                if (routine != null) StopCoroutine(routine);
                pending.Remove(experience);
            }
        }
    }
}
