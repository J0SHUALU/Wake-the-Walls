using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    public class MuralAnchors : MonoBehaviour
    {
        [SerializeField] ARAnchorManager anchorManager;
        [Tooltip("How quickly a mural slides onto the image while the image is tracked. Higher is snappier.")]
        [SerializeField, Range(1f, 30f)] float followSpeed = 8f;

        readonly Dictionary<MuralExperience, ARAnchor> anchors = new Dictionary<MuralExperience, ARAnchor>();

        void OnEnable()
        {
            if (anchorManager != null) anchorManager.trackablesChanged.AddListener(OnAnchorsChanged);
        }

        void OnDisable()
        {
            if (anchorManager != null) anchorManager.trackablesChanged.RemoveListener(OnAnchorsChanged);
        }

        void OnAnchorsChanged(ARTrackablesChangedEventArgs<ARAnchor> args)
        {
            foreach (var pair in args.removed) Release(pair.Value);
        }

        void Release(ARAnchor removed)
        {
            MuralExperience owner = null;
            foreach (var entry in anchors)
            {
                if (entry.Value == removed) { owner = entry.Key; break; }
            }
            if (owner == null) return;

            anchors.Remove(owner);
            if (removed != null) owner.transform.SetParent(removed.transform.parent, true);
        }

        public bool HasAnchor(MuralExperience experience) => anchors.TryGetValue(experience, out var anchor) && anchor != null;

        public bool IsTracking(MuralExperience experience) =>
            anchors.TryGetValue(experience, out var anchor) && anchor != null && anchor.trackingState == TrackingState.Tracking;

        public async void Place(MuralExperience experience, Pose pose)
        {
            if (anchorManager == null || experience == null || anchors.ContainsKey(experience)) return;
            anchors[experience] = null;

            var result = await anchorManager.TryAddAnchorAsync(pose);
            if (!result.status.IsSuccess() || experience == null)
            {
                anchors.Remove(experience);
                if (result.status.IsSuccess()) anchorManager.TryRemoveAnchor(result.value);
                return;
            }
            anchors[experience] = result.value;
            experience.transform.SetParent(result.value.transform, true);
        }

        public void Follow(MuralExperience experience, Pose target)
        {
            if (!anchors.TryGetValue(experience, out var anchor) || anchor == null) return;
            Transform space = anchor.transform;
            Vector3 position = space.InverseTransformPoint(target.position);
            Quaternion rotation = Quaternion.Inverse(space.rotation) * target.rotation;

            float blend = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            Transform mural = experience.transform;
            mural.SetLocalPositionAndRotation(
                Vector3.Lerp(mural.localPosition, position, blend),
                Quaternion.Slerp(mural.localRotation, rotation, blend));
        }

        public void Remove(MuralExperience experience)
        {
            if (!anchors.TryGetValue(experience, out var anchor)) return;
            anchors.Remove(experience);
            if (anchor != null && anchorManager != null) anchorManager.TryRemoveAnchor(anchor);
        }
    }
}
