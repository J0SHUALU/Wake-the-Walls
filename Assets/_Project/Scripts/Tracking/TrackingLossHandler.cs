using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    /// <summary>
    /// Decides when a mural counts as lost. Tracking has to stay down for longer than the
    /// grace period before the experience is paused, so a quick glance away does nothing.
    /// The tracking manager reports every tracking change here.
    /// </summary>
    public class TrackingLossHandler : MonoBehaviour
    {
        [Tooltip("Seconds tracking must stay down before the mural counts as lost.")]
        [SerializeField] float gracePeriod = 0.75f;

        readonly Dictionary<MuralExperience, Coroutine> pending = new Dictionary<MuralExperience, Coroutine>();
        readonly HashSet<MuralExperience> lost = new HashSet<MuralExperience>();

        /// <summary>Called by the tracking manager whenever an experience's tracking state may have changed.</summary>
        public void Report(MuralExperience experience, bool isTracking)
        {
            if (experience == null) return;

            if (isTracking)
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

        /// <summary>Stops tracking an experience, for example when it is destroyed.</summary>
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
