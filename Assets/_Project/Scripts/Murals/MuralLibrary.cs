using System.Collections.Generic;
using UnityEngine;

namespace WakeTheWalls.Murals
{
    [CreateAssetMenu(menuName = "Wake the Walls/Mural Library", fileName = "MuralLibrary")]
    public class MuralLibrary : ScriptableObject
    {
        [SerializeField] List<MuralData> murals = new List<MuralData>();

        public IReadOnlyList<MuralData> Murals => murals;

        public bool TryGet(string referenceImageName, out MuralData mural)
        {
            foreach (var candidate in murals)
            {
                if (candidate != null && candidate.ReferenceImageName == referenceImageName)
                {
                    mural = candidate;
                    return true;
                }
            }

            mural = null;
            return false;
        }
    }
}
