using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
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
