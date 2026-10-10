using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// A tap area over a stack of bangles on Mural 5. Tapping slides the bangles down the arm with a
    /// clink, sends a pulse of light out along the rays from them, then eases them back into place.
    /// </summary>
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class BangleTapInteraction : MonoBehaviour
    {
        [SerializeField] Mural05Experience experience;
        [Tooltip("Bangles element from the layer manifest.")]
        [SerializeField] string element;
        [Tooltip("Tap area, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.6f, 0.3f, 0.12f, 0.15f);
        [Tooltip("How far the bangles slide, in metres. Point it down the arm.")]
        [SerializeField] Vector2 slide = new Vector2(-0.03f, -0.08f);
        [SerializeField, Range(0.1f, 1f)] float slideSeconds = 0.3f;
        [SerializeField, Range(0f, 2f)] float holdSeconds = 0.4f;
        [SerializeField, Range(0.3f, 2f)] float returnSeconds = 0.9f;
        [SerializeField, Range(0.8f, 4f)] float pulseSeconds = 1.8f;
        [Tooltip("The clink. When empty, the app's usual tap sound still plays.")]
        [SerializeField] AudioClip sound;
        [SerializeField] string interactionId = "bangles";
        [SerializeField] UnityEvent<string> used = new UnityEvent<string>();

        Coroutine sliding;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            TapArea.Fit(GetComponent<BoxCollider>(), area, experience.Rig.MuralSize);
        }

        void OnDisable() => sliding = null;

        void OnTapped()
        {
            if (!experience.IsRunning || sliding != null) return;
            if (!experience.Rig.TryGetLayer(element, out MuralLayer layer)) return;
            sliding = StartCoroutine(Slide(layer));
            if (sound != null) AudioSource.PlayClipAtPoint(sound, transform.position);
            var from = new Vector2(layer.Entry.pivotX, layer.Entry.pivotY);
            foreach (MaskGlow ray in experience.Rays) if (ray != null) ray.Flow(from, pulseSeconds);
            used.Invoke(interactionId);
        }

        IEnumerator Slide(MuralLayer layer)
        {
            Transform pivot = layer.Pivot;
            Vector3 rest = new Vector3(layer.PivotLocal.x, layer.PivotLocal.y, 0f);
            Vector3 down = rest + (Vector3)slide;
            yield return Tween.Move(pivot, down, slideSeconds, Tween.EaseOutCubic);
            yield return new WaitForSeconds(holdSeconds);
            yield return Tween.Move(pivot, rest, returnSeconds, Tween.EaseInOutCubic);
            sliding = null;
        }
    }
}
