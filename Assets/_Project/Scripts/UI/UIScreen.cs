using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// Base for every screen and panel. Fades its CanvasGroup in and out with an eased tween,
    /// and only takes taps while it is visible, so hidden screens never block the AR view.
    /// Keep the GameObject active; visibility is controlled by the CanvasGroup alpha.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour
    {
        [Tooltip("Seconds a fade takes. Keep it between 0.6 and 1.5.")]
        [SerializeField] float fadeDuration = 0.6f;

        [Tooltip("Show this screen straight away when the scene starts.")]
        [SerializeField] bool visibleOnStart;

        CanvasGroup group;
        Coroutine fade;
        bool dimmed;

        /// <summary>True while the screen is shown or fading in.</summary>
        public bool IsVisible { get; private set; }

        /// <summary>The CanvasGroup this screen fades.</summary>
        protected CanvasGroup Group => group != null ? group : group = GetComponent<CanvasGroup>();

        protected virtual void Awake()
        {
            SetInstant(visibleOnStart);
        }

        /// <summary>Fades the screen in and lets it take taps.</summary>
        [ContextMenu("Show")]
        public void Show()
        {
            if (IsVisible && !dimmed) return;
            IsVisible = true;
            dimmed = false;
            SetInteractive(true);
            OnShowing();
            FadeTo(1f);
        }

        /// <summary>Stops the screen taking taps and fades it out.</summary>
        [ContextMenu("Hide")]
        public void Hide()
        {
            if (!IsVisible) return;
            IsVisible = false;
            dimmed = false;
            SetInteractive(false);
            OnHiding();
            FadeTo(0f);
        }

        /// <summary>
        /// Fades a visible screen down to <paramref name="dimAlpha"/> and stops it taking taps,
        /// or brings it back. Used to dim the HUD while the mural is paused.
        /// </summary>
        public void SetDimmed(bool dim, float dimAlpha = 0f)
        {
            if (!IsVisible || dim == dimmed) return;
            dimmed = dim;
            SetInteractive(!dim);
            FadeTo(dim ? dimAlpha : 1f);
        }

        /// <summary>Shows or hides the screen with no fade. Use only for the starting state.</summary>
        public void SetInstant(bool visible)
        {
            if (fade != null) StopCoroutine(fade);
            fade = null;
            IsVisible = visible;
            dimmed = false;
            Group.alpha = visible ? 1f : 0f;
            SetInteractive(visible);
        }

        /// <summary>Called just before the screen starts fading in. Fill in content here.</summary>
        protected virtual void OnShowing() { }

        /// <summary>Called just before the screen starts fading out.</summary>
        protected virtual void OnHiding() { }

        void FadeTo(float target)
        {
            if (fade != null) StopCoroutine(fade);
            if (!isActiveAndEnabled)
            {
                Group.alpha = target;
                fade = null;
                return;
            }
            fade = StartCoroutine(Fade(target));
        }

        IEnumerator Fade(float target)
        {
            yield return Tween.Value(Group.alpha, target, a => Group.alpha = a, fadeDuration, Tween.EaseInOutCubic);
            fade = null;
        }

        void SetInteractive(bool on)
        {
            Group.interactable = on;
            Group.blocksRaycasts = on;
        }
    }
}
