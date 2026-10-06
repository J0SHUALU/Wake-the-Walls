using UnityEngine;
using UnityEngine.Events;

namespace WakeTheWalls.Interaction
{
    /// <summary>
    /// Anything the user can tap in AR. Needs a collider on the same object.
    /// Hook up responses in the inspector or subscribe to <see cref="Tapped"/> from code.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [SerializeField] bool isInteractable = true;
        [SerializeField] UnityEvent tapped = new UnityEvent();

        /// <summary>Invoked when the user taps this object.</summary>
        public UnityEvent Tapped => tapped;

        /// <summary>When false, taps are ignored.</summary>
        public bool IsInteractable
        {
            get => isInteractable;
            set => isInteractable = value;
        }

        /// <summary>Called by the tap interactor when a tap ray hits this object.</summary>
        public virtual void Tap()
        {
            if (!isInteractable) return;
            tapped.Invoke();
        }
    }
}
