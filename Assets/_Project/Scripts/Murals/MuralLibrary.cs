using System.Collections.Generic;
using UnityEngine;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// The list of murals the app knows about. Code looks murals up here
    /// instead of hard coding names or IDs.
    /// </summary>
    [CreateAssetMenu(menuName = "Wake the Walls/Mural Library", fileName = "MuralLibrary")]
    public class MuralLibrary : ScriptableObject
    {
        [SerializeField] List<MuralData> murals = new List<MuralData>();

        /// <summary>All murals in display order.</summary>
        public IReadOnlyList<MuralData> Murals => murals;

        /// <summary>Finds the mural whose reference image has the given name.</summary>
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
