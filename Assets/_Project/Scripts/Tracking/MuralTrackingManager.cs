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

        [SerializeField] ARTrackedImageManager imageManager;
        [SerializeField] MuralLibrary library;
        [SerializeField] TrackingLossHandler lossHandler;
        [SerializeField] MuralAnchors anchors;

        readonly Dictionary<TrackableId, ARTrackedImage> images = new Dictionary<TrackableId, ARTrackedImage>();
        readonly Dictionary<TrackableId, MuralExperience> owners = new Dictionary<TrackableId, MuralExperience>();
        readonly Dictionary<MuralData, MuralExperience> byMural = new Dictionary<MuralData, MuralExperience>();
        MuralExperience active;
        MuralExperience focused;

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
            anchors.Place(experience, new Pose(image.transform.position, image.transform.rotation));

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
            if (centred != null) Focus(centred);
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
            bool limited = false;
            foreach (var pair in owners)
            {
                if (pair.Value != experience || !images.TryGetValue(pair.Key, out var image)) continue;
                if (image.trackingState == TrackingState.Tracking) { best = image; break; }
                if (image.trackingState == TrackingState.Limited) limited = true;
            }

            if (best != null && anchors.HasAnchor(experience))
            {
                anchors.Follow(experience, new Pose(best.transform.position, best.transform.rotation * ImageToMural));
            }
            else if (best != null && experience.transform.parent != best.transform)
            {
                experience.transform.SetParent(best.transform, false);
                experience.transform.SetLocalPositionAndRotation(Vector3.zero, ImageToMural);
            }
            lossHandler.Report(experience, best != null, limited);
            return best != null;
        }

        void Remove(TrackableId id)
        {
            images.Remove(id);
            if (!owners.TryGetValue(id, out var experience)) return;
            owners.Remove(id);
            if (owners.ContainsValue(experience)) { Refresh(experience); return; }
            if (anchors.HasAnchor(experience)) return;

            lossHandler.Forget(experience);
            anchors.Remove(experience);
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

        void ResetActive()
        {
            if (active == null) return;
            active.ResetExperience();
            active.OnFound();
        }
    }
}
