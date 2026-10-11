using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Core;

namespace WakeTheWalls.Interaction
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] bool isInteractable = true;
        [SerializeField] UnityEvent tapped = new UnityEvent();

        public UnityEvent Tapped => tapped;

        public bool IsInteractable
        {
            get => isInteractable;
            set => isInteractable = value;
        }

        public virtual void Tap()
        {
            if (!isInteractable) return;
            tapped.Invoke();
            GameEvents.RaiseInteractableTapped();
        }
    }
}
