using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Mural 1: the painting wakes up. The layers fade in over the wall, the trees and earrings start
    /// to sway one by one, paint dust drifts down from the canopies, and now and then she blinks.
    /// Everything is set up on the prefab: which elements move in the motion set, the dust sources,
    /// the lids that blink, and the tap areas. Completes once a tree and an earring have both been tapped.
    /// </summary>
    public class Mural01Experience : LayeredMuralExperience
    {
        [SerializeField] LayerBlink blink;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (blink != null) blink.Apply(Rig);
        }

        protected override void OnStarted(bool resumed)
        {
            if (blink != null) blink.Play();
        }

        protected override void OnStopped()
        {
            if (blink != null) blink.Stop();
        }

        protected override void OnResetState()
        {
            if (blink != null) blink.Stop();
        }
    }
}
