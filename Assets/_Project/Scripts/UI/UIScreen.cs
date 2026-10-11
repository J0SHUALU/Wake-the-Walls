using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.UI
{
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

        public bool IsVisible { get; private set; }

        protected CanvasGroup Group => group != null ? group : group = GetComponent<CanvasGroup>();

        protected virtual void Awake()
        {
            SetInstant(visibleOnStart);
        }

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

        public void SetDimmed(bool dim, float dimAlpha = 0f)
        {
            if (!IsVisible || dim == dimmed) return;
            dimmed = dim;
            SetInteractive(!dim);
            FadeTo(dim ? dimAlpha : 1f);
        }

        public void SetInstant(bool visible)
        {
            if (fade != null) StopCoroutine(fade);
            fade = null;
            IsVisible = visible;
            dimmed = false;
            Group.alpha = visible ? 1f : 0f;
            SetInteractive(visible);
        }

        protected virtual void OnShowing() { }

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
