using System;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Narrator
{
    [CreateAssetMenu(menuName = "Wake the Walls/Narrator Lines", fileName = "NarratorLines")]
    public class NarratorLines : ScriptableObject
    {
        [Serializable]
        public class MuralLines
        {
            public string referenceImageName;
            public AudioClip intro;
            public AudioClip about;
            public AudioClip tryThis;
            public AudioClip done;
        }

        [SerializeField] AudioClip hello;
        [SerializeField] AudioClip tapMe;
        [SerializeField] List<MuralLines> murals = new List<MuralLines>();

        public AudioClip Hello => hello;
        public AudioClip TapMe => tapMe;

        public void Set(AudioClip helloClip, AudioClip tapMeClip, List<MuralLines> muralLines)
        {
            hello = helloClip;
            tapMe = tapMeClip;
            murals = muralLines;
        }

        public MuralLines For(MuralData mural)
        {
            if (mural == null) return null;
            foreach (var lines in murals)
            {
                if (lines.referenceImageName == mural.ReferenceImageName) return lines;
            }
            return null;
        }
    }
}
