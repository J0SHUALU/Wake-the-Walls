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
        [Tooltip("How long a mural keeps playing without the painting itself being seen, while the camera still faces it.")]
        [SerializeField, Min(0f)] float anchorTrustSeconds = 20f;
        [Tooltip("A mural that is not seen only keeps playing while the camera is within this many mural widths of the wall.")]
        [SerializeField, Min(1f)] float maxDistanceInWidths = 2f;
        [Tooltip("The smallest such distance in metres, for narrow murals.")]
        [SerializeField, Min(1f)] float minDistanceMetres = 5f;

        readonly Dictionary<MuralExperience, Coroutine> pending = new Dictionary<MuralExperience, Coroutine>();
        readonly HashSet<MuralExperience> lost = new HashSet<MuralExperience>();
        readonly Dictionary<MuralExperience, float> lastSeen = new Dictionary<MuralExperience, float>();

        public void Report(MuralExperience experience, bool isTracking, bool isLimited = false)
        {
            if (experience == null) return;
            if (isTracking) lastSeen[experience] = Time.time;

            if (isTracking || StillFacesMural(experience, isLimited))
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

        bool StillFacesMural(MuralExperience experience, bool isLimited)
        {
            bool held = isLimited || (anchors != null && anchors.IsTracking(experience));
            if (!held) return false;
            if (!lastSeen.TryGetValue(experience, out float seen) || Time.time - seen > anchorTrustSeconds) return false;
            return IsNear(experience) && MuralView.IsInView(experience);
        }

        bool IsNear(MuralExperience experience)
        {
            var cam = Camera.main;
            if (cam == null || experience.Data == null) return false;
            Vector3 local = experience.transform.InverseTransformPoint(cam.transform.position);
            if (local.z >= 0f) return false;
            float limit = Mathf.Max(minDistanceMetres, experience.Data.PhysicalSizeMeters.x * maxDistanceInWidths);
            return local.magnitude <= limit;
        }

        public void Forget(MuralExperience experience)
        {
            CancelPending(experience);
            lost.Remove(experience);
            lastSeen.Remove(experience);
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
