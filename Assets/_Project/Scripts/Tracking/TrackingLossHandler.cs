using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    public class TrackingLossHandler : MonoBehaviour
    {
        [Tooltip("Seconds the painting must be out of sight or blocked before the mural hides and the rescan page shows.")]
        [SerializeField, Min(0f)] float gracePeriod = 0.5f;

        readonly Dictionary<MuralExperience, Coroutine> pending = new Dictionary<MuralExperience, Coroutine>();
        readonly HashSet<MuralExperience> lost = new HashSet<MuralExperience>();
        bool wasBlocked;

        public void Report(MuralExperience experience, bool isTracking, bool blocked = false)
        {
            if (experience == null) return;
            if (!isTracking) wasBlocked = blocked;

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
            GameEvents.ViewBlocked = wasBlocked;
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
