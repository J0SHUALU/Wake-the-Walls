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
    public class MuralTrackingManager : MonoBehaviour
    {
        // Turns AR Foundation's image plane (X and Z, normal on +Y) into ours (+X right, +Y up, -Z to the viewer).
        static readonly Quaternion ImageToMural = Quaternion.Euler(90f, 0f, 0f);

        [SerializeField] ARTrackedImageManager imageManager;
        [SerializeField] MuralLibrary library;
        [SerializeField] TrackingLossHandler lossHandler;

        readonly Dictionary<TrackableId, MuralExperience> spawned = new Dictionary<TrackableId, MuralExperience>();
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
            foreach (var image in args.added) Spawn(image);

            foreach (var image in args.updated)
            {
                if (spawned.TryGetValue(image.trackableId, out var experience))
                    lossHandler.Report(experience, image.trackingState == TrackingState.Tracking);
            }

            foreach (var pair in args.removed) Despawn(pair.Key);
        }

        void Spawn(ARTrackedImage image)
        {
            string imageName = image.referenceImage.name;
            if (!library.TryGet(imageName, out var data))
            {
                Debug.LogWarning($"No mural data for reference image '{imageName}'.");
                return;
            }
            if (data.ExperiencePrefab == null)
            {
                Debug.LogWarning($"Mural '{imageName}' has no experience prefab yet.");
                return;
            }

            var experience = Instantiate(data.ExperiencePrefab, image.transform);
            experience.transform.SetLocalPositionAndRotation(Vector3.zero, ImageToMural);
            experience.Initialize(data);
            experience.OnFound();

            spawned[image.trackableId] = experience;
            active = experience;
            GameEvents.RaiseMuralFound(data);
        }

        void Despawn(TrackableId id)
        {
            if (!spawned.TryGetValue(id, out var experience)) return;

            lossHandler.Forget(experience);
            spawned.Remove(id);
            if (active == experience) active = null;
            if (experience != null) Destroy(experience.gameObject);
        }

        void ResetActive()
        {
            if (active == null) return;
            active.ResetExperience();
            active.OnFound();
        }
    }
}
