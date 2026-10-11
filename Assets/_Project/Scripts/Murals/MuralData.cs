using UnityEngine;

namespace WakeTheWalls.Murals
{
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

        public string ReferenceImageName => referenceImageName;

        public string DisplayName => displayName;

        public string Location => location;

        public Vector2 PhysicalSizeMeters => physicalSizeMeters;

        public string Story => story;

        public MuralExperience ExperiencePrefab => experiencePrefab;

        public Color[] Palette => palette;

        public AudioClip Ambience => ambience;
    }
}
