using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    /// <summary>
    /// Spawns each mural's experience on its tracked image. When AR Foundation finds an image,
    /// this looks up the image name in the mural library, places the experience prefab on the
    /// mural and raises <see cref="GameEvents.MuralFound"/>. Tracking changes go to the loss handler.
    /// </summary>
    /// <remarks>
    /// A mural can have extra reference images for close-up scanning, named after the mural with a
    /// suffix, for example "Mural1_centre". These must be centred on the mural so the experience sits
    /// in the same place whichever image is found. All images of one mural share one experience.
    /// </remarks>
    public class MuralTrackingManager : MonoBehaviour
    {
        // Turns AR Foundation's image plane (X and Z, normal on +Y) into ours (+X right, +Y up, -Z to the viewer).
        static readonly Quaternion ImageToMural = Quaternion.Euler(90f, 0f, 0f);

        [SerializeField] ARTrackedImageManager imageManager;
        [SerializeField] MuralLibrary library;
        [SerializeField] TrackingLossHandler lossHandler;

        readonly Dictionary<TrackableId, ARTrackedImage> images = new Dictionary<TrackableId, ARTrackedImage>();
        readonly Dictionary<TrackableId, MuralExperience> owners = new Dictionary<TrackableId, MuralExperience>();
        readonly Dictionary<MuralData, MuralExperience> byMural = new Dictionary<MuralData, MuralExperience>();
        MuralExperience active;

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
            foreach (var image in args.updated)
            {
                if (owners.TryGetValue(image.trackableId, out var experience)) Refresh(experience);
            }
            foreach (var pair in args.removed) Remove(pair.Key);
        }

        void Add(ARTrackedImage image)
        {
            if (!TryFindMural(image.referenceImage.name, out var data)) return;
            images[image.trackableId] = image;

            // Another image of the same mural already spawned it: share that experience.
            if (byMural.TryGetValue(data, out var existing) && existing != null)
            {
                owners[image.trackableId] = existing;
                Refresh(existing);
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
        }

        // Tracking counts as long as any image of the mural is tracked. The experience follows a tracked image.
        void Refresh(MuralExperience experience)
        {
            ARTrackedImage best = null;
            foreach (var pair in owners)
            {
                if (pair.Value != experience || !images.TryGetValue(pair.Key, out var image)) continue;
                if (image.trackingState == TrackingState.Tracking) { best = image; break; }
            }

            if (best != null && experience.transform.parent != best.transform)
            {
                experience.transform.SetParent(best.transform, false);
                experience.transform.SetLocalPositionAndRotation(Vector3.zero, ImageToMural);
            }
            lossHandler.Report(experience, best != null);
        }

        void Remove(TrackableId id)
        {
            images.Remove(id);
            if (!owners.TryGetValue(id, out var experience)) return;
            owners.Remove(id);
            if (owners.ContainsValue(experience)) { Refresh(experience); return; }

            lossHandler.Forget(experience);
            if (experience != null)
            {
                byMural.Remove(experience.Data);
                Destroy(experience.gameObject);
            }
            if (active == experience) active = null;
        }

        bool TryFindMural(string imageName, out MuralData data)
        {
            if (library.TryGet(imageName, out data) || library.TryGet(BaseName(imageName), out data))
            {
                if (data.ExperiencePrefab != null) return true;
                Debug.LogWarning($"Mural '{data.ReferenceImageName}' has no experience prefab yet.");
                return false;
            }
            Debug.LogWarning($"No mural data for reference image '{imageName}'.");
            return false;
        }

        // "Mural1_centre" becomes "Mural1".
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
