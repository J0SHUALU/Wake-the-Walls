using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    public class MuralTrackingManager : MonoBehaviour
    {
        static readonly Quaternion ImageToMural = Quaternion.Euler(90f, 0f, 0f);
        const float FocusMargin = 0.02f;

        [SerializeField] ARTrackedImageManager imageManager;
        [SerializeField] MuralLibrary library;
        [SerializeField] TrackingLossHandler lossHandler;
        [Tooltip("Seconds after the app comes back from the background before a mural can show again, so it is placed from a fresh sighting of the wall.")]
        [SerializeField, Min(0f)] float resumeHoldSeconds = 0.75f;
        [Tooltip("When the phone loses sight of a painting that is mostly on screen, something is blocking it, so the mural hides. When less than this share of the mural is on screen, the painting is just too big for the view, so the mural stays.")]
        [SerializeField, Range(0f, 1f)] float blockedShare = 0.85f;
        [Tooltip("Seconds a mural may keep playing on the phone's last guess at where the painting is, before it hides. Keeps a mural steady when you step right up to it, without ever showing a mural you have walked away from.")]
        [SerializeField, Range(0f, 3f)] float lastGuessSeconds = 1f;

        readonly Dictionary<TrackableId, ARTrackedImage> images = new Dictionary<TrackableId, ARTrackedImage>();
        readonly Dictionary<TrackableId, MuralExperience> owners = new Dictionary<TrackableId, MuralExperience>();
        readonly Dictionary<MuralData, MuralExperience> byMural = new Dictionary<MuralData, MuralExperience>();
        readonly Dictionary<MuralExperience, float> lastSeenAt = new Dictionary<MuralExperience, float>();
        MuralExperience active;
        MuralExperience focused;
        float holdUntil;

        void OnEnable()
        {
            imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
            GameEvents.ResetRequested += ResetActive;
        }

        void OnDisable()
        {
            imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
            GameEvents.ResetRequested -= ResetActive;
        }

        void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var image in args.added) Add(image);
            foreach (var pair in args.removed) Remove(pair.Key);
        }

        void Add(ARTrackedImage image)
        {
            if (!TryFindMural(image.referenceImage.name, out var data)) return;
            images[image.trackableId] = image;

            if (byMural.TryGetValue(data, out var existing) && existing != null)
            {
                owners[image.trackableId] = existing;
                if (Refresh(existing)) Focus(existing);
                return;
            }

            var experience = Instantiate(data.ExperiencePrefab, image.transform);
            experience.transform.SetLocalPositionAndRotation(Vector3.zero, ImageToMural);
            experience.Initialize(data);
            experience.OnFound();

            owners[image.trackableId] = experience;
            byMural[data] = experience;
            active = experience;
            GameEvents.RaiseMuralFound(data);
            Focus(experience);
        }

        void Update()
        {
            MuralExperience centred = null;
            float closest = float.MaxValue;
            foreach (var experience in byMural.Values)
            {
                if (experience == null || !Refresh(experience)) continue;
                float distance = DistanceFromCentre(experience);
                if (distance < closest)
                {
                    closest = distance;
                    centred = experience;
                }
            }
            if (centred != null && (focused == null || centred == focused || closest < DistanceFromCentre(focused) - FocusMargin)) Focus(centred);
        }

        void Focus(MuralExperience experience)
        {
            if (experience == focused) return;
            focused = experience;
            active = experience;
            GameEvents.RaiseMuralFocused(experience.Data);
        }

        static float DistanceFromCentre(MuralExperience experience)
        {
            var cam = Camera.main;
            if (cam == null) return float.MaxValue;
            Vector3 view = cam.WorldToViewportPoint(experience.transform.position);
            if (view.z <= 0f) return float.MaxValue;
            return ((Vector2)view - new Vector2(0.5f, 0.5f)).sqrMagnitude;
        }

        bool Refresh(MuralExperience experience)
        {
            ARTrackedImage best = null;
            ARTrackedImage held = null;
            foreach (var pair in owners)
            {
                if (pair.Value != experience || !images.TryGetValue(pair.Key, out var image)) continue;
                if (image.trackingState == TrackingState.Tracking) { best = image; break; }
                if (image.trackingState == TrackingState.Limited) held = image;
            }

            if (best != null) lastSeenAt[experience] = Time.time;

            if (best == null && held != null && experience.transform.parent == held.transform
                && lastSeenAt.TryGetValue(experience, out float seenAt) && Time.time - seenAt <= lastGuessSeconds
                && MuralView.IsInView(experience) && MuralView.ShareOnScreen(experience) < blockedShare)
                best = held;

            if (Time.unscaledTime < holdUntil) best = null;

            if (best != null && experience.transform.parent != best.transform)
            {
                experience.transform.SetParent(best.transform, false);
                experience.transform.SetLocalPositionAndRotation(Vector3.zero, ImageToMural);
            }
            bool blocked = best == null && held != null && MuralView.IsInView(experience)
                && MuralView.ShareOnScreen(experience) >= blockedShare;
            lossHandler.Report(experience, best != null, blocked);
            return best != null;
        }

        void Remove(TrackableId id)
        {
            images.Remove(id);
            if (!owners.TryGetValue(id, out var experience)) return;
            owners.Remove(id);
            if (owners.ContainsValue(experience)) { Refresh(experience); return; }

            lossHandler.Forget(experience);
            lastSeenAt.Remove(experience);
            if (experience != null)
            {
                byMural.Remove(experience.Data);
                Destroy(experience.gameObject);
            }
            if (active == experience) active = null;
            if (focused == experience) focused = null;
        }

        bool TryFindMural(string imageName, out MuralData data)
        {
            data = null;
            if (string.IsNullOrEmpty(imageName)) return false;
            if (library.TryGet(imageName, out data) || library.TryGet(BaseName(imageName), out data))
            {
                if (data.ExperiencePrefab != null) return true;
                Debug.LogWarning($"Mural '{data.ReferenceImageName}' has no experience prefab yet.");
                return false;
            }
            Debug.LogWarning($"No mural data for reference image '{imageName}'.");
            return false;
        }

        static string BaseName(string imageName)
        {
            int cut = imageName.LastIndexOf('_');
            return cut > 0 ? imageName.Substring(0, cut) : imageName;
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused) holdUntil = Time.unscaledTime + resumeHoldSeconds;
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus) holdUntil = Time.unscaledTime + resumeHoldSeconds;
        }

        void ResetActive()
        {
            if (active == null) return;
            active.ResetExperience();
            active.OnFound();
        }
    }
}
