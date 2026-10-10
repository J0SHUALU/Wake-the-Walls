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
            if (IsVisible) return;
            IsVisible = true;
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
            SetInteractive(false);
            OnHiding();
            FadeTo(0f);
        }

        /// <summary>Shows or hides the screen with no fade. Use only for the starting state.</summary>
        public void SetInstant(bool visible)
        {
            if (fade != null) StopCoroutine(fade);
            fade = null;
            IsVisible = visible;
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
