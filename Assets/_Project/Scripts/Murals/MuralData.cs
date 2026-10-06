using UnityEngine;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Everything the app needs to know about one physical mural.
    /// Create one asset per mural with Create > Wake the Walls > Mural Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Wake the Walls/Mural Data", fileName = "MuralData")]
    public class MuralData : ScriptableObject
    {
        [Tooltip("Must match the image name in the reference image library exactly.")]
        [SerializeField] string referenceImageName;
        [SerializeField] string displayName;
        [SerializeField] string location;

        [Tooltip("Measured width and height of the painted area in metres.")]
        [SerializeField] Vector2 physicalSizeMeters = Vector2.one;

        [TextArea(3, 10)]
        [SerializeField] string story;

        [SerializeField] MuralExperience experiencePrefab;

        [Tooltip("Colours sampled from the mural, used to tint shaders, VFX and UI accents.")]
        [SerializeField] Color[] palette = new Color[0];

        [SerializeField] AudioClip ambience;

        /// <summary>Name of the matching image in the reference image library.</summary>
        public string ReferenceImageName => referenceImageName;

        /// <summary>Title shown in the UI.</summary>
        public string DisplayName => displayName;

        /// <summary>Where the mural is on campus.</summary>
        public string Location => location;

        /// <summary>Real size of the mural in metres (x is width, y is height).</summary>
        public Vector2 PhysicalSizeMeters => physicalSizeMeters;

        /// <summary>The story of the mural as given by guest relations.</summary>
        public string Story => story;

        /// <summary>Prefab spawned on the mural when it is found.</summary>
        public MuralExperience ExperiencePrefab => experiencePrefab;

        /// <summary>Colours picked from the painting.</summary>
        public Color[] Palette => palette;

        /// <summary>Looping background sound for this mural.</summary>
        public AudioClip Ambience => ambience;
    }
}
