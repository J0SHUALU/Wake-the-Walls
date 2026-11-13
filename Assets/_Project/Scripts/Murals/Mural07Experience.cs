using UnityEngine;

namespace WakeTheWalls.Murals
{
    public class Mural07Experience : LayeredMuralExperience
    {
        [SerializeField] BrickBuild wall;

        protected override void OnResetState()
        {
            if (wall != null) wall.ResetWall();
        }
    }
}
