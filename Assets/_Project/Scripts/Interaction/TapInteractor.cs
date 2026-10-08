using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace WakeTheWalls.Interaction
{
    /// <summary>
    /// Turns a tap on the screen into a ray from the AR camera. If the ray hits an
    /// <see cref="Interactable"/>, its <see cref="Interactable.Tap"/> is called.
    /// Taps that land on UI are left to the UI.
    /// </summary>
    public class TapInteractor : MonoBehaviour
    {
        [Tooltip("Camera the tap ray starts from. Uses the main camera when empty.")]
        [SerializeField] Camera arCamera;

        [Tooltip("How far into the scene a tap can reach, in metres.")]
        [SerializeField] float maxDistance = 20f;

        [SerializeField] LayerMask layers = ~0;

        InputAction tapAction;
        readonly List<RaycastResult> uiHits = new List<RaycastResult>();

        void Awake()
        {
            tapAction = new InputAction("Tap", InputActionType.Button, "<Pointer>/press", "tap");
        }

        void OnEnable()
        {
            tapAction.performed += OnTap;
            tapAction.Enable();
        }

        void OnDisable()
        {
            tapAction.performed -= OnTap;
            tapAction.Disable();
        }

        void OnDestroy() => tapAction.Dispose();

        void OnTap(InputAction.CallbackContext context)
        {
            if (Pointer.current == null) return;

            Vector2 screenPosition = Pointer.current.position.ReadValue();
            if (IsOverUI(screenPosition)) return;

            var cam = arCamera != null ? arCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out var hit, maxDistance, layers, QueryTriggerInteraction.Collide))
            {
                var interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null) interactable.Tap();
            }
        }

        bool IsOverUI(Vector2 screenPosition)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            var pointerData = new PointerEventData(eventSystem) { position = screenPosition };
            uiHits.Clear();
            eventSystem.RaycastAll(pointerData, uiHits);
            return uiHits.Count > 0;
        }
    }
}
