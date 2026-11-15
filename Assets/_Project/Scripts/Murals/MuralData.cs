using System;
using System.Collections.Generic;
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

        [Tooltip("Small readable copy of the painting, used to notice when something is in front of it.")]
        [SerializeField] Texture2D checkImage;

        [Serializable]
        public class Spot
        {
            [Tooltip("Matches the topic or element a tap asks about.")]
            public string topic;
            public string title;
            [TextArea(2, 6)] public string text;
            [Tooltip("Optional line Pearl says when this spot is tapped.")]
            public AudioClip voice;
        }

        [Tooltip("Extra info for parts of the mural people can tap, such as the faces on Mural 4.")]
        [SerializeField] List<Spot> spots = new List<Spot>();

        public string ReferenceImageName => referenceImageName;

        public string DisplayName => displayName;

        public string Location => location;

        public Vector2 PhysicalSizeMeters => physicalSizeMeters;

        public string Story => story;

        public MuralExperience ExperiencePrefab => experiencePrefab;

        public Color[] Palette => palette;

        public AudioClip Ambience => ambience;

        public Texture2D CheckImage => checkImage;

        public Spot FindSpot(string topic)
        {
            if (string.IsNullOrEmpty(topic)) return null;
            foreach (Spot spot in spots) if (spot.topic == topic) return spot;
            return null;
        }
    }
}
